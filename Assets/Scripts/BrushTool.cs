using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class BrushTool : MonoBehaviour
{
    [SerializeField] private GameObject pixelPrefab;
    [SerializeField] private Transform canvasParent;
    [SerializeField] private float gridSize = 0.03125f;
    [SerializeField] private Button toggleBrushButton;
    [SerializeField] private Button redButton;
    [SerializeField] private Button blueButton;
    [SerializeField] private Button greenButton;
    [SerializeField] private Button eraseButton; // New: Erase button
    [SerializeField] private Button clearButton; // New: Clear button
    private bool isBrushActive = false;
    private bool eraseMode = false; // New: Erase mode
    private Color brushColor = Color.red;
    private Vector3 lastDrawnPos;
    private int lastToggleFrame = -1;
    private const float toggleCooldown = 0.2f;

    void Awake()
    {
        BrushTool[] instances = FindObjectsOfType<BrushTool>();
        if (instances.Length > 1) {
            Debug.LogWarning($"Multiple BrushTool instances found: {instances.Length}. Destroying extras.");
            for (int i = 1; i < instances.Length; i++) {
                Destroy(instances[i].gameObject);
            }
        }
    }

    void Start()
    {
        Debug.Log($"BrushTool Start on {gameObject.name}, instance ID: {GetInstanceID()}");
        if (toggleBrushButton != null) {
            toggleBrushButton.onClick.RemoveAllListeners();
            toggleBrushButton.onClick.AddListener(() => {
                if (Time.time - (Time.frameCount - 1) * Time.deltaTime > toggleCooldown && Time.frameCount != lastToggleFrame) {
                    lastToggleFrame = Time.frameCount;
                    ToggleBrush();
                    Debug.Log($"ToggleBrushButton clicked, listener count: {toggleBrushButton.onClick.GetPersistentEventCount()}, button: {toggleBrushButton.name}, stack: {StackTraceUtility.ExtractStackTrace()}");
                } else {
                    Debug.LogWarning($"ToggleBrushButton click ignored due to cooldown or same-frame toggle, frame: {Time.frameCount}");
                }
            });
        }
        if (redButton != null) {
            redButton.onClick.RemoveAllListeners();
            redButton.onClick.AddListener(() => {
                SetColorRed();
                Debug.Log($"RedButton clicked, button: {redButton.name}");
            });
        }
        if (blueButton != null) {
            blueButton.onClick.RemoveAllListeners();
            blueButton.onClick.AddListener(() => {
                SetColorBlue();
                Debug.Log($"BlueButton clicked, button: {blueButton.name}");
            });
        }
        if (greenButton != null) {
            greenButton.onClick.RemoveAllListeners();
            greenButton.onClick.AddListener(() => {
                SetColorGreen();
                Debug.Log($"GreenButton clicked, button: {greenButton.name}");
            });
        }
        if (eraseButton != null) {
            eraseButton.onClick.RemoveAllListeners();
            eraseButton.onClick.AddListener(() => {
                ToggleErase();
                Debug.Log($"EraseButton clicked, button: {eraseButton.name}");
            });
        }
        if (clearButton != null) {
            clearButton.onClick.RemoveAllListeners();
            clearButton.onClick.AddListener(() => {
                ClearCanvas();
                Debug.Log($"ClearButton clicked, button: {clearButton.name}");
            });
        }
    }

    void Update()
    {
        if (isBrushActive && Mouse.current != null && Mouse.current.leftButton.isPressed)
        {
            Vector2 mousePos = Mouse.current.position.ReadValue();
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(mousePos);
            float snappedX = Mathf.Round(worldPos.x / gridSize) * gridSize;
            float snappedY = Mathf.Round(worldPos.y / gridSize) * gridSize;
            Vector3 snappedPos = new Vector3(snappedX, snappedY, 0);
            if (snappedPos != lastDrawnPos)
            {
                if (eraseMode)
                {
                    Debug.Log($"Mouse left button held, attempting to erase at {snappedPos}");
                    ErasePixel(snappedPos);
                }
                else
                {
                    Debug.Log($"Mouse left button held, attempting to draw at {snappedPos}");
                    GameObject pixel = Instantiate(pixelPrefab, snappedPos, Quaternion.identity, canvasParent);
                    SpriteRenderer sr = pixel.GetComponent<SpriteRenderer>();
                    if (sr != null) sr.color = brushColor;
                    Debug.Log($"Drew pixel at {snappedPos}");
                }
                lastDrawnPos = snappedPos;
            }
        }
    }

    public void ToggleBrush()
    {
        isBrushActive = !isBrushActive;
        // Toggle color buttons visibility
        if (redButton != null) {
            redButton.enabled = isBrushActive;
            redButton.interactable = isBrushActive;
        }
        if (blueButton != null) {
            blueButton.enabled = isBrushActive;
            blueButton.interactable = isBrushActive;
        }
        if (greenButton != null) {
            greenButton.enabled = isBrushActive;
            greenButton.interactable = isBrushActive;
        }
        Debug.Log($"Brush {(isBrushActive ? "enabled" : "disabled")}, caller: {StackTraceUtility.ExtractStackTrace()}");
    }

    public void ToggleErase()
    {
        eraseMode = !eraseMode;
        if (eraseMode) isBrushActive = true; // Ensure brush is active for erasing
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

    private void ErasePixel(Vector3 position)
    {
        RaycastHit2D hit = Physics2D.Raycast(position, Vector2.zero);
        if (hit.collider != null && hit.transform.parent == canvasParent)
        {
            Destroy(hit.transform.gameObject);
            Debug.Log($"Erased pixel at {position}");
        }
    }

    public void SetColorRed() { brushColor = Color.red; eraseMode = false; Debug.Log("Brush color set to red"); }
    public void SetColorBlue() { brushColor = Color.blue; eraseMode = false; Debug.Log("Brush color set to blue"); }
    public void SetColorGreen() { brushColor = Color.green; eraseMode = false; Debug.Log("Brush color set to green"); }
}