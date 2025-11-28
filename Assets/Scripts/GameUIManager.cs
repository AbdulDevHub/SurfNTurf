using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

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
    public TMP_Text upgradeButtonText;
    public TMP_Text upgradeCostText;
    public TMP_Text sellCostText;

    [Header("Shake Settings")]
    public float shakeDuration = 0.5f;
    public float shakeAmount = 10f;

    // Track selected tower type for placement
    private string selectedTowerTypeForPlacement = "";
    private Coroutine upgradeButtonShakeCoroutine;
    private Coroutine sellButtonShakeCoroutine;

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
        // Tower Type UI buttons - now just select tower type
        if (birdButton != null)
            birdButton.onClick.AddListener(() => SelectTowerType("Bird"));

        if (bearButton != null)
            bearButton.onClick.AddListener(() => SelectTowerType("Bear"));

        if (fishermanButton != null)
            fishermanButton.onClick.AddListener(() => SelectTowerType("Fisherman"));

        if (exitTowerTypeButton != null)
            exitTowerTypeButton.onClick.AddListener(ExitAllUI);

        // Tower Info UI buttons - only add onClick for when they're enabled
        if (upgradeButton != null)
            upgradeButton.onClick.AddListener(HandleUpgradeOrPlaceButton);

        if (sellButton != null)
            sellButton.onClick.AddListener(SellTower);

        if (exitTowerInfoButton != null)
            exitTowerInfoButton.onClick.AddListener(ExitAllUI);

        // Add EventTriggers for disabled button detection
        AddDisabledButtonDetection(upgradeButton, OnUpgradeButtonClicked);
        AddDisabledButtonDetection(sellButton, OnSellButtonClicked);
    }

    // NEW: Add EventTrigger to detect clicks even when button is disabled
    void AddDisabledButtonDetection(Button button, System.Action callback)
    {
        if (button == null) return;

        EventTrigger trigger = button.GetComponent<EventTrigger>();
        if (trigger == null)
            trigger = button.gameObject.AddComponent<EventTrigger>();

        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerClick;
        entry.callback.AddListener((data) => { callback?.Invoke(); });
        trigger.triggers.Add(entry);
    }

    // NEW: Handle upgrade button clicks (works even when disabled)
    void OnUpgradeButtonClicked()
    {
        // If button is disabled, shake it
        if (upgradeButton != null && !upgradeButton.interactable)
        {
            if (upgradeButtonShakeCoroutine == null)
            {
                upgradeButtonShakeCoroutine = StartCoroutine(ShakeButton(upgradeButton, () => upgradeButtonShakeCoroutine = null));
            }
        }
        // If enabled, the onClick listener will handle it
    }

    // NEW: Handle sell button clicks (works even when disabled)
    void OnSellButtonClicked()
    {
        // If button is disabled, shake it
        if (sellButton != null && !sellButton.interactable)
        {
            if (sellButtonShakeCoroutine == null)
            {
                sellButtonShakeCoroutine = StartCoroutine(ShakeButton(sellButton, () => sellButtonShakeCoroutine = null));
            }
        }
        // If enabled, the onClick listener will handle it
    }

    void SelectTowerType(string type)
    {
        if (TowerSpotController.ActiveSpot != null && TowerSpotController.ActiveSpot.GetCurrentLevel() == 0)
        {
            selectedTowerTypeForPlacement = type;
            
            // Switch to tower info UI showing placement info
            HideTowerTypeUI();
            ShowTowerInfoUI();
            UpdatePlacementInfoUI(type);
        }
    }

    void HandleUpgradeOrPlaceButton()
    {
        if (TowerSpotController.ActiveSpot == null) return;

        // If we're in placement mode
        if (TowerSpotController.ActiveSpot.GetCurrentLevel() == 0 && !string.IsNullOrEmpty(selectedTowerTypeForPlacement))
        {
            int buildCost = TowerSpotController.ActiveSpot.GetBuildCost(selectedTowerTypeForPlacement);
            if (StatManager.Instance.remainingScales < buildCost)
            {
                return; // EventTrigger will handle the shake
            }

            // Place the tower
            TowerSpotController.ActiveSpot.BuildTowerFromUI(selectedTowerTypeForPlacement);
            selectedTowerTypeForPlacement = "";
        }
        // If we're in upgrade mode
        else
        {
            UpgradeTower();
        }
    }

    IEnumerator ShakeButton(Button button, System.Action onComplete)
    {
        if (button == null)
        {
            onComplete?.Invoke();
            yield break;
        }

        RectTransform rectTransform = button.GetComponent<RectTransform>();
        if (rectTransform == null)
        {
            onComplete?.Invoke();
            yield break;
        }

        Vector3 originalPosition = rectTransform.localPosition;
        float elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            float x = originalPosition.x + Random.Range(-shakeAmount, shakeAmount);
            rectTransform.localPosition = new Vector3(x, originalPosition.y, originalPosition.z);

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Always reset to original position
        rectTransform.localPosition = originalPosition;
        
        // Mark shake as complete
        onComplete?.Invoke();
    }

    void UpgradeTower()
    {
        if (TowerSpotController.ActiveSpot != null)
            TowerSpotController.ActiveSpot.UpgradeTowerFromUI();
    }

    void SellTower()
    {
        if (TowerSpotController.ActiveSpot == null) return;
        
        TowerSpotController.ActiveSpot.DeleteTowerFromUI();
    }

    public void ShowTowerTypeUI()
    {
        if (towerTypeUI != null)
        {
            towerTypeUI.SetActive(true);
        }
        selectedTowerTypeForPlacement = "";
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
        selectedTowerTypeForPlacement = "";
    }

    public void ExitAllUI()
    {
        if (TowerSpotController.ActiveSpot != null)
            TowerSpotController.ActiveSpot.ExitUIFromUI();
        selectedTowerTypeForPlacement = "";
    }

    void UpdatePlacementInfoUI(string type)
    {
        if (TowerSpotController.ActiveSpot == null) return;

        int buildCost = TowerSpotController.ActiveSpot.GetBuildCost(type);
        int range = TowerSpotController.ActiveSpot.GetTowerRange(type, 1);

        // Update header
        if (typeAndLevelText != null)
        {
            string displayType = type == "Fisherman" ? "Fishman" : type;
            typeAndLevelText.text = $"{displayType} Tower";
        }

        // Get level 1 stats
        var stats = TowerSpotController.ActiveSpot.GetTowerStatsForLevel(type, 1);

        // Update stats display
        if (type == "Fisherman")
        {
            if (damageText != null)
                damageText.text = $"Damage: {stats.damage}";
            if (rangeText != null)
                rangeText.text = $"Range: {range}";
            if (rateText != null)
                rateText.text = $"Rate: {stats.rate:F1}s";
        }
        else
        {
            if (damageText != null)
                damageText.text = $"Damage: {stats.damage}";
            if (rangeText != null)
                rangeText.text = $"Range: {range}";
            if (rateText != null)
                rateText.text = $"Rate: {stats.rate:F1}s";
        }

        // Update button text to "Place"
        if (upgradeButtonText != null)
            upgradeButtonText.text = "Place";

        if (upgradeCostText != null)
            upgradeCostText.text = $"{buildCost} Scales";

        // Show N/A for sell cost during placement
        if (sellCostText != null)
            sellCostText.text = "N/A";

        // Update button interactability based on affordability
        if (upgradeButton != null)
        {
            bool canAfford = StatManager.Instance.remainingScales >= buildCost;
            upgradeButton.interactable = canAfford;
        }

        // Grey out sell button during placement (can't sell what hasn't been built)
        if (sellButton != null)
            sellButton.interactable = false;
    }

    public void UpdateTowerInfo(
    string type,
    int level,
    int damage,
    int range,
    float rate,
    int upgradeCost,
    int sellCost,
    bool canUpgrade,
    int maxLevel)
    {
        // Reset button text to "Upgrade"
        if (upgradeButtonText != null)
            upgradeButtonText.text = "Upgrade";

        // Enable sell button for existing towers
        if (sellButton != null)
            sellButton.interactable = true;

        // Header text
        if (typeAndLevelText != null)
        {
            string displayType = type == "Fisherman" ? "Fishman" : type;
            typeAndLevelText.text = $"{displayType} (LV {level}/{maxLevel})";
        }

        // --- FISHERMAN SPECIAL LOGIC (ONLY LV2 & LV3) ---
        if (type == "Fisherman" && level >= 2)
        {
            if (damageText != null)
                damageText.text = $"Net Capacity: {damage}";

            if (rangeText != null)
                rangeText.text = $"Range: {range}";

            if (rateText != null)
                rateText.text = $"Net Cooldown: {rate:F1}s";

            if (upgradeCostText != null)
                upgradeCostText.text = canUpgrade ? $"{upgradeCost} Scales" : "MAX LEVEL";

            if (sellCostText != null)
                sellCostText.text = $"{sellCost} Scales";

            if (upgradeButton != null)
            {
                bool hasEnoughScales = StatManager.Instance.remainingScales >= upgradeCost;
                upgradeButton.interactable = canUpgrade && hasEnoughScales;
            }

            return;
        }

        // --- NORMAL TOWERS + FISHERMAN LV1 ---
        if (damageText != null)
            damageText.text = $"Damage: {damage}";

        if (rangeText != null)
            rangeText.text = $"Range: {range}";

        if (rateText != null)
            rateText.text = $"Rate: {rate:F1}s";

        if (upgradeCostText != null)
            upgradeCostText.text = canUpgrade ? $"{upgradeCost} Scales" : "MAX LEVEL";

        if (sellCostText != null)
            sellCostText.text = $"{sellCost} Scales";

        if (upgradeButton != null)
        {
            bool hasEnoughScales = StatManager.Instance.remainingScales >= upgradeCost;
            upgradeButton.interactable = canUpgrade && hasEnoughScales;
        }
    }
}