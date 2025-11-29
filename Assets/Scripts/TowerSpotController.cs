using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TowerSpotController : MonoBehaviour
{
    private Renderer circleRenderer;

    public Color defaultColor = new Color(0, 0.4f, 1f, 0.4f);
    public Color selectedColor = new Color(1f, 1f, 0.2f, 0.6f);
    public static TowerSpotController ActiveSpot;

    private GameObject crystalEffect;
    private GameObject magicCircleEffect;

    private bool isSelected = false;

    // Tower System
    private Dictionary<string, List<GameObject>> towerLevels =
        new Dictionary<string, List<GameObject>>();

    private string currentTowerType = "";
    private int currentLevel = 0;

    // Range System
    private GameObject attackRangesRoot;
    private Dictionary<int, GameObject> rangeObjects = new();

    private Dictionary<string, Dictionary<int, int>> towerRangeLookup =
        new Dictionary<string, Dictionary<int, int>>()
    {
        { "Bird",      new Dictionary<int,int>() { {1,3}, {2,4}, {3,5} } },
        { "Bear",      new Dictionary<int,int>() { {1,1}, {2,2} } },
        { "Fisherman", new Dictionary<int,int>() { {1,1}, {2,2}, {3,3} } }
    };

    // Tower Stats Lookup
    private Dictionary<string, Dictionary<int, TowerStats>> towerStatsLookup =
        new Dictionary<string, Dictionary<int, TowerStats>>()
    {
        { "Bird", new Dictionary<int, TowerStats>() {
            {1, new TowerStats(500, 1, 3, 2f)},
            {2, new TowerStats(600, 2, 4, 2f)},
            {3, new TowerStats(700, 4, 5, 1f)}
        }},
        { "Bear", new Dictionary<int, TowerStats>() {
            {1, new TowerStats(400, 3, 1, 3f)},
            {2, new TowerStats(500, 5, 2, 2f)}
        }},
        { "Fisherman", new Dictionary<int, TowerStats>() {
            {1, new TowerStats(250, 1, 1, 1f)},
            {2, new TowerStats(300, 1, 2, 1f)},
            {3, new TowerStats(350, 1, 3, 1f)}
        }}
    };

    // Initial build costs (same as level 1 upgrade cost)
    private Dictionary<string, int> initialBuildCosts = new Dictionary<string, int>()
    {
        { "Bird", 500 },
        { "Bear", 400 },
        { "Fisherman", 250 }
    };

    void Awake()
    {
        circleRenderer = transform.Find("Circle")?.GetComponent<Renderer>();
        crystalEffect = transform.Find("Crystal")?.gameObject;
        magicCircleEffect = transform.Find("MagicCircle")?.gameObject;

        if (circleRenderer != null)
            circleRenderer.material.color = defaultColor;

        AutoDetectTowers();
        HideAllTowers();
        AutoDetectRanges();
    }

    // ============================================================
    // Circle Management
    // ============================================================
    void UpdateCircleVisibility()
    {
        if (circleRenderer == null) return;

        bool shouldShowCircle = isSelected || currentLevel == 0;
        circleRenderer.enabled = shouldShowCircle;

        if (crystalEffect != null)
            crystalEffect.SetActive(currentLevel == 0 && !isSelected);
    }

    // ============================================================
    // Handling Circle Click
    // ============================================================
    public void HandleClickFromCircle()
    {
        // Prevent clicks when pressing UI buttons
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        // Deselect previously active spot
        if (ActiveSpot != null && ActiveSpot != this)
            ActiveSpot.DeselectSpot();

        isSelected = true;
        ActiveSpot = this;

        SoundManager.Instance.PlaySound("Tower Spot Select", transform.position);

        if (circleRenderer != null)
            circleRenderer.material.color = selectedColor;

        UpdateCircleVisibility();
        UpdateRangeDisplay();

        // Show appropriate UI based on tower state
        if (currentLevel == 0)
        {
            GameUIManager.Instance.HideTowerInfoUI();
            GameUIManager.Instance.ShowTowerTypeUI();
        }
        else
        {
            GameUIManager.Instance.HideTowerTypeUI();
            GameUIManager.Instance.ShowTowerInfoUI();
            UpdateTowerInfoUI();
        }
    }

    // ============================================================
    // UI Updates
    // ============================================================
    public void UpdateTowerInfoUI()
    {
        if (currentLevel == 0) return;

        string type = currentTowerType;
        int level = currentLevel;

        // ================================
        // NON-FISHERMAN TOWERS (Bird/Bear)
        // ================================
        if (type != "Fisherman")
        {
            TowerStats stats = towerStatsLookup[type][level];

            int damage = stats.damage;
            int range = stats.range;
            float rate = stats.rate;

            bool canUpgrade = towerStatsLookup[type].ContainsKey(level + 1);
            int upgradeCost = canUpgrade ? towerStatsLookup[type][level + 1].upgradeCost : 0;

            int sellCost = CalculateSellCost();
            int maxLevel = towerStatsLookup[type].Count;

            GameUIManager.Instance.UpdateTowerInfo(
                type,
                level,
                damage,
                range,
                rate,
                upgradeCost,
                sellCost,
                canUpgrade,
                maxLevel
            );
            return;
        }

        // ================================
        // FISHERMAN SPECIAL CASE
        // ================================

        int displayDamage;   // = rod damage OR net capacity
        float displayRate;   // = rod rate OR net cooldown

        // LV1 → Rod mode (normal attack)
        if (level == 1)
        {
            displayDamage = 1;    // baseDamage from FishermanAttack
            displayRate   = 1f;   // baseAttackRate
        }
        // LV2 → Net trap (small net)
        else if (level == 2)
        {
            displayDamage = 5;    // net capacity
            displayRate   = 5f;   // cooldown
        }
        // LV3 → Net trap (large net)
        else // level 3
        {
            displayDamage = 10;   // net capacity
            displayRate   = 5f;   // cooldown
        }

        // Get range from your normal lookup
        int displayRange = towerRangeLookup["Fisherman"][level];

        // Upgrade logic
        bool fisherCanUpgrade = towerStatsLookup["Fisherman"].ContainsKey(level + 1);
        int fisherUpgradeCost = fisherCanUpgrade
            ? towerStatsLookup["Fisherman"][level + 1].upgradeCost
            : 0;

        int fisherSell = CalculateSellCost();
        int fisherMaxLevel = towerStatsLookup["Fisherman"].Count;

        // Send to UI
        GameUIManager.Instance.UpdateTowerInfo(
            "Fisherman",
            level,
            displayDamage,
            displayRange,
            displayRate,
            fisherUpgradeCost,
            fisherSell,
            fisherCanUpgrade,
            fisherMaxLevel
        );
    }

    int CalculateSellCost()
    {
        int totalCost = 0;
        for (int i = 1; i <= currentLevel; i++)
        {
            totalCost += towerStatsLookup[currentTowerType][i].upgradeCost;
        }
        return totalCost / 2;
    }

    // ============================================================
    // Public UI Buttons
    // ============================================================
    public void BuildTowerFromUI(string type)
    {
        // Check if player has enough scales
        int buildCost = initialBuildCosts[type];
        if (StatManager.Instance.remainingScales < buildCost)
        {
            Debug.Log("Not enough scales to build " + type);
            return;
        }

        if (!BuildTower(type)) return;

        // Deduct scales
        StatManager.Instance.AddScales(-buildCost);

        // Stay in tower info UI, but update to show the built tower
        UpdateTowerInfoUI();
    }

    public void UpgradeTowerFromUI()
    {
        // Check if player has enough scales
        int upgradeCost = towerStatsLookup[currentTowerType][currentLevel + 1].upgradeCost;
        if (StatManager.Instance.remainingScales < upgradeCost)
        {
            Debug.Log("Not enough scales to upgrade");
            return;
        }

        if (!UpgradeTower()) return;
        
        // Deduct scales
        StatManager.Instance.AddScales(-upgradeCost);
        
        UpdateTowerInfoUI();
    }

    public void DeleteTowerFromUI()
    {
        // Refund scales
        int sellCost = CalculateSellCost();
        StatManager.Instance.AddScales(sellCost);

        DeleteTower();
        GameUIManager.Instance.HideTowerInfoUI();
        GameUIManager.Instance.ShowTowerTypeUI();
    }

    public void ExitUIFromUI()
    {
        DeselectSpot();
        GameUIManager.Instance.HideTowerInfoUI();
        GameUIManager.Instance.HideTowerTypeUI();
    }

    // ============================================================
    // Tower Building
    // ============================================================
    bool BuildTower(string type)
    {
        if (currentLevel > 0) return false;
        if (!towerLevels.ContainsKey(type)) return false;
        if (towerLevels[type].Count == 0 || towerLevels[type][0] == null) return false;

        HideAllTowers();
        towerLevels[type][0].SetActive(true);

        currentTowerType = type;
        currentLevel = 1;

        RotateTowardRiver();
        UpdateRangeDisplay();
        UpdateCircleVisibility();

        return true;
    }

    bool UpgradeTower()
    {
        if (currentLevel == 0) return false;

        string type = currentTowerType;
        int nextLevel = currentLevel + 1;

        if (!towerLevels.ContainsKey(type)) return false;
        if (towerLevels[type].Count < nextLevel) return false;
        if (towerLevels[type][nextLevel - 1] == null) return false;

        towerLevels[type][currentLevel - 1].SetActive(false);
        towerLevels[type][nextLevel - 1].SetActive(true);

        currentLevel = nextLevel;

        RotateTowardRiver();
        UpdateRangeDisplay();

        return true;
    }

    void DeleteTower()
    {
        if (currentLevel == 0) return;

        towerLevels[currentTowerType][currentLevel - 1].SetActive(false);

        currentTowerType = "";
        currentLevel = 0;

        HideAllRanges();
        UpdateCircleVisibility();
    }

    // ============================================================
    // Auto Detect Model Levels
    // ============================================================
    void AutoDetectTowers()
    {
        foreach (Transform child in transform)
        {
            string name = child.name;
            if (!name.Contains("LV")) continue;

            string[] parts = name.Split("LV");
            if (parts.Length != 2) continue;

            string type = parts[0];
            if (!int.TryParse(parts[1], out int level)) continue;

            if (!towerLevels.ContainsKey(type))
                towerLevels[type] = new List<GameObject>();

            while (towerLevels[type].Count < level)
                towerLevels[type].Add(null);

            towerLevels[type][level - 1] = child.gameObject;
        }
    }

    void HideAllTowers()
    {
        foreach (var kv in towerLevels)
        {
            foreach (var model in kv.Value)
            {
                if (model != null)
                    model.SetActive(false);
            }
        }
    }

    // ============================================================
    // Public Accessors (for GameUIManager)
    // ============================================================
    
    public int GetBuildCost(string type)
    {
        if (initialBuildCosts.ContainsKey(type))
            return initialBuildCosts[type];
        return 0;
    }

    public int GetCurrentLevel()
    {
        return currentLevel;
    }

    public int GetTowerRange(string type, int level)
    {
        if (towerRangeLookup.ContainsKey(type) && towerRangeLookup[type].ContainsKey(level))
            return towerRangeLookup[type][level];
        return 0;
    }

    public TowerStats GetTowerStatsForLevel(string type, int level)
    {
        if (towerStatsLookup.ContainsKey(type) && towerStatsLookup[type].ContainsKey(level))
            return towerStatsLookup[type][level];
        return new TowerStats(0, 0, 0, 0f);
    }

    // ============================================================
    // Range Visualization
    // ============================================================
    void AutoDetectRanges()
    {
        attackRangesRoot = transform.Find("AttackRanges")?.gameObject;
        if (attackRangesRoot == null) return;

        foreach (Transform t in attackRangesRoot.transform)
        {
            if (!t.name.StartsWith("Range")) continue;

            string num = t.name.Replace("Range", "");
            if (int.TryParse(num, out int r))
                rangeObjects[r] = t.gameObject;

            var rend = t.GetComponent<Renderer>();
            if (rend != null) rend.enabled = false;
        }
    }

    void UpdateRangeDisplay()
    {
        if (currentLevel == 0 || string.IsNullOrEmpty(currentTowerType))
        {
            HideAllRanges();
            return;
        }

        if (!towerRangeLookup.ContainsKey(currentTowerType)) return;
        if (!towerRangeLookup[currentTowerType].ContainsKey(currentLevel)) return;

        int correctRange = towerRangeLookup[currentTowerType][currentLevel];

        foreach (var r in rangeObjects)
        {
            var rend = r.Value.GetComponent<Renderer>();
            if (rend != null)
                rend.enabled = (r.Key == correctRange);
        }
    }

    void HideAllRanges()
    {
        foreach (var r in rangeObjects)
        {
            var rend = r.Value.GetComponent<Renderer>();
            if (rend != null)
                rend.enabled = false;
        }
    }

    // ============================================================
    // Tower Orientation
    // ============================================================
    Transform GetClosestPathPoint()
    {
        GameObject[] points = GameObject.FindGameObjectsWithTag("PathPoint");
        if (points.Length == 0) return null;

        Transform closest = null;
        float minDist = Mathf.Infinity;

        Vector3 myPos = transform.position;
        myPos.y = 0f;

        foreach (var p in points)
        {
            Vector3 pPos = p.transform.position;
            pPos.y = 0f;

            float dist = Vector3.Distance(myPos, pPos);
            if (dist < minDist)
            {
                minDist = dist;
                closest = p.transform;
            }
        }

        return closest;
    }

    void RotateTowardRiver()
    {
        if (currentLevel == 0) return;
        if (!towerLevels.ContainsKey(currentTowerType)) return;
        if (currentLevel > towerLevels[currentTowerType].Count) return;

        GameObject towerModel = towerLevels[currentTowerType][currentLevel - 1];
        if (towerModel == null) return;

        Transform target = GetClosestPathPoint();
        if (target == null) return;

        Vector3 lookPos = target.position;
        lookPos.y = towerModel.transform.position.y;

        towerModel.transform.LookAt(lookPos);
    }

    // ============================================================
    // Deselect
    // ============================================================
    public void DeselectSpot()
    {
        isSelected = false;
        
        if (ActiveSpot == this)
            ActiveSpot = null;

        if (circleRenderer != null)
            circleRenderer.material.color = defaultColor;

        HideAllRanges();
        UpdateCircleVisibility();
    }
}

// Helper struct to store tower stats
[System.Serializable]
public struct TowerStats
{
    public int upgradeCost;
    public int damage;
    public int range;
    public float rate;

    public TowerStats(int upgradeCost, int damage, int range, float rate)
    {
        this.upgradeCost = upgradeCost;
        this.damage = damage;
        this.range = range;
        this.rate = rate;
    }
}