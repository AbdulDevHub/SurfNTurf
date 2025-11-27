using UnityEngine;

public class TowerSelectorUI : MonoBehaviour
{
    [Header("Tower Prefabs (assign in Inspector)")]
    public GameObject birdPrefab;
    public GameObject bearPrefab;
    public GameObject fishermanPrefab;

    [Header("UI Panel")]
    public GameObject panel;  // Optional: assign in Inspector, but can auto-find

    [HideInInspector] public TowerPlaceholder currentPlaceholder;

    private void Awake()
    {
        // If panel not manually assigned, try to find it in the scene by tag
        if (panel == null)
        {
            panel = GameObject.FindWithTag("towerui");  // Add this tag to your panel in the scene
            if (panel == null)
                Debug.LogWarning("TowerSelectorUI: Could not find the panel! Add tag 'TowerPanel' to it.");
        }
    }

    public void SelectBird()
    {
        Debug.Log("bird was selected");
        SpawnTower(birdPrefab);
        Debug.Log("bird selection finished");
    }

    public void SelectBear()
    {
        SpawnTower(bearPrefab);
    }

    public void SelectFisherman()
    {
        SpawnTower(fishermanPrefab);
    }

    private void SpawnTower(GameObject towerPrefab)
    {
        if (currentPlaceholder == null)
        {
            Debug.LogWarning("No placeholder selected!");
            return;
        }

        if (towerPrefab == null)
        {
            Debug.LogWarning("Tower prefab not assigned!");
            return;
        }

        currentPlaceholder.PlaceTower(towerPrefab);

        // Hide the panel now that selection is done
        if (panel != null)
            panel.SetActive(false);

        currentPlaceholder = null;
    }

    public void ClosePanel()
    {
        if (panel != null)
            panel.SetActive(false);

        currentPlaceholder = null;
    }
}
