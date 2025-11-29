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

    [Header("Dam Break Settings")]
    [Tooltip("Wave number (1-based) that breaks Dam 1. Set to 0 to disable.")]
    public int dam1BreakWave = 1;

    [Tooltip("Wave number (1-based) that breaks Dam 2. Set to 0 to disable.")]
    public int dam2BreakWave = 2;

    [Header("Dam Break Messages")]
    [Tooltip("Text added to the wave message when Dam 1 breaks.")]
    public string dam1Message = "The Dam Broke!";

    [Tooltip("Text added to the wave message when Dam 2 breaks.")]
    public string dam2Message = "The Dam Broke!";

    [Header("Dams")]
    public GameObject dam1;
    public GameObject dam2;

    [Header("Dam Meshes")]
    public MeshRenderer dam1Mesh;
    public MeshRenderer dam2Mesh;

    [Header("Dam Explosion Effects")]
    public ParticleSystem dam1Explosion;
    public ParticleSystem dam2Explosion;

    [Header("Camera Shake Settings")]
    public float shakeDuration = 0.3f;
    public float shakeMagnitude = 0.3f;

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

        // Build master enemy list
        List<GameObject> enemyPool = new List<GameObject>();
        foreach (EnemyGroup group in wave.enemies)
        {
            for (int i = 0; i < group.count; i++)
                enemyPool.Add(group.enemyPrefab);
        }

        // Shuffle list
        for (int i = 0; i < enemyPool.Count; i++)
        {
            GameObject temp = enemyPool[i];
            int rand = Random.Range(i, enemyPool.Count);
            enemyPool[i] = enemyPool[rand];
            enemyPool[rand] = temp;
        }

        // Spawn enemies
        foreach (GameObject prefab in enemyPool)
        {
            EnemyPath path = wave.availablePaths[Random.Range(0, wave.availablePaths.Length)];
            GameObject enemyGO = Instantiate(prefab, path.GetSpawnPoint(), Quaternion.identity);

            Enemy enemy = enemyGO.GetComponent<Enemy>();
            if (enemy != null)
                enemy.pathToFollow = path;

            yield return new WaitForSeconds(spawnInterval);
        }

        // Wait until all enemies die
        while (EnemyManager.aliveEnemies > 0)
            yield return null;

        // Handle wave completion
        yield return StartCoroutine(HandleWaveCompletion(waveIndex));

        waveIndex++;
        waveInProgress = false;

        if (waveIndex < waves.Length)
        {
            countdown = waves[waveIndex - 1].delayAfterWave;
            UpdateCountdownUI();
        }
        else
        {
            countdownText.text = "All waves completed!";
            StartCoroutine(HandleAllWavesCompleted());
        }
    }

    private IEnumerator HandleWaveCompletion(int completedWaveIndex)
    {
        int completedWaveNumber = completedWaveIndex + 1;

        bool dam1BrokeThisWave = (dam1BreakWave > 0 && completedWaveNumber == dam1BreakWave);
        bool dam2BrokeThisWave = (dam2BreakWave > 0 && completedWaveNumber == dam2BreakWave);

        // --- PLAY THE CORRECT SOUND ---
        if (dam1BrokeThisWave || dam2BrokeThisWave) {
            Vector3 pos = dam1BrokeThisWave ? dam1.transform.position : dam2.transform.position;
            SoundManager.Instance.PlaySound("Dam Explosion", pos);
        }
        else {
            SoundManager.Instance.PlaySound("Wave End Sound");
        }
        // ------------------------------

        // DAM 1
        if (dam1BrokeThisWave && dam1 != null)
            yield return StartCoroutine(BreakDam(dam1, dam1Mesh, dam1Explosion));

        // DAM 2
        if (dam2BrokeThisWave && dam2 != null)
            yield return StartCoroutine(BreakDam(dam2, dam2Mesh, dam2Explosion));

        // BASE WAVE MESSAGE
        Wave wave = waves[completedWaveIndex];
        string finalMessage = wave.waveMessage;

        // OPTIONAL DAM BREAK MESSAGE
        if (dam1BrokeThisWave && !string.IsNullOrWhiteSpace(dam1Message))
            finalMessage += " " + dam1Message;

        if (dam2BrokeThisWave && !string.IsNullOrWhiteSpace(dam2Message))
            finalMessage += " " + dam2Message;

        // DISPLAY WAVE MESSAGE
        if (!string.IsNullOrWhiteSpace(finalMessage) && waveMessagePanel != null)
        {
            waveMessagePanel.SetActive(true);

            if (waveMessageText != null)
                waveMessageText.text = finalMessage;

            yield return new WaitForSeconds(wave.delayAfterWave);

            waveMessagePanel.SetActive(false);
        }
    }

    private IEnumerator BreakDam(GameObject dam, MeshRenderer damMesh, ParticleSystem explosion)
    {
        Vector3 damPos = dam.transform.position;

        // Camera moves to dam and STAYS there
        if (CameraShake.Instance != null)
            yield return StartCoroutine(CameraShake.Instance.FocusOnPoint(damPos, 12f, 0.7f));

        // Hide dam mesh
        if (damMesh != null)
            damMesh.enabled = false;

        // Play explosion
        if (explosion != null)
            explosion.Play();

        // Shake camera without snapping to old position
        if (CameraShake.Instance != null)
            StartCoroutine(CameraShake.Instance.Shake(shakeDuration, shakeMagnitude));

        // Wait for explosion to finish
        if (explosion != null)
            yield return new WaitForSeconds(explosion.main.duration);

        // Stop explosion
        if (explosion != null)
            explosion.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
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

    private IEnumerator HandleAllWavesCompleted()
    {
        yield return new WaitForSeconds(2f);
        UnityEngine.SceneManagement.SceneManager.LoadScene("EndScene");
    }
}
