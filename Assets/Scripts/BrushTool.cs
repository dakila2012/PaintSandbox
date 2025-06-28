using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class BrushTool : MonoBehaviour
{
    [SerializeField] private GameObject pixelPrefab;
    [SerializeField] private Transform canvasParent;
    [SerializeField] private float gridSize = 0.03125f; // 1/32 for 512x512 grid with camera size=1
    [SerializeField] private Button toggleBrushButton;
    [SerializeField] private Button redButton;
    [SerializeField] private Button blueButton;
    [SerializeField] private Button greenButton;
    private bool isBrushActive = false;
    private Color brushColor = Color.red;
    private Vector3 lastDrawnPos;

    void Start()
    {
        // Clear existing listeners to prevent duplicates
        if (toggleBrushButton != null) {
            toggleBrushButton.onClick.RemoveAllListeners();
            toggleBrushButton.onClick.AddListener(() => { ToggleBrush(); Debug.Log("ToggleBrushButton clicked"); });
        }
        if (redButton != null) {
            redButton.onClick.RemoveAllListeners();
            redButton.onClick.AddListener(() => { SetColorRed(); Debug.Log("RedButton clicked"); });
        }
        if (blueButton != null) {
            blueButton.onClick.RemoveAllListeners();
            blueButton.onClick.AddListener(() => { SetColorBlue(); Debug.Log("BlueButton clicked"); });
        }
        if (greenButton != null) {
            greenButton.onClick.RemoveAllListeners();
            greenButton.onClick.AddListener(() => { SetColorGreen(); Debug.Log("GreenButton clicked"); });
        }
    }

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.tKey.wasPressedThisFrame) {
            ToggleBrush();
            Debug.Log("T key pressed");
        }
        if (isBrushActive && Mouse.current != null && Mouse.current.leftButton.isPressed)
        {
            Debug.Log("Mouse left button held");
            Vector2 mousePos = Mouse.current.position.ReadValue();
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(mousePos);
            float snappedX = Mathf.Round(worldPos.x / gridSize) * gridSize;
            float snappedY = Mathf.Round(worldPos.y / gridSize) * gridSize;
            Vector3 snappedPos = new Vector3(snappedX, snappedY, 0);
            if (snappedPos != lastDrawnPos) // Prevent duplicate pixels
            {
                Debug.Log($"Attempting to draw at {snappedPos}");
                GameObject pixel = Instantiate(pixelPrefab, snappedPos, Quaternion.identity, canvasParent);
                SpriteRenderer sr = pixel.GetComponent<SpriteRenderer>();
                if (sr != null) sr.color = brushColor;
                Debug.Log($"Drew pixel at {snappedPos}");
                lastDrawnPos = snappedPos;
            }
        }
    }

    public void ToggleBrush()
    {
        isBrushActive = !isBrushActive;
        Debug.Log($"Brush {(isBrushActive ? "enabled" : "disabled")}");
    }

    public void SetColorRed() { brushColor = Color.red; Debug.Log("Brush color set to red"); }
    public void SetColorBlue() { brushColor = Color.blue; Debug.Log("Brush color set to blue"); }
    public void SetColorGreen() { brushColor = Color.green; Debug.Log("Brush color set to green"); }
}