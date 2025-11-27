using UnityEngine;

public class BirdTower : TowerBase
{
    private void Awake()
    {
        damagePerLevel = new int[] { 5, 8, 12 };
        rangePerLevel  = new float[] { 3f, 3.5f, 4.2f };
        ratePerLevel   = new float[] { 1f, 0.8f, 0.6f };
        upgradeCosts   = new int[] { 30, 60 };
        sellValue = 40;
    }
}
