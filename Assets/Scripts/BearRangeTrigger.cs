using UnityEngine;
using System;

public class BearRangeTrigger : MonoBehaviour
{
    public event Action<Enemy> OnEnemyEnter;
    public event Action<Enemy> OnEnemyExit;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Enemy") && !other.CompareTag("HiddenEnemy"))
            return;

        Enemy enemy = other.GetComponent<Enemy>();
        if (enemy != null)
            OnEnemyEnter?.Invoke(enemy);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Enemy") && !other.CompareTag("HiddenEnemy"))
            return;

        Enemy enemy = other.GetComponent<Enemy>();
        if (enemy != null)
            OnEnemyExit?.Invoke(enemy);
    }
}
