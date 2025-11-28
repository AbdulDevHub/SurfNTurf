using UnityEngine;

public class HoverAndClickQuickOutline : MonoBehaviour
{
    public Vector2 hotspot = Vector2.zero;

    private Outline lastHovered;
    private CursorManager cursorManager;

    void Start()
    {
        cursorManager = Object.FindFirstObjectByType<CursorManager>();

        Outline[] outlines = Object.FindObjectsByType<Outline>(FindObjectsSortMode.None);
        foreach (var o in outlines)
        {
            o.enabled = false;
        }
    }

    void Update()
    {
        if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
            return;

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
                    lastHovered.enabled = true;
                }

                cursorManager.SetPointerCursor();

                if (Input.GetMouseButtonDown(0))
                {
                    Debug.Log("Clicked: " + hit.collider.gameObject.name);
                }

                return;
            }
        }

        ResetLastHovered();
        cursorManager.SetDefaultCursor();
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
