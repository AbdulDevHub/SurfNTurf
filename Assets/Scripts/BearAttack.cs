using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BearAttack : MonoBehaviour
{
    [Header("Bear Settings")]
    public int bearLevel = 1;

    [Header("Animation")]
    public Animator animator;
    public float attackAnimationDuration = 1f;

    [Header("Range Trigger")]
    public Collider rangeTrigger; // Assign Range1 for LV1, Range2 for LV2

    // Level-based stats
    private int damage;
    private float attackInterval;

    private BearRangeTrigger rangeTriggerScript;
    private Enemy currentTarget = null;
    private Coroutine attackCoroutine;

    private void Awake()
    {
        // Set stats based on bear level
        SetStatsForLevel();

        if (rangeTrigger == null)
        {
            Debug.LogError($"{gameObject.name}: Range trigger not assigned!");
            return;
        }

        // Ensure trigger collider is set
        rangeTrigger.isTrigger = true;

        // Get the BearRangeTrigger component from the assigned collider
        rangeTriggerScript = rangeTrigger.GetComponent<BearRangeTrigger>();
        if (rangeTriggerScript == null)
        {
            Debug.LogError($"{gameObject.name}: BearRangeTrigger script not found on range trigger!");
        }
    }

    private void SetStatsForLevel()
    {
        switch (bearLevel)
        {
            case 1:
                damage = 3;
                attackInterval = 3f;
                break;
            case 2:
                damage = 5;
                attackInterval = 2f;
                break;
            case 3: // If you add a level 3 later
                damage = 8;
                attackInterval = 1.5f;
                break;
            default:
                damage = 3;
                attackInterval = 3f;
                Debug.LogWarning($"{gameObject.name}: Unknown bear level {bearLevel}, using default stats");
                break;
        }
    }

    private void OnEnable()
    {
        // Subscribe to THIS specific range trigger's events
        if (rangeTriggerScript != null)
        {
            rangeTriggerScript.OnEnemyEnter += HandleEnemyEnter;
            rangeTriggerScript.OnEnemyExit += HandleEnemyExit;
        }

        // Start the attack routine when enabled
        if (attackCoroutine == null)
        {
            attackCoroutine = StartCoroutine(AttackRoutine());
        }
    }

    private void OnDisable()
    {
        // Unsubscribe from THIS specific range trigger's events
        if (rangeTriggerScript != null)
        {
            rangeTriggerScript.OnEnemyEnter -= HandleEnemyEnter;
            rangeTriggerScript.OnEnemyExit -= HandleEnemyExit;
        }

        // Stop the attack routine when disabled
        if (attackCoroutine != null)
        {
            StopCoroutine(attackCoroutine);
            attackCoroutine = null;
        }

        currentTarget = null;
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

                // 🔊 Play the Bear Attack sound here
                SoundManager.Instance.PlaySound("Bear Attack", transform.position);

                // Wait for attack animation to play
                yield return new WaitForSeconds(attackAnimationDuration);

                // Deal damage after animation plays
                if (currentTarget != null)
                    currentTarget.TakeDamage(damage);

                // Sit animation during cooldown
                animator?.Play("Sit");

                // Wait for remaining cooldown time
                float remainingCooldown = attackInterval - attackAnimationDuration;
                if (remainingCooldown > 0)
                {
                    yield return new WaitForSeconds(remainingCooldown);
                }

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