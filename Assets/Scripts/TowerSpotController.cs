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
        { "Fisherman", new Dictionary<int,int>() { {1,1} } }
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

        if (magicCircleEffect != null)
            magicCircleEffect.SetActive(shouldShowCircle);
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
    void UpdateTowerInfoUI()
    {
        if (currentLevel == 0) return;

        // Calculate tower stats (customize these formulas as needed)
        int damage = currentLevel * 10;
        int range = towerRangeLookup[currentTowerType][currentLevel];
        float rate = 1f / currentLevel;
        int upgradeCost = currentLevel * 100;
        int sellCost = currentLevel * 50;

        // Check if tower can be upgraded
        bool canUpgrade = towerLevels.ContainsKey(currentTowerType) &&
                         towerLevels[currentTowerType].Count > currentLevel &&
                         towerLevels[currentTowerType][currentLevel] != null;

        GameUIManager.Instance.UpdateTowerInfo(
            currentTowerType,
            currentLevel,
            damage,
            range,
            rate,
            upgradeCost,
            sellCost,
            canUpgrade
        );
    }

    // ============================================================
    // Public UI Buttons
    // ============================================================
    public void BuildTowerFromUI(string type)
    {
        if (!BuildTower(type)) return;

        GameUIManager.Instance.HideTowerTypeUI();
        GameUIManager.Instance.ShowTowerInfoUI();
        UpdateTowerInfoUI();
    }

    public void UpgradeTowerFromUI()
    {
        if (!UpgradeTower()) return;
        
        UpdateTowerInfoUI();
    }

    public void DeleteTowerFromUI()
    {
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