using UnityEngine;
using UnityEngine.InputSystem; // Required for the new Input System

public class CursorFollow : MonoBehaviour
{
    void Update()
    {
        // Get mouse position using the new Input System
        Vector2 mousePos2D = Mouse.current.position.ReadValue();
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(new Vector3(mousePos2D.x, mousePos2D.y, 0));
        mousePos.z = 0; // Ensure 2D positioning
        transform.position = mousePos;
    }
}