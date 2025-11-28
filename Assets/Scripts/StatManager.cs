using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class StatManager : MonoBehaviour
{
    public static StatManager Instance;

    [Header("UI References")]
    public TMP_Text scalesText;

    // Stats
    public float totalTime = 0f;
    public int remainingScales = 1000;
    public int remainingHealth = 0;
    public int totalScore = 0;

    private PlayerHealth playerHealth;

    private void Awake()
    {
        // Singleton
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Listen for scene loads
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Start()
    {
        playerHealth = FindAnyObjectByType<PlayerHealth>();
    }

    private void Update()
    {
        // Track time only during Game Scene
        if (SceneManager.GetActiveScene().name == "Game")
            totalTime += Time.deltaTime;

        // Update health reference
        if (playerHealth == null)
            playerHealth = FindAnyObjectByType<PlayerHealth>();

        if (playerHealth != null)
            remainingHealth = playerHealth.currentHealth;
    }

    // Called whenever a new scene loads
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Reset pointer to player health if we returned to game
        if (scene.name == "Game")
        {
            playerHealth = FindAnyObjectByType<PlayerHealth>();
            totalTime = 0f;
            totalScore = 0;
            remainingScales = 1000;

            // Refresh UI
            UpdateScalesUI();

            if (playerHealth != null)
                remainingHealth = playerHealth.currentHealth;
        }
        else if (scene.name == "EndScene")
        {
            // Apply stats to End Scene UI
            EndScreenScript end = FindAnyObjectByType<EndScreenScript>();

            if (end != null)
            {
                end.ShowScore(
                    totalTime,
                    remainingScales,
                    remainingHealth,
                    totalScore
                );
            }
        }
    }

    // Call this whenever player spends/loses scales
    public void AddScales(int amount)
    {
        remainingScales += amount;
        UpdateScalesUI();
    }

    public void UpdateScalesUI()
    {
        if (scalesText != null)
            scalesText.text = remainingScales.ToString();
    }

    // Called by enemy damage
    public void AddScore(int amount)
    {
        totalScore += amount;
    }
}
