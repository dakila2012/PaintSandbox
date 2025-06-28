using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class BrushTool : MonoBehaviour
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
    private const float minSize = 0.03125f;
    private const float maxSize = 0.25f;
    private float currentSize = 0.03125f;
    private InputAction mousePositionAction;
    private InputAction mouseLeftClickAction;
    private bool isMouseHeld = false;

    void Awake()
    {
        BrushTool[] instances = FindObjectsOfType<BrushTool>();
        if (instances.Length > 1)
        {
            Debug.LogWarning($"Multiple BrushTool instances found: {instances.Length}. Destroying extras.");
            for (int i = 1; i < instances.Length; i++)
            {
                Destroy(instances[i].gameObject);
            }
        }
    }

    void Start()
    {
        Debug.Log($"BrushTool Start on {gameObject.name}, instance ID: {GetInstanceID()}");
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
                Debug.Log($"Brush/Erase size set to {currentSize}");
            });
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
                Debug.Log($"Brush {(isBrushActive ? "enabled" : "disabled")}");
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

        // Set up Input System actions
        mousePositionAction = new InputAction("MousePosition", InputActionType.Value, "<Mouse>/position");
        mouseLeftClickAction = new InputAction("MouseLeftClick", InputActionType.Button, "<Mouse>/leftButton");
        mouseLeftClickAction.performed += ctx => { isMouseHeld = true; HandleInput(); };
        mouseLeftClickAction.canceled += ctx => { isMouseHeld = false; lastErasedPos = Vector3.zero; };
        mousePositionAction.performed += ctx => { if (isMouseHeld) HandleInput(); };
        mousePositionAction.Enable();
        mouseLeftClickAction.Enable();
    }

    void OnDestroy()
    {
        mousePositionAction.Disable();
        mouseLeftClickAction.Disable();
    }

    private void HandleInput()
    {
        Vector2 mousePos = mousePositionAction.ReadValue<Vector2>();
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(mousePos);
        worldPos.z = 0;
        if (isBrushActive && !eraseMode)
        {
            if (Vector3.Distance(worldPos, lastDrawnPos) > currentSize * 0.5f)
            {
                Debug.Log($"Drawing pixel at {worldPos} with size {currentSize}");
                DrawPixel(worldPos);
                lastDrawnPos = worldPos;
            }
        }
        else if (eraseMode)
        {
            if (lastErasedPos != Vector3.zero)
            {
                // Interpolate between lastErasedPos and worldPos
                float distance = Vector3.Distance(lastErasedPos, worldPos);
                if (distance > 0)
                {
                    int steps = Mathf.CeilToInt(distance / (currentSize * 0.5f));
                    for (int i = 0; i <= steps; i++)
                    {
                        Vector3 interpPos = Vector3.Lerp(lastErasedPos, worldPos, i / (float)steps);
                        if (Time.time - lastLogTime >= 0.5f)
                        {
                            Debug.Log($"Erasing at {interpPos} with size {currentSize}");
                        }
                        ErasePixel(interpPos);
                    }
                }
            }
            else
            {
                if (Time.time - lastLogTime >= 0.5f)
                {
                    Debug.Log($"Erasing at {worldPos} with size {currentSize}");
                }
                ErasePixel(worldPos);
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
        Debug.Log($"Erase mode {(eraseMode ? "enabled" : "disabled")}");
    }

    public void ClearCanvas()
    {
        foreach (Transform child in canvasParent)
        {
            Destroy(child.gameObject);
        }
        Debug.Log("Canvas cleared, all pixels destroyed");
    }

    private void DrawPixel(Vector3 position)
    {
        if (pixelPrefab == null || canvasParent == null)
        {
            Debug.LogError($"DrawPixel failed: pixelPrefab={(pixelPrefab == null ? "null" : "set")}, canvasParent={(canvasParent == null ? "null" : canvasParent.name)}");
            return;
        }
        GameObject pixel = Instantiate(pixelPrefab, position, Quaternion.identity, canvasParent);
        SpriteRenderer sr = pixel.GetComponent<SpriteRenderer>();
        BoxCollider2D collider = pixel.GetComponent<BoxCollider2D>();
        if (sr != null && collider != null)
        {
            sr.color = brushColor;
            pixel.layer = LayerMask.NameToLayer("Default");
            float scale = currentSize * 8f; // Adjust scale for visibility
            pixel.transform.localScale = Vector3.one * scale;
            collider.size = new Vector2(0.2f, 0.2f); // Larger fixed collider size
            collider.enabled = true;
            Debug.Log($"Pixel instantiated at {position}, Scale={scale}, Layer={LayerMask.LayerToName(pixel.layer)}, Parent={pixel.transform.parent?.name}, ColliderEnabled={collider.enabled}");
        }
        else
        {
            Debug.LogError($"Pixel at {position} missing SpriteRenderer or BoxCollider2D");
        }
    }

    private void ErasePixel(Vector3 position)
    {
        float eraseRadius = currentSize * 12f; // Larger radius to match brush scale
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
                Destroy(hit.gameObject);
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
    }

    public void SetColorRed() { brushColor = Color.red; eraseMode = false; Debug.Log("Brush color set to red"); }
    public void SetColorBlue() { brushColor = Color.blue; eraseMode = false; Debug.Log("Brush color set to blue"); }
    public void SetColorGreen() { brushColor = Color.green; eraseMode = false; Debug.Log("Brush color set to green"); }
}