using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class StatManager : MonoBehaviour
{
    public static StatManager Instance;

    [Header("UI References")]
    public TMP_Text scalesText;

    [Header("Default Stats")]
    public int defaultScales = 10000;

    // Stats
    public float totalTime = 0f;
    public int remainingScales;
    public int remainingHealth;
    public int totalScore = 0;

    private PlayerHealth playerHealth;

    // Animation settings
    private Coroutine scaleAnimationCoroutine;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;

        remainingScales = defaultScales;
    }

    private void Start()
    {
        playerHealth = FindAnyObjectByType<PlayerHealth>();
        UpdateScalesUI();
    }

    private void Update()
    {
        if (SceneManager.GetActiveScene().name == "Game")
            totalTime += Time.deltaTime;

        if (playerHealth == null)
            playerHealth = FindAnyObjectByType<PlayerHealth>();

        if (playerHealth != null)
            remainingHealth = playerHealth.currentHealth;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Game")
        {
            playerHealth = FindAnyObjectByType<PlayerHealth>();
            totalTime = 0f;
            totalScore = 0;
            remainingScales = defaultScales;
            UpdateScalesUI();

            if (playerHealth != null)
                remainingHealth = playerHealth.currentHealth;
        }
        else if (scene.name == "EndScene")
        {
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

    // Smoothly animate scales increase
    public void AddScales(int amount)
    {
        if (scaleAnimationCoroutine != null)
            StopCoroutine(scaleAnimationCoroutine);

        scaleAnimationCoroutine = StartCoroutine(AnimateScales(remainingScales, remainingScales + amount));
        remainingScales += amount;
    }

    private IEnumerator AnimateScales(int startValue, int endValue)
    {
        float duration = 0.5f; // half a second for the animation
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            int displayedValue = Mathf.RoundToInt(Mathf.Lerp(startValue, endValue, t));
            if (scalesText != null)
                scalesText.text = displayedValue.ToString();
            yield return null;
        }

        // Ensure final value is correct
        if (scalesText != null)
            scalesText.text = endValue.ToString();
    }

    public void UpdateScalesUI()
    {
        if (scalesText != null)
            scalesText.text = remainingScales.ToString();
    }

    public void AddScore(int amount)
    {
        totalScore += amount;
        AddScales(amount * 10); // Multiply by 10 for scales
    }
}
