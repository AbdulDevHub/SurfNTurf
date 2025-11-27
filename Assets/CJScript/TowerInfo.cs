using UnityEngine;
using UnityEngine.UI;
using TMPro; // Add this


public class TowerInfoUI : MonoBehaviour
{
    public GameObject panel;

    public TextMeshProUGUI damageText; // <-- changed
    public TextMeshProUGUI rangeText;  // <-- changed
    public TextMeshProUGUI rateText;

    private TowerBase currentTower;

    // Set which tower the panel is displaying
    public void SetTower(TowerBase tower)
    {
        currentTower = tower;
        UpdateInfo();
    }

    // Update the text fields
    public void UpdateInfo()
    {
        if (currentTower == null) return;

        damageText.text = "Damage: " + currentTower.GetDamage();
        rangeText.text = "Range: " + currentTower.GetRange();
        rateText.text = "Rate: " + currentTower.GetRate();
    }

    // Upgrade button
    public void OnUpgradePressed()
    {
        if (currentTower != null)
        {
            currentTower.Upgrade();
            UpdateInfo();
        }
    }

    // Sell button
    public void OnSellPressed()
    {
        if (currentTower != null)
        {
            currentTower.Sell();
            panel.SetActive(false);
        }
    }

    // Close button
    public void OnClosePressed()
    {
        panel.SetActive(false);
    }

    // Open the panel for a tower
    public void Open(TowerBase tower)
    {
        SetTower(tower);
        panel.SetActive(true);
    }
}
