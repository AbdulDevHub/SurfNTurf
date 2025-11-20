using UnityEngine;

public class HoverAndClickQuickOutline : MonoBehaviour
{
    [Header("Cursor Textures")]
    private Texture2D hoverCursor;
    private Texture2D defaultCursor;
    private Vector2 hotspot = Vector2.zero;

    private Outline lastHovered;

    void Start()
    {
        // Disable all Outline components in the scene initially
        Outline[] outlines = FindObjectsOfType<Outline>();
        foreach (var o in outlines)
        {
            o.enabled = false;
        }
    }

    void Update()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Outline outline = hit.collider.GetComponent<Outline>();

            if (outline != null)
            {
                if (lastHovered != outline)
                {
                    ResetLastHovered();
                    lastHovered = outline;
                    lastHovered.enabled = true; // enable outline on hover
                }

                Cursor.SetCursor(hoverCursor, hotspot, CursorMode.Auto);

                if (Input.GetMouseButtonDown(0))
                {
                    Debug.Log("Clicked: " + hit.collider.gameObject.name);
                }

                return; // exit to prevent resetting cursor/outline
            }
        }

        ResetLastHovered();
        Cursor.SetCursor(defaultCursor, hotspot, CursorMode.Auto);
    }

    private void ResetLastHovered()
    {
        if (lastHovered != null)
        {
            lastHovered.enabled = false;
            lastHovered = null;
        }
    }
}
