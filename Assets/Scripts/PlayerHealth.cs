using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 100;
    public int currentHealth;

    [Header("UI")]
    public Image healthGreen;   // green fill
    public Image healthRed;     // background red
    public TMP_Text healthText;

    [Header("Animation")]
    public float healthChangeSpeed = 0.5f; // seconds for smooth animation

    private void Start()
    {
        currentHealth = maxHealth;
        UpdateHealthUIInstant();
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        currentHealth = Mathf.Max(currentHealth, 0);

        // Sync to StatManager
        if (StatManager.Instance != null)
            StatManager.Instance.remainingHealth = currentHealth;

        StartCoroutine(AnimateHealthBar());
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
            yield return null;
        }

        // Ensure final value is exact
        healthGreen.fillAmount = targetFill;

        UpdateHealthText();
    }

    private void UpdateHealthUIInstant()
    {
        healthGreen.fillAmount = (float)currentHealth / maxHealth;
        UpdateHealthText();
    }

    private void UpdateHealthText()
    {
        if (healthText != null)
            healthText.text = $"{currentHealth} / {maxHealth}";
    }
}
