using UnityEngine;

public class TowerClick : MonoBehaviour
{
    private TowerBase tower;

    private void Awake()
    {
        tower = GetComponent<TowerBase>();
    }

    private void OnMouseDown()
    {
        TowerInfoUI ui = FindObjectOfType<TowerInfoUI>();
        ui.Open(tower);
    }
}
