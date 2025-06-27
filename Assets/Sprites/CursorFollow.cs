using UnityEngine;

public class CursorFollow : MonoBehaviour
{
    void Update()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0; // Keep 2D.
        transform.position = mousePos;
    }
}