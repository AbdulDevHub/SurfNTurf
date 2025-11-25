using System.Collections;
using UnityEngine;
using TMPro;

public class WaveManager : MonoBehaviour
{
    public Wave[] waves;
    private int waveIndex = 0;

    [Header("UI")]
    public TMP_Text waveText;
    public TMP_Text countdownText;

    [Header("Wave Message UI")]
    public GameObject waveMessagePanel;   // UI Panel to show after wave
    public TMP_Text waveMessageText;      // Text inside the panel

    [Header("Settings")]
    public float timeBeforeFirstWave = 5f;
    public float spawnInterval = 0.5f;

    private float countdown = 0f;
    private bool waveInProgress = false;

    private void Start()
    {
        countdown = timeBeforeFirstWave;
        UpdateWaveUI();
        UpdateCountdownUI();

        if (waveMessagePanel != null)
            waveMessagePanel.SetActive(false);
    }

    private void Update()
    {
        if (waveIndex >= waves.Length) return;

        if (!waveInProgress)
        {
            countdown -= Time.deltaTime;
            UpdateCountdownUI();

            if (countdown <= 0f)
            {
                StartCoroutine(SpawnWave(waves[waveIndex]));
            }
        }
    }

    private IEnumerator SpawnWave(Wave wave)
    {
        waveInProgress = true;
        countdownText.text = ""; 

        UpdateWaveUI();

        // Spawn all enemies in groups
        foreach (EnemyGroup group in wave.enemies)
        {
            for (int i = 0; i < group.count; i++)
            {
                EnemyPath path = wave.availablePaths[Random.Range(0, wave.availablePaths.Length)];
                GameObject enemyGO = Instantiate(group.enemyPrefab, path.GetSpawnPoint(), Quaternion.identity);
                Enemy enemy = enemyGO.GetComponent<Enemy>();
                if (enemy != null)
                    enemy.pathToFollow = path;

                yield return new WaitForSeconds(spawnInterval);
            }
        }

        // Wait until all enemies are dead
        while (EnemyManager.aliveEnemies > 0)
            yield return null;

        // Wave completed
        StartCoroutine(HandleWaveCompletion(wave));

        waveIndex++;
        waveInProgress = false;

        if (waveIndex < waves.Length)
        {
            countdown = wave.delayAfterWave;
            UpdateCountdownUI();
        }
        else
        {
            countdownText.text = "All waves completed!";
        }
    }

    private IEnumerator HandleWaveCompletion(Wave wave)
    {
        // Only show UI if wave has a message
        if (!string.IsNullOrWhiteSpace(wave.waveMessage) && waveMessagePanel != null)
        {
            waveMessagePanel.SetActive(true);

            if (waveMessageText != null)
                waveMessageText.text = wave.waveMessage;

            // Use custom time if given; otherwise use delayAfterWave
            // float showTime = (wave.messageDisplayTime > 0)
            //     ? wave.messageDisplayTime
            //     : wave.delayAfterWave;

            float showTime = wave.delayAfterWave;

            yield return new WaitForSeconds(showTime);

            waveMessagePanel.SetActive(false);
        }
    }

    private void UpdateWaveUI()
    {
        if (waveText != null)
            waveText.text = $"Wave {Mathf.Min(waveIndex + 1, waves.Length)} / {waves.Length}";
    }

    private void UpdateCountdownUI()
    {
        if (countdownText != null)
        {
            if (!waveInProgress)
                countdownText.text = $"Next wave in {Mathf.Ceil(countdown)} seconds...";
            else
                countdownText.text = "";
        }
    }
}
