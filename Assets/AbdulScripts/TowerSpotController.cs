using System.Collections.Generic;
using UnityEngine;

public class TowerSpotController : MonoBehaviour
{
    private Renderer circleRenderer;

    public Color defaultColor = new Color(0, 0.4f, 1f, 0.4f);
    public Color selectedColor = new Color(1f, 1f, 0.2f, 0.6f);

    private bool isSelected = false;

    // ===============================
    // Attention Pulse Effect Settings
    // ===============================
    [Header("Circle Attention Effect")]
    public bool pulseWhenIdle = true;
    public float pulseSpeed = 2f;
    public float pulseAmplitude = 0.2f;
    public float scalePulseAmplitude = 0.03f;

    private Vector3 circleOriginalScale;

    // ===============================
    // Tower System
    // ===============================
    private Dictionary<string, List<GameObject>> towerLevels
        = new Dictionary<string, List<GameObject>>();

    private string currentTowerType = "";
    private int currentLevel = 0;

    // ===============================
    // Range System
    // ===============================
    private GameObject attackRangesRoot;
    private Dictionary<int, GameObject> rangeObjects = new Dictionary<int, GameObject>();

    private Dictionary<string, Dictionary<int, int>> towerRangeLookup =
        new Dictionary<string, Dictionary<int, int>>()
        {
            { "Bird",       new Dictionary<int, int>() { {1,3}, {2,4}, {3,5} } },
            { "Bear",       new Dictionary<int, int>() { {1,1}, {2,2} } },
            { "Fisherman",  new Dictionary<int, int>() { {1,1} } }
        };

    void Awake()
    {
        circleRenderer = transform.Find("Circle")?.GetComponent<Renderer>();

        if (circleRenderer != null)
        {
            circleRenderer.material.color = defaultColor;
            circleOriginalScale = circleRenderer.transform.localScale;
        }

        AutoDetectTowers();
        HideAllTowers();

        AutoDetectRanges();
    }

    // ===============================
    // Pulse Effect
    // ===============================
    void PulseCircleEffect()
    {
        if (!pulseWhenIdle) return;
        if (circleRenderer == null) return;
        if (isSelected) return; // stop pulsing when selected

        // Pulse alpha ("breathing")
        float alphaPulse = defaultColor.a +
            Mathf.Sin(Time.time * pulseSpeed) * pulseAmplitude * 0.5f;

        Color c = defaultColor;
        c.a = alphaPulse;
        circleRenderer.material.color = c;

        // Subtle scale pulse
        float scalePulse = 1f + Mathf.Sin(Time.time * pulseSpeed) * scalePulseAmplitude;
        circleRenderer.transform.localScale = circleOriginalScale * scalePulse;
    }

    // ===============================
    // Range Detection
    // ===============================
    void AutoDetectRanges()
    {
        attackRangesRoot = transform.Find("AttackRanges")?.gameObject;

        if (attackRangesRoot == null) return;

        foreach (Transform t in attackRangesRoot.transform)
        {
            if (t.name.StartsWith("Range"))
            {
                string num = t.name.Replace("Range", "");
                if (int.TryParse(num, out int rangeNum))
                {
                    rangeObjects[rangeNum] = t.gameObject;
                }

                t.gameObject.SetActive(false);
            }
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
            r.Value.SetActive(r.Key == correctRange);
    }

    void HideAllRanges()
    {
        foreach (var r in rangeObjects)
            r.Value.SetActive(false);
    }

    // ===============================
    // Tower Detection
    // ===============================
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

    public void HandleClickFromCircle()
    {
        isSelected = true;

        if (circleRenderer != null)
        {
            circleRenderer.material.color = selectedColor;
            circleRenderer.transform.localScale = circleOriginalScale; // stop scale pulse
        }

        UpdateRangeDisplay();
    }

    void Update()
    {
        // Pulse when NOT selected
        PulseCircleEffect();

        if (!isSelected) return;

        if (Input.GetKeyDown(KeyCode.F)) BuildTower("Fisherman");
        if (Input.GetKeyDown(KeyCode.B)) BuildTower("Bird");
        if (Input.GetKeyDown(KeyCode.E)) BuildTower("Bear");

        if (Input.GetKeyDown(KeyCode.X)) DeleteTower();

        if (Input.GetKeyDown(KeyCode.U)) UpgradeTower();

        if (Input.GetKeyDown(KeyCode.D))
            DeselectSpot();
    }

    // ===============================
    // Facing River
    // ===============================
    Transform GetClosestPathPoint()
    {
        GameObject[] points = GameObject.FindGameObjectsWithTag("PathPoint");
        if (points.Length == 0) return null;

        Transform closest = null;
        float minDist = Mathf.Infinity;

        Vector3 myPos = transform.position;
        myPos.y = 0f;

        foreach (GameObject p in points)
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
        if (currentLevel == 0 || string.IsNullOrEmpty(currentTowerType))
            return;

        GameObject towerModel = towerLevels[currentTowerType][currentLevel - 1];
        if (towerModel == null) return;

        Transform target = GetClosestPathPoint();
        if (target == null) return;

        Vector3 lookPos = target.position;
        lookPos.y = towerModel.transform.position.y;

        towerModel.transform.LookAt(lookPos);
    }

    // ===============================
    // Build / Upgrade / Delete
    // ===============================
    void BuildTower(string type)
    {
        if (currentLevel > 0) return;

        if (!towerLevels.ContainsKey(type) || towerLevels[type][0] == null)
            return;

        HideAllTowers();

        towerLevels[type][0].SetActive(true);

        currentTowerType = type;
        currentLevel = 1;

        RotateTowardRiver();
        UpdateRangeDisplay();
    }

    void UpgradeTower()
    {
        if (currentLevel == 0) return;

        string type = currentTowerType;
        int nextLevel = currentLevel + 1;

        if (!towerLevels.ContainsKey(type)) return;
        if (towerLevels[type].Count < nextLevel) return;
        if (towerLevels[type][nextLevel - 1] == null) return;

        towerLevels[type][currentLevel - 1].SetActive(false);
        towerLevels[type][nextLevel - 1].SetActive(true);

        currentLevel = nextLevel;

        RotateTowardRiver();
        UpdateRangeDisplay();
    }

    void DeleteTower()
    {
        if (currentLevel == 0) return;

        towerLevels[currentTowerType][currentLevel - 1].SetActive(false);

        currentTowerType = "";
        currentLevel = 0;

        HideAllRanges();
    }

    void HideAllTowers()
    {
        foreach (var type in towerLevels.Keys)
        {
            foreach (var model in towerLevels[type])
            {
                if (model != null) model.SetActive(false);
            }
        }
    }

    void DeselectSpot()
    {
        isSelected = false;

        if (circleRenderer != null)
        {
            circleRenderer.material.color = defaultColor;
            circleRenderer.transform.localScale = circleOriginalScale;
        }

        HideAllRanges();
    }
}
