using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class GameUIManager : MonoBehaviour
{
    public static GameUIManager Instance;

    [Header("Panels")]
    public GameObject towerTypeUI;
    public GameObject towerInfoUI;

    [Header("Tower Type Buttons")]
    public Button birdButton;
    public Button bearButton;
    public Button fishermanButton;
    public Button exitTowerTypeButton;

    [Header("Tower Info Buttons")]
    public Button upgradeButton;
    public Button sellButton;
    public Button exitTowerInfoButton;

    [Header("Tower Info Texts")]
    public TMP_Text typeAndLevelText;
    public TMP_Text damageText;
    public TMP_Text rangeText;
    public TMP_Text rateText;
    public TMP_Text upgradeCostText;
    public TMP_Text sellCostText;

    void Awake()
    {
        if (Instance == null) 
            Instance = this;
        else 
        {
            Destroy(gameObject);
            return;
        }

        SetupButtonListeners();
        
        // Hide both panels at start
        HideTowerTypeUI();
        HideTowerInfoUI();
    }

    void SetupButtonListeners()
    {
        // Tower Type UI buttons
        if (birdButton != null)
            birdButton.onClick.AddListener(() => BuildTowerType("Bird"));

        if (bearButton != null)
            bearButton.onClick.AddListener(() => BuildTowerType("Bear"));

        if (fishermanButton != null)
            fishermanButton.onClick.AddListener(() => BuildTowerType("Fisherman"));

        if (exitTowerTypeButton != null)
            exitTowerTypeButton.onClick.AddListener(ExitAllUI);

        // Tower Info UI buttons
        if (upgradeButton != null)
            upgradeButton.onClick.AddListener(UpgradeTower);

        if (sellButton != null)
            sellButton.onClick.AddListener(SellTower);

        if (exitTowerInfoButton != null)
            exitTowerInfoButton.onClick.AddListener(ExitAllUI);
    }

    // ===== Tower Type Panel Methods =====

    void BuildTowerType(string type)
    {
        if (TowerSpotController.ActiveSpot != null)
            TowerSpotController.ActiveSpot.BuildTowerFromUI(type);
    }

    // ===== Tower Info Panel Methods =====

    void UpgradeTower()
    {
        if (TowerSpotController.ActiveSpot != null)
            TowerSpotController.ActiveSpot.UpgradeTowerFromUI();
    }

    void SellTower()
    {
        if (TowerSpotController.ActiveSpot != null)
            TowerSpotController.ActiveSpot.DeleteTowerFromUI();
    }

    // ===== UI Visibility Methods =====

    public void ShowTowerTypeUI()
    {
        if (towerTypeUI != null)
            towerTypeUI.SetActive(true);
    }

    public void HideTowerTypeUI()
    {
        if (towerTypeUI != null)
            towerTypeUI.SetActive(false);
    }

    public void ShowTowerInfoUI()
    {
        if (towerInfoUI != null)
            towerInfoUI.SetActive(true);
    }

    public void HideTowerInfoUI()
    {
        if (towerInfoUI != null)
            towerInfoUI.SetActive(false);
    }

    public void ExitAllUI()
    {
        if (TowerSpotController.ActiveSpot != null)
            TowerSpotController.ActiveSpot.ExitUIFromUI();
    }

    // ===== Update Tower Info Display =====

    public void UpdateTowerInfo(string type, int level, int damage, int range, float rate, int upgradeCost, int sellCost, bool canUpgrade)
    {
        if (typeAndLevelText != null)
            typeAndLevelText.text = $"{type} LV{level}";
        
        if (damageText != null)
            damageText.text = $"Damage: {damage}";
        
        if (rangeText != null)
            rangeText.text = $"Range: {range}";
        
        if (rateText != null)
            rateText.text = $"Rate: {rate:F2}s";
        
        if (upgradeCostText != null)
            upgradeCostText.text = canUpgrade ? $"Upgrade: ${upgradeCost}" : "MAX LEVEL";
        
        if (sellCostText != null)
            sellCostText.text = $"Sell: ${sellCost}";

        // Disable upgrade button if at max level
        if (upgradeButton != null)
            upgradeButton.interactable = canUpgrade;
    }
}