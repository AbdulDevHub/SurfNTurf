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
        SetStatsForLevel();

        if (rangeTrigger == null)
        {
            Debug.LogError($"{gameObject.name}: Range trigger not assigned!");
            return;
        }

        rangeTrigger.isTrigger = true;

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
            case 3:
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
        if (rangeTriggerScript != null)
        {
            rangeTriggerScript.OnEnemyEnter += HandleEnemyEnter;
            rangeTriggerScript.OnEnemyExit += HandleEnemyExit;
        }

        if (attackCoroutine == null)
            attackCoroutine = StartCoroutine(AttackRoutine());
    }

    private void OnDisable()
    {
        if (rangeTriggerScript != null)
        {
            rangeTriggerScript.OnEnemyEnter -= HandleEnemyEnter;
            rangeTriggerScript.OnEnemyExit -= HandleEnemyExit;
        }

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

    private void Update()
    {
        // Rotate every frame toward the enemy
        if (currentTarget != null)
        {
            LookAtTarget();
        }
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

                SoundManager.Instance.PlaySound("Bear Attack", transform.position);

                yield return new WaitForSeconds(attackAnimationDuration);

                if (currentTarget != null)
                    currentTarget.TakeDamage(damage);

                animator?.Play("Sit");

                float remainingCooldown = attackInterval - attackAnimationDuration;
                if (remainingCooldown > 0)
                    yield return new WaitForSeconds(remainingCooldown);

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

    private void LookAtTarget()
    {
        if (currentTarget == null) return;

        Vector3 direction = currentTarget.transform.position - transform.position;
        direction.y = 0; // keep upright

        if (direction.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                Time.deltaTime * 8f
            );
        }
    }
}
