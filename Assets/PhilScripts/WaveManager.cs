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

    [Header("Settings")]
    public float timeBeforeFirstWave = 5f;

    private float countdown = 0f;
    private bool waveInProgress = false;

    private void Start()
    {
        countdown = timeBeforeFirstWave;
        UpdateWaveUI();
        UpdateCountdownUI();
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
        countdownText.text = ""; // hide countdown during wave

        UpdateWaveUI(); // show current wave number

        foreach (EnemyGroup group in wave.enemies)
        {
            for (int i = 0; i < group.count; i++)
            {
                EnemyPath path = wave.availablePaths[Random.Range(0, wave.availablePaths.Length)];
                GameObject enemyGO = Instantiate(group.enemyPrefab, path.GetSpawnPoint(), Quaternion.identity);
                Enemy enemy = enemyGO.GetComponent<Enemy>();
                if (enemy != null)
                    enemy.pathToFollow = path;

                yield return new WaitForSeconds(group.enemyPrefab.GetComponent<Enemy>().speed / 2f); // optional pacing
            }
        }

        // Wait until all enemies are dead
        while (EnemyManager.aliveEnemies > 0)
            yield return null;

        // Wave finished
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
