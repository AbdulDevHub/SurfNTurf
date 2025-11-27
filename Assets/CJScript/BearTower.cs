using UnityEngine;

public class BearTower : TowerBase
{
    private void Awake()
    {
        damagePerLevel = new int[] { 10, 15, 25 };
        rangePerLevel  = new float[] { 2f, 2.2f, 2.6f };
        ratePerLevel   = new float[] { 1.5f, 1.3f, 1.1f };
        upgradeCosts   = new int[] { 50, 100 };
        sellValue = 60;
    }
}
