using UnityEngine;

public class TowerBase : MonoBehaviour
{
    // Stats for upgrades
    public int[] damagePerLevel;
    public float[] rangePerLevel;
    public float[] ratePerLevel;

    public int[] upgradeCosts;
    public int sellValue;

    public int level = 0;

    // References
    public TowerInfoUI infoUI;
    public TowerPlaceholder placeholder;

    // Open the info panel
    public void OpenInfoPanel()
    {
        if (infoUI != null)
        {
            infoUI.SetTower(this);
            infoUI.panel.SetActive(true);
        }
    }

    // Upgrade tower
    public void Upgrade()
    {
        if (level < damagePerLevel.Length - 1)
        {
            level++;
            if (infoUI != null)
                infoUI.UpdateInfo();
        }
    }

    // Sell tower
    public void Sell()
    {
        if (placeholder != null)
        {
            placeholder.isFilled = false;
            placeholder.currentTower = null;
        }

        if (infoUI != null)
            infoUI.panel.SetActive(false);

        Destroy(gameObject);
    }

    // Helper methods to get current stats
    public int GetDamage() => damagePerLevel[level];
    public float GetRange() => rangePerLevel[level];
    public float GetRate() => ratePerLevel[level];
    public int GetUpgradeCost() => (level < upgradeCosts.Length) ? upgradeCosts[level] : 0;
}
