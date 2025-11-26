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
    public EnemyGroup[] enemies;
    public EnemyPath[] availablePaths;

    public float delayAfterWave = 5f;

    // [Header("Wave Message")]
    // [TextArea]
    public string waveMessage;         // leave empty = no UI
    // public float messageDisplayTime;   // 0 = use delayAfterWave
}
