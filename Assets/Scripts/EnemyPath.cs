using UnityEngine;

public class EnemyPath : MonoBehaviour
{
    public Transform[] nodes;

    private void Awake()
    {
        nodes = new Transform[transform.childCount];
        for (int i = 0; i < transform.childCount; i++)
            nodes[i] = transform.GetChild(i);
    }

    // First node is the spawn point
    public Vector3 GetSpawnPoint()
    {
        if (nodes.Length > 0)
            return nodes[0].position;
        return Vector3.zero;
    }
}
