using UnityEngine;

public class CircleClickForwarder : MonoBehaviour
{
    private TowerSpotController towerSpot;

    void Awake()
    {
        towerSpot = GetComponentInParent<TowerSpotController>();
    }

    void OnMouseDown()
    {
        if (towerSpot != null)
            towerSpot.HandleClickFromCircle();
    }
}
