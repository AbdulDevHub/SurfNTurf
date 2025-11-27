using UnityEngine;

public class FishermanTower : TowerBase
{
    private void Awake()
    {
        damagePerLevel = new int[] { 3, 6, 10 };
        rangePerLevel  = new float[] { 4f, 4.5f, 5f };
        ratePerLevel   = new float[] { 0.7f, 0.55f, 0.4f };
        upgradeCosts   = new int[] { 20, 45 };
        sellValue = 35;
    }
}
