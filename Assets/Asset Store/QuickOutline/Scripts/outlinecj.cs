using UnityEngine;

public class TowerPlaceholder : MonoBehaviour
{
    public GameObject selectPanel;
    public TowerSelectorUI selector;
    
    [HideInInspector] public bool isFilled = false;
    [HideInInspector] public TowerBase currentTower;

    private void OnMouseDown()
    {
        if (isFilled) 
        {
            // If already filled, open the info panel of the tower
            currentTower.OpenInfoPanel();
            return;
        }

        // Open tower selection UI
        selector.currentPlaceholder = this;
        selectPanel.SetActive(true);
    }

    public void PlaceTower(GameObject towerPrefab)
    {
        GameObject towerObj = Instantiate(towerPrefab, transform.position, Quaternion.identity);
        currentTower = towerObj.GetComponent<TowerBase>();
        isFilled = true;

        currentTower.placeholder = this;
        currentTower.OpenInfoPanel();
    }
}
