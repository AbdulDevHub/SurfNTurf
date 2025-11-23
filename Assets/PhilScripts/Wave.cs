using UnityEngine;

[System.Serializable]
public class EnemyGroup
{
    public GameObject enemyPrefab;
    public int count;
}

[System.Serializable]
public class Wave
{
    public EnemyGroup[] enemies;         // Enemy types in this wave
    public EnemyPath[] availablePaths;   // Paths this wave can use
    public float spawnInterval = 0.5f;
    public float delayAfterWave = 3f;
}
