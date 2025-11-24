using UnityEngine;

public class EnemyHoverUI : MonoBehaviour
{
    public GameObject enemyUIPrefab;
    public Vector3 offset = new Vector3(0, 1f, 1f);

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
            // Instantiate the prefab above the enemy
            uiInstance = Instantiate(enemyUIPrefab, transform.position + offset, Quaternion.Euler(90f, 0f, 0f));
            uiController = uiInstance.GetComponent<EnemyUIController>();

            // Initialize UI immediately with enemy values
            if (uiController != null)
                uiController.Initialize(enemy);

            // Hide by default
            uiInstance.SetActive(false);
        }
    }

    private void LateUpdate()
    {
        if (uiInstance == null || enemy == null) return;

        // Follow enemy
        uiInstance.transform.position = transform.position + offset;

        // Keep horizontal
        uiInstance.transform.rotation = Quaternion.Euler(90f, 0f, 0f);

        // Update health each frame
        if (uiController != null)
            uiController.UpdateUI();

        // Show UI only when hovering
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
