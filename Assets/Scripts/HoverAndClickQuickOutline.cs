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
            // Priority 1: Outline objects
            Outline outline = hit.collider.GetComponent<Outline>();
            if (outline != null)
            {
                HandleOutlineHover(outline);
                cursorManager.SetPointerCursor();
                return;
            }

            // Priority 2: TowerSpot
            TowerSpotController spot = hit.collider.GetComponentInParent<TowerSpotController>();
            if (spot != null)
            {
                cursorManager.SetPointerCursor();
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

    void HandleOutlineHover(Outline outline)
    {
        if (lastHovered != outline)
        {
            ResetLastHovered();
            lastHovered = outline;
            lastHovered.enabled = true;
        }
    }
}
