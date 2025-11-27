using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BearAttack : MonoBehaviour
{
    [Header("Bear Settings")]
    public int bearLevel = 1;
    public int damage = 3;
    public float attackInterval = 3f;

    [Header("Animation")]
    public Animator animator;

    [Header("Range Trigger")]
    public Collider rangeTrigger; // Assign Range1 collider here

    private Enemy currentTarget = null;

    private void Start()
    {
        if (rangeTrigger == null)
        {
            Debug.LogError("Range trigger not assigned!");
            return;
        }

        // Ensure trigger collider is set
        rangeTrigger.isTrigger = true;

        StartCoroutine(AttackRoutine());
    }

    private void OnEnable()
    {
        // Subscribe to trigger events on the range object
        BearRangeTrigger.OnEnemyEnter += HandleEnemyEnter;
        BearRangeTrigger.OnEnemyExit += HandleEnemyExit;
    }

    private void OnDisable()
    {
        BearRangeTrigger.OnEnemyEnter -= HandleEnemyEnter;
        BearRangeTrigger.OnEnemyExit -= HandleEnemyExit;
    }

    private void HandleEnemyEnter(Enemy enemy)
    {
        if (currentTarget == null)
            currentTarget = enemy;
    }

    private void HandleEnemyExit(Enemy enemy)
    {
        if (currentTarget == enemy)
            currentTarget = null;
    }

    private IEnumerator AttackRoutine()
    {
        while (true)
        {
            if (currentTarget != null)
            {
                // Play random attack animation
                string[] attacks = { "Attack1", "Attack2", "Attack3", "Attack5" };
                string attackAnim = attacks[Random.Range(0, attacks.Length)];
                animator?.Play(attackAnim);

                // Deal damage
                if (currentTarget != null)
                    currentTarget.TakeDamage(damage);

                // Sit animation during cooldown
                animator?.Play("Sit");

                yield return new WaitForSeconds(attackInterval);

                // Reset if enemy is dead
                if (currentTarget != null && currentTarget.CurrentHealth <= 0)
                    currentTarget = null;
            }
            else
            {
                animator?.Play("Idle");
                yield return null;
            }
        }
    }
}
