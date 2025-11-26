using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EnemyUIController : MonoBehaviour
{
    public Image healthGreen;
    public TMP_Text healthText;
    public TMP_Text nameText;

    private Enemy enemy;

    [Header("Animation Settings")]
    public float healthLerpSpeed = 5f; // higher = faster animation
    private float displayedHealthFraction = 1f;

    public void Initialize(Enemy attachedEnemy)
    {
        enemy = attachedEnemy;

        // Immediately set UI values
        displayedHealthFraction = (float)enemy.CurrentHealth / enemy.MaxHealth;
        UpdateUIInstant();
    }

    public void UpdateUI()
    {
        if (enemy == null) return;

        // Smoothly animate the health bar
        float targetHealthFraction = (float)enemy.CurrentHealth / enemy.MaxHealth;
        displayedHealthFraction = Mathf.Lerp(displayedHealthFraction, targetHealthFraction, Time.deltaTime * healthLerpSpeed);
        if (healthGreen != null)
            healthGreen.fillAmount = displayedHealthFraction;

        // Update health text
        if (healthText != null)
            healthText.text = $"{enemy.CurrentHealth} / {enemy.MaxHealth}";

        // Update name
        if (nameText != null)
            nameText.text = enemy.enemyName;
    }

    private void UpdateUIInstant()
    {
        if (healthGreen != null)
            healthGreen.fillAmount = (float)enemy.CurrentHealth / enemy.MaxHealth;

        if (healthText != null)
            healthText.text = $"{enemy.CurrentHealth} / {enemy.MaxHealth}";

        if (nameText != null)
            nameText.text = enemy.enemyName;
    }
}
