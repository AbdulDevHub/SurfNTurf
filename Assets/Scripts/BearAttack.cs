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
    public Collider rangeTrigger;

    // Level stats
    private int damage;
    private float attackInterval;

    // Target & enemy tracking
    private List<Enemy> enemiesInRange = new List<Enemy>();
    private Enemy currentTarget = null;
    private Coroutine attackRoutine;

    private BearRangeTrigger rangeTriggerScript;

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
            Debug.LogError($"{gameObject.name}: BearRangeTrigger script missing on range trigger!");
    }

    private void SetStatsForLevel()
    {
        switch (bearLevel)
        {
            case 1: damage = 3; attackInterval = 3f; break;
            case 2: damage = 5; attackInterval = 2f; break;
            case 3: damage = 8; attackInterval = 1.5f; break;
            default:
                damage = 3;
                attackInterval = 3f;
                Debug.LogWarning($"{gameObject.name}: Unknown bearLevel {bearLevel}, using defaults.");
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

        attackRoutine = StartCoroutine(AttackLoop());
    }

    private void OnDisable()
    {
        if (rangeTriggerScript != null)
        {
            rangeTriggerScript.OnEnemyEnter -= HandleEnemyEnter;
            rangeTriggerScript.OnEnemyExit -= HandleEnemyExit;
        }

        if (attackRoutine != null)
            StopCoroutine(attackRoutine);

        enemiesInRange.Clear();
        currentTarget = null;
    }

    // ——————————————————————————————————————
    // Trigger Management
    // ——————————————————————————————————————

    private void HandleEnemyEnter(Enemy enemy)
    {
        if (!CanSeeEnemy(enemy))
            return;

        if (!enemiesInRange.Contains(enemy))
            enemiesInRange.Add(enemy);

        if (currentTarget == null)
            currentTarget = enemy;
    }

    private void HandleEnemyExit(Enemy enemy)
    {
        enemiesInRange.Remove(enemy);

        if (currentTarget == enemy)
            currentTarget = null;
    }

    private bool CanSeeEnemy(Enemy enemy)
    {
        return !(enemy.CompareTag("HiddenEnemy") && bearLevel != 2);
    }

    // ——————————————————————————————————————
    // Update: Rotation
    // ——————————————————————————————————————

    private void Update()
    {
        if (currentTarget != null)
            RotateToward(currentTarget);
    }

    private void RotateToward(Enemy target)
    {
        if (!CanSeeEnemy(target))
            return;

        Vector3 dir = target.transform.position - transform.position;
        dir.y = 0;

        if (dir.sqrMagnitude > 0.01f)
        {
            Quaternion lookRot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                lookRot,
                Time.deltaTime * 8f
            );
        }
    }

    // ——————————————————————————————————————
    // Attack Loop
    // ——————————————————————————————————————

    private IEnumerator AttackLoop()
    {
        while (true)
        {
            CleanupNullEnemies();

            if (currentTarget == null)
                currentTarget = SelectNextTarget();

            if (currentTarget == null)
            {
                animator?.Play("Idle");
                yield return null;
                continue;
            }

            yield return StartCoroutine(PerformAttack());

            if (currentTarget != null && currentTarget.CurrentHealth <= 0)
            {
                enemiesInRange.Remove(currentTarget);
                currentTarget = null;
            }

            yield return null;
        }
    }

    private IEnumerator PerformAttack()
    {
        string[] attacks = { "Attack1", "Attack2", "Attack3", "Attack5" };
        animator?.Play(attacks[Random.Range(0, attacks.Length)]);
        SoundManager.Instance.PlaySound("Bear Attack", transform.position);

        yield return new WaitForSeconds(attackAnimationDuration);

        if (currentTarget != null)
            currentTarget.TakeDamage(damage);

        animator?.Play("Sit");

        float cooldown = attackInterval - attackAnimationDuration;
        if (cooldown > 0)
            yield return new WaitForSeconds(cooldown);
    }

    // ——————————————————————————————————————
    // Target Selection & Cleanup
    // ——————————————————————————————————————

    private void CleanupNullEnemies()
    {
        for (int i = enemiesInRange.Count - 1; i >= 0; i--)
        {
            if (enemiesInRange[i] == null)
                enemiesInRange.RemoveAt(i);
        }
    }

    private Enemy SelectNextTarget()
    {
        if (enemiesInRange.Count == 0)
            return null;

        // Choose closest enemy to the bear
        float minDist = float.MaxValue;
        Enemy closest = null;

        foreach (var enemy in enemiesInRange)
        {
            if (enemy == null) continue;
            if (!CanSeeEnemy(enemy)) continue;

            float dist = (enemy.transform.position - transform.position).sqrMagnitude;
            if (dist < minDist)
            {
                minDist = dist;
                closest = enemy;
            }
        }

        return closest;
    }
}
