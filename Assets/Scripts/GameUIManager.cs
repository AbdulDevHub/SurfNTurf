using UnityEngine;
using TMPro;
using UnityEngine.UI;
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
    public TMP_Text upgradeCostText;
    public TMP_Text sellCostText;

    [Header("Shake Settings")]
    public float shakeDuration = 0.5f;
    public float shakeAmount = 10f;

    // Store original colors
    private Color birdButtonOriginalColor;
    private Color bearButtonOriginalColor;
    private Color fishermanButtonOriginalColor;
    private bool colorsStored = false;

    // Track which buttons are currently shaking
    private Coroutine birdShakeCoroutine;
    private Coroutine bearShakeCoroutine;
    private Coroutine fishermanShakeCoroutine;

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
            birdButton.onClick.AddListener(() => BuildTowerType("Bird", birdButton));

        if (bearButton != null)
            bearButton.onClick.AddListener(() => BuildTowerType("Bear", bearButton));

        if (fishermanButton != null)
            fishermanButton.onClick.AddListener(() => BuildTowerType("Fisherman", fishermanButton));

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

    void BuildTowerType(string type, Button button)
    {
        if (TowerSpotController.ActiveSpot != null)
        {
            // Check if player can afford this tower
            int buildCost = TowerSpotController.ActiveSpot.GetBuildCost(type);
            if (StatManager.Instance.remainingScales < buildCost)
            {
                // Determine which coroutine to use based on button type
                if (button == birdButton && birdShakeCoroutine == null)
                {
                    birdShakeCoroutine = StartCoroutine(ShakeButton(button, () => birdShakeCoroutine = null));
                }
                else if (button == bearButton && bearShakeCoroutine == null)
                {
                    bearShakeCoroutine = StartCoroutine(ShakeButton(button, () => bearShakeCoroutine = null));
                }
                else if (button == fishermanButton && fishermanShakeCoroutine == null)
                {
                    fishermanShakeCoroutine = StartCoroutine(ShakeButton(button, () => fishermanShakeCoroutine = null));
                }
                return;
            }

            TowerSpotController.ActiveSpot.BuildTowerFromUI(type);
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
        {
            towerTypeUI.SetActive(true);
            
            // Store original colors on first show
            if (!colorsStored)
            {
                StoreOriginalColors();
                colorsStored = true;
            }
            
            UpdateTowerButtonStates();
        }
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

    // ===== Update Tower Button States =====
    void StoreOriginalColors()
    {
        if (birdButton != null)
        {
            Image img = birdButton.GetComponent<Image>();
            if (img != null) birdButtonOriginalColor = img.color;
        }

        if (bearButton != null)
        {
            Image img = bearButton.GetComponent<Image>();
            if (img != null) bearButtonOriginalColor = img.color;
        }

        if (fishermanButton != null)
        {
            Image img = fishermanButton.GetComponent<Image>();
            if (img != null) fishermanButtonOriginalColor = img.color;
        }
    }

    public void UpdateTowerButtonStates()
    {
        if (TowerSpotController.ActiveSpot == null) return;

        int currentScales = StatManager.Instance.remainingScales;

        // Update Bird button
        if (birdButton != null)
        {
            int birdCost = TowerSpotController.ActiveSpot.GetBuildCost("Bird");
            UpdateButtonVisuals(birdButton, currentScales >= birdCost, birdButtonOriginalColor);
        }

        // Update Bear button
        if (bearButton != null)
        {
            int bearCost = TowerSpotController.ActiveSpot.GetBuildCost("Bear");
            UpdateButtonVisuals(bearButton, currentScales >= bearCost, bearButtonOriginalColor);
        }

        // Update Fisherman button
        if (fishermanButton != null)
        {
            int fishermanCost = TowerSpotController.ActiveSpot.GetBuildCost("Fisherman");
            UpdateButtonVisuals(fishermanButton, currentScales >= fishermanCost, fishermanButtonOriginalColor);
        }
    }

    void UpdateButtonVisuals(Button button, bool canAfford, Color originalColor)
    {
        if (button == null) return;

        // Change to grey color when can't afford, restore original color when can afford
        Image buttonImage = button.GetComponent<Image>();
        if (buttonImage != null)
        {
            if (canAfford)
            {
                // Restore original color
                buttonImage.color = originalColor;
            }
            else
            {
                // Apply lighter grey tint for better text visibility
                buttonImage.color = new Color(0.7f, 0.7f, 0.7f, 1f);
            }
        }

        // Also grey out any child images (icons, etc.)
        Image[] childImages = button.GetComponentsInChildren<Image>();
        foreach (Image img in childImages)
        {
            if (img != buttonImage) // Skip the button itself
            {
                if (canAfford)
                {
                    img.color = Color.white;
                }
                else
                {
                    img.color = new Color(0.7f, 0.7f, 0.7f, 1f);
                }
            }
        }

        // Text stays black - no color changes needed for text
    }

    // ===== Update Tower Info Display =====
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