using UnityEngine;
using System;

public class BirdRangeTrigger : MonoBehaviour
{
    public event Action<Enemy> OnEnemyEnter;
    public event Action<Enemy> OnEnemyExit;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Enemy") &&
            !other.CompareTag("HiddenEnemy") &&
            !other.CompareTag("Boss"))
            return;

        if (other.TryGetComponent(out Enemy e))
            OnEnemyEnter?.Invoke(e);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Enemy") &&
            !other.CompareTag("HiddenEnemy") &&
            !other.CompareTag("Boss"))
            return;

        if (other.TryGetComponent(out Enemy e))
            OnEnemyExit?.Invoke(e);
    }
}
