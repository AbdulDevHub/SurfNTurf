using UnityEngine;
using System;

public class BirdRangeTrigger : MonoBehaviour
{
    // Non-static instance events
    public event Action<Enemy> OnEnemyEnter;
    public event Action<Enemy> OnEnemyExit;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy") || other.CompareTag("HiddenEnemy"))
        {
            Enemy enemy = other.GetComponent<Enemy>();
            if (enemy != null)
                OnEnemyEnter?.Invoke(enemy);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Enemy") || other.CompareTag("HiddenEnemy"))
        {
            Enemy enemy = other.GetComponent<Enemy>();
            if (enemy != null)
                OnEnemyExit?.Invoke(enemy);
        }
    }
}