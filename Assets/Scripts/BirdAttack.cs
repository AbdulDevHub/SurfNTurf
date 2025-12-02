using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BirdAttack : MonoBehaviour
{
    [Header("Bird Settings")]
    public int birdLevel = 1;

    [Header("Range Trigger")]
    public Collider rangeTrigger;

    [Header("Movement & Attack")]
    public float attackMoveSpeed = 6f;
    public float returnMoveSpeed = 4f;
    public float attackDistance = 0.5f;
    public float rotationSpeed = 10f;

    private int damage;
    private float attackInterval;

    private List<Enemy> enemiesInRange = new List<Enemy>();
    private Enemy currentTarget;
    private Coroutine attackRoutine;

    private Vector3 originalPosition;
    private Quaternion originalRotation;

    private BirdRangeTrigger rangeTriggerScript;

    private void Awake()
    {
        originalPosition = transform.position;
        originalRotation = transform.rotation;

        SetStatsForLevel();

        if (rangeTrigger == null)
        {
            Debug.LogError($"{gameObject.name}: No range trigger assigned!");
            return;
        }

        rangeTrigger.isTrigger = true;
        rangeTriggerScript = rangeTrigger.GetComponent<BirdRangeTrigger>();

        if (rangeTriggerScript == null)
            Debug.LogError($"{gameObject.name}: BirdRangeTrigger missing on range trigger!");
    }

    private void SetStatsForLevel()
    {
        switch (birdLevel)
        {
            case 1: damage = 1; attackInterval = 2f; break;
            case 2: damage = 2; attackInterval = 2f; break;
            case 3: damage = 4; attackInterval = 1f; break;
            default:
                damage = 1; attackInterval = 2f;
                Debug.LogWarning($"Invalid birdLevel {birdLevel}, using defaults.");
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

        DetectInitialEnemies();

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

        ResetTransform();
    }

    private void ResetTransform()
    {
        transform.position = originalPosition;
        transform.rotation = originalRotation;
    }

    // ——————————————————————————————————————————————
    // TRIGGER EVENTS
    // ——————————————————————————————————————————————

    private void HandleEnemyEnter(Enemy enemy)
    {
        if (!CanSeeEnemy(enemy))
            return;

        if (!enemiesInRange.Contains(enemy))
            enemiesInRange.Add(enemy);

        if (currentTarget == null)
            currentTarget = SelectNextTarget();
    }

    private void HandleEnemyExit(Enemy enemy)
    {
        enemiesInRange.Remove(enemy);

        if (currentTarget == enemy)
            currentTarget = null;
    }

    private bool CanSeeEnemy(Enemy e)
    {
        return !(e.CompareTag("HiddenEnemy") && birdLevel < 2);
    }

    // ——————————————————————————————————————————————
    // MAIN ATTACK LOOP
    // ——————————————————————————————————————————————

    private IEnumerator AttackLoop()
    {
        while (true)
        {
            CleanupNullEnemies();

            if (currentTarget == null)
                currentTarget = SelectNextTarget();

            if (currentTarget == null)
            {
                yield return null;
                continue;
            }

            yield return StartCoroutine(DoAttackSequence());

            if (currentTarget != null && currentTarget.CurrentHealth <= 0)
            {
                enemiesInRange.Remove(currentTarget);
                currentTarget = null;
            }

            yield return null;
        }
    }

    // ——————————————————————————————————————————————
    // ATTACK SEQUENCE
    // ——————————————————————————————————————————————

    private IEnumerator DoAttackSequence()
    {
        if (currentTarget == null || !CanSeeEnemy(currentTarget))
            yield break;

        SoundManager.Instance.PlaySound("Bird Attack", transform.position);

        // Move to enemy
        yield return StartCoroutine(MoveToEnemy());

        // Deal damage IF still valid target
        if (currentTarget != null)
            currentTarget.TakeDamage(damage);

        // Attack cooldown
        yield return new WaitForSeconds(attackInterval);

        // Return home
        yield return StartCoroutine(ReturnHome());
    }

    private IEnumerator MoveToEnemy()
    {
        while (currentTarget != null)
        {
            Vector3 enemyPos = currentTarget.transform.position;
            Vector3 dir = (enemyPos - transform.position).normalized;
            Vector3 attackPos = enemyPos - dir * attackDistance;

            // Rotate toward enemy
            Quaternion lookRot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, rotationSpeed * Time.deltaTime);

            // Move toward attack position
            transform.position = Vector3.MoveTowards(transform.position, attackPos, attackMoveSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, attackPos) < 0.1f)
                break;

            yield return null;
        }
    }

    private IEnumerator ReturnHome()
    {
        while (Vector3.Distance(transform.position, originalPosition) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                originalPosition,
                returnMoveSpeed * Time.deltaTime
            );

            yield return null;
        }

        ResetTransform();
    }

    // ——————————————————————————————————————————————
    // TARGET MANAGEMENT
    // ——————————————————————————————————————————————

    private void CleanupNullEnemies()
    {
        for (int i = enemiesInRange.Count - 1; i >= 0; i--)
        {
            if (enemiesInRange[i] == null || enemiesInRange[i].CurrentHealth <= 0)
                enemiesInRange.RemoveAt(i);
        }
    }

    private Enemy SelectNextTarget()
    {
        if (enemiesInRange.Count == 0)
            return null;

        // Choose closest valid enemy
        float minDist = float.MaxValue;
        Enemy closest = null;

        foreach (var enemy in enemiesInRange)
        {
            if (enemy == null) continue;
            if (!CanSeeEnemy(enemy)) continue;

            float dist = (enemy.transform.position - originalPosition).sqrMagnitude;

            if (dist < minDist)
            {
                minDist = dist;
                closest = enemy;
            }
        }

        return closest;
    }

    private void DetectInitialEnemies()
    {
        // Use the collider's AABB bounds to find enemies
        Collider[] hits = Physics.OverlapBox(
            rangeTrigger.bounds.center,
            rangeTrigger.bounds.extents,
            rangeTrigger.transform.rotation
        );

        foreach (var hit in hits)
        {
            if (!hit.CompareTag("Enemy") && !hit.CompareTag("HiddenEnemy"))
                continue;

            Enemy enemy = hit.GetComponent<Enemy>();
            if (enemy != null && CanSeeEnemy(enemy))
                HandleEnemyEnter(enemy);
        }
    }
}
