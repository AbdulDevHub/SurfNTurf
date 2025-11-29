using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 100;
    public int currentHealth;

    [Header("UI")]
    public Image healthGreen;   // Green fill
    public Image healthRed;     // Background red (shakes)
    public TMP_Text healthText;

    [Header("Animation")]
    public float healthChangeSpeed = 0.5f; // seconds for smooth animation
    public float shakeDuration = 0.2f;
    public float shakeStrength = 6f;

    private Vector2 redOriginalPos;
    private Coroutine healthAnimRoutine;
    private Coroutine shakeRoutine;

    private void Start()
    {
        currentHealth = maxHealth;
        UpdateHealthUIInstant();

        if (healthRed != null)
            redOriginalPos = healthRed.rectTransform.anchoredPosition;
    }

    public void TakeDamage(int amount)
    {
        int oldHealth = currentHealth;

        currentHealth -= amount;
        currentHealth = Mathf.Max(currentHealth, 0);

        // Sync to StatManager
        if (StatManager.Instance != null)
            StatManager.Instance.remainingHealth = currentHealth;

        // Smooth shrink animation
        if (healthAnimRoutine != null)
            StopCoroutine(healthAnimRoutine);

        healthAnimRoutine = StartCoroutine(AnimateHealthBar());

        // Trigger shake if health actually decreased
        if (currentHealth < oldHealth)
        {
            SoundManager.Instance.PlaySound("Lose Health");

            if (shakeRoutine != null)
                StopCoroutine(shakeRoutine);

            shakeRoutine = StartCoroutine(ShakeHealthBar());
        }
    }

    private IEnumerator AnimateHealthBar()
    {
        float startFill = healthGreen.fillAmount;
        float targetFill = (float)currentHealth / maxHealth;
        float elapsed = 0f;

        while (elapsed < healthChangeSpeed)
        {
            elapsed += Time.deltaTime;
            healthGreen.fillAmount = Mathf.Lerp(startFill, targetFill, elapsed / healthChangeSpeed);

            // Smoothly update text as well
            int displayedHealth = Mathf.RoundToInt(Mathf.Lerp(startFill * maxHealth, currentHealth, elapsed / healthChangeSpeed));
            healthText.text = $"{displayedHealth} / {maxHealth}";

            yield return null;
        }

        healthGreen.fillAmount = targetFill;
        UpdateHealthText();
    }

    private IEnumerator ShakeHealthBar()
    {
        RectTransform rt = healthRed.rectTransform;
        rt.anchoredPosition = redOriginalPos;

        float elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            elapsed += Time.deltaTime;
            float strength = shakeStrength * (1f - (elapsed / shakeDuration)); // fade out

            Vector2 offset = Random.insideUnitCircle * strength;
            rt.anchoredPosition = redOriginalPos + offset;

            yield return null;
        }

        // ALWAYS restore perfectly
        rt.anchoredPosition = redOriginalPos;
    }

    private void UpdateHealthUIInstant()
    {
        float fill = (float)currentHealth / maxHealth;
        healthGreen.fillAmount = fill;
        UpdateHealthText();
    }

    private void UpdateHealthText()
    {
        if (healthText != null)
            healthText.text = $"{currentHealth} / {maxHealth}";
    }
}
