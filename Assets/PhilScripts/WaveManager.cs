using System.Collections;
using System.Collections.Generic;
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
    public GameObject waveMessagePanel;
    public TMP_Text waveMessageText;

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

        // ---------------------------
        // BUILD LIST OF ALL ENEMIES
        // ---------------------------
        List<GameObject> enemyPool = new List<GameObject>();

        foreach (EnemyGroup group in wave.enemies)
        {
            for (int i = 0; i < group.count; i++)
                enemyPool.Add(group.enemyPrefab);
        }

        // ---------------------------
        // SHUFFLE THE ENEMY LIST
        // ---------------------------
        for (int i = 0; i < enemyPool.Count; i++)
        {
            GameObject temp = enemyPool[i];
            int rand = Random.Range(i, enemyPool.Count);
            enemyPool[i] = enemyPool[rand];
            enemyPool[rand] = temp;
        }

        // ---------------------------
        // SPAWN SHUFFLED ENEMIES
        // ---------------------------
        foreach (GameObject prefab in enemyPool)
        {
            EnemyPath path = wave.availablePaths[Random.Range(0, wave.availablePaths.Length)];
            GameObject enemyGO = Instantiate(prefab, path.GetSpawnPoint(), Quaternion.identity);

            Enemy enemy = enemyGO.GetComponent<Enemy>();
            if (enemy != null)
                enemy.pathToFollow = path;

            yield return new WaitForSeconds(spawnInterval);
        }

        // Wait for all enemies to die
        while (EnemyManager.aliveEnemies > 0)
            yield return null;

        // Handle wave completion UI message
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
        if (!string.IsNullOrWhiteSpace(wave.waveMessage) && waveMessagePanel != null)
        {
            waveMessagePanel.SetActive(true);

            if (waveMessageText != null)
                waveMessageText.text = wave.waveMessage;

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
