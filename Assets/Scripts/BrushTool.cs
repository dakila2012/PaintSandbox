using UnityEngine;

public class BrushTool : MonoBehaviour
{
    [SerializeField] private GameObject pixelPrefab; // Prefab for pixel_red.png
    [SerializeField] private Transform canvasParent; // Parent for drawn pixels
    [SerializeField] private float gridSize = 1f;   // Grid cell size (1 unit = 1 pixel)
    private bool isBrushActive = false;
    private Color brushColor = Color.red;           // Default color

    void Update()
    {
        if (isBrushActive && Input.GetMouseButtonDown(0)) // Left-click to draw
        {
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            // Snap to grid
            float snappedX = Mathf.Round(worldPos.x / gridSize) * gridSize;
            float snappedY = Mathf.Round(worldPos.y / gridSize) * gridSize;
            Vector3 snappedPos = new Vector3(snappedX, snappedY, 0);

            // Instantiate pixel prefab
            GameObject pixel = Instantiate(pixelPrefab, snappedPos, Quaternion.identity, canvasParent);
            SpriteRenderer sr = pixel.GetComponent<SpriteRenderer>();
            if (sr != null) sr.color = brushColor;

            Debug.Log($"Drew pixel at {snappedPos}");
        }
    }

    public void ToggleBrush()
    {
        isBrushActive = !isBrushActive;
        Debug.Log($"Brush {(isBrushActive ? "enabled" : "disabled")}");
    }

    public void SetColorRed()
    {
        brushColor = Color.red;
        Debug.Log("Brush color set to red");
    }

    public void SetColorBlue() // Example for another color
    {
        brushColor = Color.blue;
        Debug.Log("Brush color set to blue");
    }
}