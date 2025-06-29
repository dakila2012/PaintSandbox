using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using ExitGames.Client.Photon;

public class BrushTool : MonoBehaviourPunCallbacks
{
    [SerializeField] private GameObject pixelPrefab;
    [SerializeField] private Transform canvasParent;
    [SerializeField] private Button toggleBrushButton;
    [SerializeField] private Button redButton;
    [SerializeField] private Button blueButton;
    [SerializeField] private Button greenButton;
    [SerializeField] private Button eraseButton;
    [SerializeField] private Button clearButton;
    [SerializeField] private Slider sizeSlider;
    [SerializeField] private Button debugPixelsButton;
    private bool isBrushActive = false;
    private bool eraseMode = false;
    private Color brushColor = Color.red;
    private Vector3 lastDrawnPos;
    private Vector3 lastErasedPos;
    private float lastLogTime = 0f;
    private float lastDrawTime = 0f;
    private int pixelCount = 0;
    private Queue<GameObject> pixelQueue = new Queue<GameObject>();
    private const float minSize = 0.03125f;
    private const float maxSize = 0.25f;
    private float currentSize = 0.03125f;
    private const float drawCooldown = 0.01f; // Your adjusted value
    private const int viewIdLimit = 999;
    private InputAction mousePositionAction;
    private InputAction mouseLeftClickAction;
    private bool isMouseHeld = false;
    private PhotonView photonView;
    private bool isInitialized = false;
    private static BrushTool instance; // Singleton instance

    void Awake()
    {
        photonView = GetComponent<PhotonView>();
        if (photonView == null)
        {
            Debug.LogError("PhotonView component missing on BrushTool GameObject");
        }

        // Singleton pattern
        if (instance != null && instance != this)
        {
            Debug.LogWarning($"Another BrushTool instance found on {gameObject.name}. Destroying this instance.");
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject); // Persist across scenes

        BrushTool[] instances = FindObjectsOfType<BrushTool>();
        if (instances.Length > 1)
        {
            Debug.LogWarning($"Multiple BrushTool instances found: {instances.Length}. Keeping only {gameObject.name}.");
            for (int i = 0; i < instances.Length; i++)
            {
                if (instances[i] != this)
                {
                    Destroy(instances[i].gameObject);
                }
            }
        }
    }

    void Start()
    {
        if (!PhotonNetwork.IsConnected)
        {
            Debug.LogWarning("Not connected to Photon. Multiplayer features disabled.");
        }

        Debug.Log($"BrushTool Start on {gameObject.name}, instance ID: {GetInstanceID()}");
        Debug.Log($"sizeSlider is {(sizeSlider != null ? $"assigned to {sizeSlider.name}" : "null")}");

        if (redButton != null) redButton.gameObject.SetActive(isBrushActive);
        if (blueButton != null) blueButton.gameObject.SetActive(isBrushActive);
        if (greenButton != null) greenButton.gameObject.SetActive(isBrushActive);

        if (debugPixelsButton != null)
        {
            debugPixelsButton.onClick.RemoveAllListeners();
            debugPixelsButton.onClick.AddListener(() => { DebugCanvasPixels(); Debug.Log("DebugPixelsButton clicked"); });
        }

        if (sizeSlider != null)
        {
            sizeSlider.minValue = minSize;
            sizeSlider.maxValue = maxSize;
            sizeSlider.value = currentSize;
            sizeSlider.onValueChanged.RemoveAllListeners();
            sizeSlider.onValueChanged.AddListener((value) =>
            {
                currentSize = value;
                UpdatePlayerProperties();
                Debug.Log($"Brush/Erase size set to {currentSize} for Player {PhotonNetwork.LocalPlayer.ActorNumber}");
            });
        }
        else
        {
            Debug.LogError("sizeSlider is not assigned in the Inspector");
        }

        if (toggleBrushButton != null)
        {
            toggleBrushButton.onClick.RemoveAllListeners();
            toggleBrushButton.onClick.AddListener(() =>
            {
                isBrushActive = !isBrushActive;
                if (eraseMode && isBrushActive) eraseMode = false;
                if (redButton != null) redButton.gameObject.SetActive(isBrushActive);
                if (blueButton != null) blueButton.gameObject.SetActive(isBrushActive);
                if (greenButton != null) greenButton.gameObject.SetActive(isBrushActive);
                Debug.Log($"Brush {(isBrushActive ? "enabled" : "disabled")} for Player {PhotonNetwork.LocalPlayer.ActorNumber}");
            });
        }
        if (redButton != null)
        {
            redButton.onClick.RemoveAllListeners();
            redButton.onClick.AddListener(() => { SetColorRed(); Debug.Log($"RedButton clicked, button: {redButton.name}"); });
        }
        if (blueButton != null)
        {
            blueButton.onClick.RemoveAllListeners();
            blueButton.onClick.AddListener(() => { SetColorBlue(); Debug.Log($"BlueButton clicked, button: {blueButton.name}"); });
        }
        if (greenButton != null)
        {
            greenButton.onClick.RemoveAllListeners();
            greenButton.onClick.AddListener(() => { SetColorGreen(); Debug.Log($"GreenButton clicked, button: {greenButton.name}"); });
        }
        if (eraseButton != null)
        {
            eraseButton.onClick.RemoveAllListeners();
            eraseButton.onClick.AddListener(() => { ToggleErase(); Debug.Log($"EraseButton clicked, button: {eraseButton.name}"); });
        }
        if (clearButton != null)
        {
            clearButton.onClick.RemoveAllListeners();
            clearButton.onClick.AddListener(() => { ClearCanvas(); Debug.Log($"ClearButton clicked, button: {clearButton.name}"); });
        }

        // Set initial player properties
        UpdatePlayerProperties();

        // Set up Input System actions
        mousePositionAction = new InputAction("MousePosition", InputActionType.Value, "<Mouse>/position");
        mouseLeftClickAction = new InputAction("MouseLeftClick", InputActionType.Button, "<Mouse>/leftButton");
        mouseLeftClickAction.performed += ctx => { if (isInitialized) { isMouseHeld = true; HandleInput(); } };
        mouseLeftClickAction.canceled += ctx => { isMouseHeld = false; lastErasedPos = Vector3.zero; };
        mousePositionAction.performed += ctx => { if (isInitialized && isMouseHeld && Camera.main != null) HandleInput(); };
        mousePositionAction.Enable();
        mouseLeftClickAction.Enable();

        isInitialized = true;
    }

    void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
        if (mousePositionAction != null) mousePositionAction.Disable();
        if (mouseLeftClickAction != null) mouseLeftClickAction.Disable();
    }

    private void UpdatePlayerProperties()
    {
        if (PhotonNetwork.IsConnected)
        {
            Hashtable props = new Hashtable
            {
                { "BrushColor", new float[] { brushColor.r, brushColor.g, brushColor.b, brushColor.a } },
                { "BrushSize", currentSize }
            };
            PhotonNetwork.LocalPlayer.SetCustomProperties(props);
        }
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        if (changedProps.ContainsKey("BrushColor") || changedProps.ContainsKey("BrushSize"))
        {
            Debug.Log($"Player {targetPlayer.ActorNumber} updated properties: Color={changedProps["BrushColor"]}, Size={changedProps["BrushSize"]}");
        }
    }

    private void HandleInput()
    {
        if (Camera.main == null)
        {
            Debug.LogError("Main Camera not found. Ensure a camera is tagged 'MainCamera'.");
            return;
        }
        if (mousePositionAction == null)
        {
            Debug.LogError("mousePositionAction is null. Input System not initialized correctly.");
            return;
        }

        Vector2 mousePos = Vector2.zero;
        try
        {
            mousePos = mousePositionAction.ReadValue<Vector2>();
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Failed to read mouse position: {ex.Message}");
            return;
        }

        Vector3 worldPos = Camera.main.ScreenToWorldPoint(mousePos);
        worldPos.z = 0;
        if (isBrushActive && !eraseMode)
        {
            if (Time.time - lastDrawTime >= drawCooldown && Vector3.Distance(worldPos, lastDrawnPos) > currentSize * 1.5f)
            {
                if (PhotonNetwork.IsConnected && pixelCount > viewIdLimit)
                {
                    Debug.Log($"Pixel count exceeds limit ({pixelCount}/{viewIdLimit}). Deleting oldest pixel.");
                    if (pixelQueue.Count > 0)
                    {
                        GameObject oldPixel = pixelQueue.Dequeue();
                        if (oldPixel != null)
                        {
                            if (PhotonNetwork.IsConnected)
                            {
                                PhotonNetwork.Destroy(oldPixel);
                            }
                            else
                            {
                                Destroy(oldPixel);
                            }
                            pixelCount--;
                            Debug.Log($"Deleted oldest pixel at {oldPixel.transform.position}. Pixel count: {pixelCount}");
                        }
                    }
                }
                Debug.Log($"Drawing pixel at {worldPos} with size {currentSize} by Player {PhotonNetwork.LocalPlayer.ActorNumber}");
                if (PhotonNetwork.IsConnected && photonView != null)
                {
                    try
                    {
                        photonView.RPC("DrawPixelRPC", RpcTarget.Others, worldPos, currentSize, brushColor.r, brushColor.g, brushColor.b, brushColor.a, PhotonNetwork.LocalPlayer.ActorNumber);
                        DrawPixelRPC(worldPos, currentSize, brushColor.r, brushColor.g, brushColor.b, brushColor.a, PhotonNetwork.LocalPlayer.ActorNumber); // Local draw
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogError($"Failed to send DrawPixelRPC: {ex.Message}");
                    }
                }
                else
                {
                    DrawPixelRPC(worldPos, currentSize, brushColor.r, brushColor.g, brushColor.b, brushColor.a, -1);
                }
                lastDrawnPos = worldPos;
                lastDrawTime = Time.time;
            }
        }
        else if (eraseMode)
        {
            if (lastErasedPos != Vector3.zero)
            {
                float distance = Vector3.Distance(lastErasedPos, worldPos);
                if (distance > 0)
                {
                    int steps = Mathf.CeilToInt(distance / (currentSize * 0.5f));
                    for (int i = 0; i <= steps; i++)
                    {
                        Vector3 interpPos = Vector3.Lerp(lastErasedPos, worldPos, i / (float)steps);
                        if (Time.time - lastLogTime >= 0.5f)
                        {
                            Debug.Log($"Erasing at {interpPos} with size {currentSize} by Player {PhotonNetwork.LocalPlayer.ActorNumber}");
                        }
                        if (PhotonNetwork.IsConnected && photonView != null)
                        {
                            try
                            {
                                photonView.RPC("ErasePixelRPC", RpcTarget.All, interpPos, currentSize);
                            }
                            catch (System.Exception ex)
                            {
                                Debug.LogError($"Failed to send ErasePixelRPC: {ex.Message}");
                            }
                        }
                        else
                        {
                            ErasePixelRPC(interpPos, currentSize);
                        }
                    }
                }
            }
            else
            {
                if (Time.time - lastLogTime >= 0.5f)
                {
                    Debug.Log($"Erasing at {worldPos} with size {currentSize} by Player {PhotonNetwork.LocalPlayer.ActorNumber}");
                }
                if (PhotonNetwork.IsConnected && photonView != null)
                {
                    try
                    {
                        photonView.RPC("ErasePixelRPC", RpcTarget.All, worldPos, currentSize);
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogError($"Failed to send ErasePixelRPC: {ex.Message}");
                    }
                }
                else
                {
                    ErasePixelRPC(worldPos, currentSize);
                }
            }
            lastErasedPos = worldPos;
        }
    }

    public void ToggleErase()
    {
        eraseMode = !eraseMode;
        isBrushActive = false;
        if (redButton != null) redButton.gameObject.SetActive(isBrushActive);
        if (blueButton != null) blueButton.gameObject.SetActive(isBrushActive);
        if (greenButton != null) greenButton.gameObject.SetActive(isBrushActive);
        Debug.Log($"Erase mode {(eraseMode ? "enabled" : "disabled")} for Player {PhotonNetwork.LocalPlayer.ActorNumber}");
    }

    public void ClearCanvas()
    {
        if (PhotonNetwork.IsConnected && photonView != null)
        {
            try
            {
                photonView.RPC("ClearCanvasRPC", RpcTarget.All);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Failed to send ClearCanvasRPC: {ex.Message}");
            }
        }
        else
        {
            ClearCanvasRPC();
        }
    }

    [PunRPC]
    private void DrawPixelRPC(Vector3 position, float size, float r, float g, float b, float a, int actorNumber)
    {
        if (pixelPrefab == null || canvasParent == null)
        {
            Debug.LogError($"DrawPixel failed: pixelPrefab={(pixelPrefab == null ? "null" : "set")}, canvasParent={(canvasParent == null ? "null" : canvasParent.name)}");
            return;
        }
        GameObject pixel = PhotonNetwork.IsConnected ? PhotonNetwork.Instantiate("PixelRed", position, Quaternion.identity) : Instantiate(pixelPrefab, position, Quaternion.identity, canvasParent);
        pixel.transform.SetParent(canvasParent);
        SpriteRenderer sr = pixel.GetComponent<SpriteRenderer>();
        BoxCollider2D collider = pixel.GetComponent<BoxCollider2D>();
        if (sr != null && collider != null)
        {
            sr.color = new Color(r, g, b, a);
            pixel.layer = LayerMask.NameToLayer("Default");
            float scale = size * 8f;
            pixel.transform.localScale = Vector3.one * scale;
            collider.size = new Vector2(0.2f, 0.2f);
            collider.enabled = true;
            pixelQueue.Enqueue(pixel);
            pixelCount++;
            if (Time.time - lastLogTime >= 0.5f)
            {
                Debug.Log($"Pixel instantiated at {position}, Scale={scale}, Color=({r},{g},{b},{a}), Layer={LayerMask.LayerToName(pixel.layer)}, Parent={pixel.transform.parent?.name}, ColliderEnabled={collider.enabled}, Pixel count: {pixelCount}, By Player {actorNumber}");
            }
        }
        else
        {
            Debug.LogError($"Pixel at {position} missing SpriteRenderer or BoxCollider2D");
            if (PhotonNetwork.IsConnected)
            {
                PhotonNetwork.Destroy(pixel);
            }
            else
            {
                Destroy(pixel);
            }
        }
    }

    [PunRPC]
    private void ErasePixelRPC(Vector3 position, float size)
    {
        float eraseRadius = size * 12f;
        int defaultLayer = LayerMask.NameToLayer("Default");
        LayerMask layerMask = 1 << defaultLayer;
        if (Time.time - lastLogTime >= 0.5f)
        {
            Debug.Log($"ErasePixel: position={position}, eraseRadius={eraseRadius}, querying layer: {LayerMask.LayerToName(defaultLayer)}");
            Debug.DrawRay(position, Vector3.up * eraseRadius, Color.red, 1f);
            Debug.DrawRay(position, Vector3.right * eraseRadius, Color.red, 1f);
        }
        Collider2D[] hits = Physics2D.OverlapCircleAll(position, eraseRadius, layerMask);
        if (Time.time - lastLogTime >= 0.5f)
        {
            Debug.Log($"ErasePixel at {position}, found {hits.Length} colliders with eraseRadius {eraseRadius}");
        }
        foreach (var hit in hits)
        {
            if (hit.transform.parent == canvasParent)
            {
                if (Time.time - lastLogTime >= 0.5f)
                {
                    Debug.Log($"Erasing pixel at {hit.transform.position}, Scale={hit.transform.localScale.x}");
                }
                if (PhotonNetwork.IsConnected)
                {
                    PhotonNetwork.Destroy(hit.gameObject);
                }
                else
                {
                    Destroy(hit.gameObject);
                }
                pixelCount--;
                if (pixelCount < 0) pixelCount = 0;
                if (Time.time - lastLogTime >= 0.5f)
                {
                    Debug.Log($"Pixel count after erase: {pixelCount}");
                }
            }
            else if (Time.time - lastLogTime >= 0.5f)
            {
                Debug.Log($"Found collider at {hit.transform.position}, Scale={hit.transform.localScale.x}, Layer={LayerMask.LayerToName(hit.gameObject.layer)}, Parent={hit.transform.parent?.name}, ColliderEnabled={hit.enabled}");
            }
        }
        if (Time.time - lastLogTime >= 0.5f)
        {
            lastLogTime = Time.time;
        }
    }

    [PunRPC]
    private void ClearCanvasRPC()
    {
        foreach (Transform child in canvasParent)
        {
            if (child.GetComponent<SpriteRenderer>() != null)
            {
                if (PhotonNetwork.IsConnected)
                {
                    PhotonNetwork.Destroy(child.gameObject);
                }
                else
                {
                    Destroy(child.gameObject);
                }
            }
        }
        pixelQueue.Clear();
        pixelCount = 0;
        if (Time.time - lastLogTime >= 0.5f)
        {
            Debug.Log("Canvas cleared, all pixels destroyed");
            Debug.Log($"Pixel count reset: {pixelCount}");
            lastLogTime = Time.time;
        }
    }

    public void DebugCanvasPixels()
    {
        int pixelCount = 0;
        foreach (Transform child in canvasParent)
        {
            if (child.GetComponent<SpriteRenderer>() != null)
            {
                pixelCount++;
                Debug.Log($"Pixel found: GameObject={child.gameObject.name}, Position={child.position}, Scale={child.localScale.x}, Layer={LayerMask.LayerToName(child.gameObject.layer)}, Parent={child.transform.parent?.name}");
            }
        }
        Debug.Log($"Total pixels in canvas: {pixelCount}");
        this.pixelCount = pixelCount;
        pixelQueue.Clear();
        foreach (Transform child in canvasParent)
        {
            if (child.GetComponent<SpriteRenderer>() != null)
            {
                pixelQueue.Enqueue(child.gameObject);
            }
        }
    }

    public void SetColorRed()
    {
        brushColor = Color.red;
        eraseMode = false;
        UpdatePlayerProperties();
        Debug.Log($"Brush color set to red for Player {PhotonNetwork.LocalPlayer.ActorNumber}");
    }

    public void SetColorBlue()
    {
        brushColor = Color.blue;
        eraseMode = false;
        UpdatePlayerProperties();
        Debug.Log($"Brush color set to blue for Player {PhotonNetwork.LocalPlayer.ActorNumber}");
    }

    public void SetColorGreen()
    {
        brushColor = Color.green;
        eraseMode = false;
        UpdatePlayerProperties();
        Debug.Log($"Brush color set to green for Player {PhotonNetwork.LocalPlayer.ActorNumber}");
    }
}