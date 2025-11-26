using UnityEngine;

public class EnemyHoverUI : MonoBehaviour
{
    [Header("UI Settings")]
    public GameObject enemyUIPrefab;
    public Vector3 offset = new Vector3(0, 1f, 1f);

    [Header("Scaling")]
    [Tooltip("How big the UI should appear on the screen. Higher = larger UI.")]
    public float scaleFactor = 0.1f;

    private GameObject uiInstance;
    private EnemyUIController uiController;
    private Enemy enemy;
    private Camera mainCamera;

    private void Start()
    {
        enemy = GetComponent<Enemy>();
        mainCamera = Camera.main;

        if (enemyUIPrefab != null && enemy != null)
        {
            // Spawn UI
            uiInstance = Instantiate(enemyUIPrefab, transform.position + offset, Quaternion.Euler(90f, 0f, 0f));
            uiController = uiInstance.GetComponent<EnemyUIController>();

            if (uiController != null)
                uiController.Initialize(enemy);

            uiInstance.SetActive(false);
        }
    }

    private void LateUpdate()
    {
        if (uiInstance == null || enemy == null) return;

        // Keep UI following enemy
        uiInstance.transform.position = transform.position + offset;

        // Keep UI flat (horizontal)
        uiInstance.transform.rotation = Quaternion.Euler(90f, 0f, 0f);

        // Maintain consistent on-screen size
        float distance = Vector3.Distance(mainCamera.transform.position, uiInstance.transform.position);
        float scale = distance * scaleFactor * 0.01f; // tweakable scaling formula
        uiInstance.transform.localScale = Vector3.one * scale;

        // Update displayed health
        if (uiController != null)
            uiController.UpdateUI();

        // Visibility when hovered
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
            uiInstance.SetActive(hit.collider.gameObject == gameObject);
        else
            uiInstance.SetActive(false);
    }

    private void OnDestroy()
    {
        if (uiInstance != null)
            Destroy(uiInstance);
    }
}
