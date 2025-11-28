using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;

public class CursorUISelector : MonoBehaviour
{
    private CursorManager cursorManager;
    private bool isPointerCursor = false;

    EventSystem eventSystem;
    GraphicRaycaster raycaster;

    void Start()
    {
        cursorManager = FindObjectOfType<CursorManager>();

        // Find the raycaster on your Canvas
        raycaster = FindObjectOfType<Canvas>().GetComponent<GraphicRaycaster>();
        eventSystem = EventSystem.current;
    }

    void Update()
    {
        // Prepare UI raycast
        PointerEventData pointerData = new PointerEventData(eventSystem);
        pointerData.position = Input.mousePosition;

        List<RaycastResult> results = new List<RaycastResult>();
        raycaster.Raycast(pointerData, results);

        // Check if ANY UI Button is under the cursor
        bool overButton = false;

        foreach (var hit in results)
        {
            if (hit.gameObject.GetComponentInParent<Button>() != null)
            {
                overButton = true;
                break;
            }
        }

        // Change cursor if needed
        if (overButton && !isPointerCursor)
        {
            cursorManager.SetPointerCursor();
            isPointerCursor = true;
        }
        else if (!overButton && isPointerCursor)
        {
            cursorManager.SetDefaultCursor();
            isPointerCursor = false;
        }
    }
}
