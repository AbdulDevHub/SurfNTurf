using System.Collections;
using UnityEngine;

public class BirdAttack : MonoBehaviour
{
    [Header("Bird Settings")]
    public int birdLevel = 1;

    [Header("Range Trigger")]
    public Collider rangeTrigger; // Assign Range3 for LV1, Range4 for LV2, Range5 for LV3

    [Header("Attack Animation")]
    public float attackMoveSpeed = 6f; // Speed of movement toward enemy
    public float returnMoveSpeed = 4f; // Speed of returning to original position
    public float attackDistance = 0.5f; // How close to get to the enemy
    public float rotationSpeed = 10f; // How fast the bird rotates to face enemy

    // Level-based stats
    private int damage;
    private float attackInterval;

    private BirdRangeTrigger rangeTriggerScript;
    private Enemy currentTarget = null;
    private Coroutine attackCoroutine;

    // Original transform data
    private Vector3 originalPosition;

    private void Awake()
    {
        // Store original position and rotation
        originalPosition = transform.position;

        // Set stats based on bird level
        SetStatsForLevel();

        if (rangeTrigger == null)
        {
            Debug.LogError($"{gameObject.name}: Range trigger not assigned!");
            return;
        }

        // Ensure trigger collider is set
        rangeTrigger.isTrigger = true;

        // Get the BirdRangeTrigger component from the assigned collider
        rangeTriggerScript = rangeTrigger.GetComponent<BirdRangeTrigger>();
        if (rangeTriggerScript == null)
        {
            Debug.LogError($"{gameObject.name}: BirdRangeTrigger script not found on range trigger!");
        }
    }

    private void SetStatsForLevel()
    {
        switch (birdLevel)
        {
            case 1:
                damage = 1;
                attackInterval = 2f;
                break;
            case 2:
                damage = 2;
                attackInterval = 2f;
                break;
            case 3:
                damage = 4;
                attackInterval = 1f;
                break;
            default:
                damage = 1;
                attackInterval = 2f;
                Debug.LogWarning($"{gameObject.name}: Unknown bird level {birdLevel}, using default stats");
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

        // Reset to original transform
        ResetTransform();

        currentTarget = null;
    }

    private void ResetTransform()
    {
        transform.position = originalPosition;
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
                // Play attack animation (move to enemy and back)
                yield return StartCoroutine(AttackAnimation());

                // Deal damage after reaching the enemy
                if (currentTarget != null)
                    currentTarget.TakeDamage(damage);

                // Wait for remaining attack interval
                yield return new WaitForSeconds(attackInterval);

                // Reset if enemy is dead
                if (currentTarget != null && currentTarget.CurrentHealth <= 0)
                    currentTarget = null;
            }
            else
            {
                // No target, just wait for next frame
                yield return null;
            }
        }
    }

    private IEnumerator AttackAnimation()
    {
        if (currentTarget == null) yield break;

        // 🔊 Play Bird Attack sound
        SoundManager.Instance.PlaySound("Bird Attack", transform.position);

        // Phase 1: Look at and move toward enemy
        Vector3 targetPosition = currentTarget.transform.position;
        Vector3 directionToEnemy = (targetPosition - transform.position).normalized;
        Vector3 attackPosition = targetPosition - (directionToEnemy * attackDistance);

        // Rotate toward enemy while moving
        while (Vector3.Distance(transform.position, attackPosition) > 0.1f)
        {
            if (currentTarget == null) break;

            // Update target position in case enemy is moving
            targetPosition = currentTarget.transform.position;
            directionToEnemy = (targetPosition - transform.position).normalized;
            attackPosition = targetPosition - (directionToEnemy * attackDistance);

            // Rotate to face enemy
            Quaternion targetRotation = Quaternion.LookRotation(directionToEnemy);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

            // Move toward enemy
            transform.position = Vector3.MoveTowards(transform.position, attackPosition, attackMoveSpeed * Time.deltaTime);

            yield return null;
        }

        // Small pause at attack position
        yield return new WaitForSeconds(0.1f);

        // Phase 2: Return to original position and rotation
        while (Vector3.Distance(transform.position, originalPosition) > 0.1f) {
            // Move back to original position
            transform.position = Vector3.MoveTowards(transform.position, originalPosition, returnMoveSpeed * Time.deltaTime);

            yield return null;
        }

        // Snap to exact original transform
        transform.position = originalPosition;
    }
}