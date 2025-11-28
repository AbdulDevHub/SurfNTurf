using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishermanAttack : MonoBehaviour
{
    [Header("Fisherman Settings")]
    public int fishermanLevel = 1;

    [Header("Range Trigger")]
    public Collider rangeTrigger; // Assign Range1 for all levels

    [Header("Fishing Rod (Level 1 Only)")]
    public Transform fishingRod; // Assign the fishing rod GameObject/Model here
    public float rodSwingSpeed = 2f; // How fast the rod swings
    public float rodDownAngle = 45f; // How far down the rod swings
    public float rodUpAngle = -30f; // How far up the rod raises

    [Header("Net Prefab (Level 2 & 3)")]
    public GameObject netPrefab; // Assign your net visual prefab here

    // Level-based stats
    private int damage;
    private float attackInterval;
    private bool usesNet;
    private int netCapacity;
    private float netCooldown = 5f;
    private float netDuration = 5f; // How long the net stays out

    private FishermanRangeTrigger rangeTriggerScript;
    private List<Enemy> enemiesInRange = new List<Enemy>();
    private Enemy currentTarget = null;
    private Coroutine attackCoroutine;

    // Fishing rod animation
    private Quaternion rodOriginalRotation;
    private bool isSwingingRod = false;

    // Net tracking
    private GameObject activeNet;
    private List<Enemy> trappedEnemies = new List<Enemy>();
    private bool netOnCooldown = false;
    private float netTimer = 0f;
    private int currentNetSize = 0;
    private bool netForceFull = false;

    private void Awake()
    {
        // Set stats based on fisherman level
        SetStatsForLevel();

        if (rangeTrigger == null)
        {
            Debug.LogError($"{gameObject.name}: Range trigger not assigned!");
            return;
        }

        // Ensure trigger collider is set
        rangeTrigger.isTrigger = true;

        // Get the FishermanRangeTrigger component from the assigned collider
        rangeTriggerScript = rangeTrigger.GetComponent<FishermanRangeTrigger>();
        if (rangeTriggerScript == null)
        {
            Debug.LogError($"{gameObject.name}: FishermanRangeTrigger script not found on range trigger!");
        }

        // Store original fishing rod rotation for LV1
        if (fishermanLevel == 1 && fishingRod != null)
        {
            rodOriginalRotation = fishingRod.localRotation;
        }
    }

    private void SetStatsForLevel()
    {
        switch (fishermanLevel)
        {
            case 1:
                damage = 1;
                attackInterval = 1f;
                usesNet = false;
                netCapacity = 0;
                break;
            case 2:
                damage = 0;
                attackInterval = 2f;
                usesNet = true;
                netCapacity = 5;
                break;
            case 3:
                damage = 0;
                attackInterval = 2f;
                usesNet = true;
                netCapacity = 10;
                break;
            default:
                damage = 1;
                attackInterval = 1f;
                usesNet = false;
                netCapacity = 0;
                Debug.LogWarning($"{gameObject.name}: Unknown fisherman level {fishermanLevel}, using default stats");
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

        // Reset fishing rod to original position
        if (fishermanLevel == 1 && fishingRod != null)
        {
            fishingRod.localRotation = rodOriginalRotation;
        }

        // Clean up net and trapped enemies
        if (activeNet != null)
        {
            Destroy(activeNet);
        }
        ReleaseAllTrappedEnemies();
        
        enemiesInRange.Clear();
        currentTarget = null;
    }

    private void HandleEnemyEnter(Enemy enemy)
    {
        if (!enemiesInRange.Contains(enemy))
        {
            enemiesInRange.Add(enemy);
        }

        if (currentTarget == null)
        {
            currentTarget = enemy;
        }
    }

    private void HandleEnemyExit(Enemy enemy)
    {
        enemiesInRange.Remove(enemy);

        if (currentTarget == enemy)
        {
            currentTarget = GetNextTarget();
        }
    }

    private Enemy GetNextTarget()
    {
        // Clean up any null references
        enemiesInRange.RemoveAll(e => e == null);

        if (enemiesInRange.Count > 0)
        {
            return enemiesInRange[0];
        }
        return null;
    }

    private IEnumerator AttackRoutine()
    {
        while (true)
        {
            if (usesNet)
            {
                // Level 2 & 3: Net behavior
                if (!netOnCooldown && enemiesInRange.Count > 0 && activeNet == null)
                {
                    DeployNet();
                    netOnCooldown = true;
                    StartCoroutine(NetCooldownTimer());
                }

                yield return null;
            }
            else
            {
                // Level 1: Direct damage with fishing rod animation
                if (currentTarget != null)
                {
                    // Make fisherman look at the target
                    LookAtTarget(currentTarget.transform.position);

                    // Play fishing rod attack animation
                    if (fishingRod != null && !isSwingingRod)
                    {
                        yield return StartCoroutine(SwingFishingRod());
                    }

                    // Deal damage at the end of the swing
                    if (currentTarget != null)
                    {
                        currentTarget.TakeDamage(damage);
                    }

                    // Wait for attack interval
                    yield return new WaitForSeconds(attackInterval);

                    // Check if target is dead and get next target
                    if (currentTarget == null || currentTarget.CurrentHealth <= 0)
                    {
                        currentTarget = GetNextTarget();
                    }
                }
                else
                {
                    // No target, just wait for next frame
                    yield return null;
                }
            }
        }
    }

    private IEnumerator SwingFishingRod()
    {
        isSwingingRod = true;

        // PLAY FISHING ROD SOUND (LEVEL 1)
        SoundManager.Instance.PlaySound("Fishing Rod", transform.position);

        // Phase 1: Swing down
        float elapsedTime = 0f;
        float swingDuration = 0.15f / rodSwingSpeed;
        Quaternion startRotation = fishingRod.localRotation;
        Quaternion downRotation = rodOriginalRotation * Quaternion.Euler(rodDownAngle, 0f, 0f);

        while (elapsedTime < swingDuration)
        {
            fishingRod.localRotation = Quaternion.Lerp(startRotation, downRotation, elapsedTime / swingDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        fishingRod.localRotation = downRotation;

        // Phase 2: Raise up
        elapsedTime = 0f;
        swingDuration = 0.2f / rodSwingSpeed;
        startRotation = fishingRod.localRotation;
        Quaternion upRotation = rodOriginalRotation * Quaternion.Euler(rodUpAngle, 0f, 0f);

        while (elapsedTime < swingDuration)
        {
            fishingRod.localRotation = Quaternion.Lerp(startRotation, upRotation, elapsedTime / swingDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        fishingRod.localRotation = upRotation;

        // Phase 3: Return to original
        elapsedTime = 0f;
        swingDuration = 0.15f / rodSwingSpeed;
        startRotation = fishingRod.localRotation;

        while (elapsedTime < swingDuration)
        {
            fishingRod.localRotation = Quaternion.Lerp(startRotation, rodOriginalRotation, elapsedTime / swingDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        fishingRod.localRotation = rodOriginalRotation;

        isSwingingRod = false;
    }

    private void Update()
    {
        // Update net logic if net is active
        if (activeNet != null)
        {
            netTimer += Time.deltaTime;

            // Try to catch additional fish if net isn't force-full
            if (!netForceFull)
            {
                TryCatchAdditionalFish();
            }

            // Check if net duration expired or net is full
            if (netTimer >= netDuration || IsNetFull())
            {
                StartCoroutine(CollectNetDelayed());
            }
        }
    }

    private void DeployNet()
    {
        // Reset net tracking variables
        currentNetSize = 0;
        netForceFull = false;
        netTimer = 0f;

        // Gather initial enemies to trap
        List<Enemy> enemiesToTrap = SelectEnemiesToTrap();

        // If no fish to trap, do nothing
        if (enemiesToTrap.Count == 0) return;

        // Compute net position (center of selected fish)
        Vector3 netPosition = CalculateCenterPosition(enemiesToTrap);

        // Make fisherman look at net
        LookAtTarget(netPosition);

        // Spawn net object
        if (netPrefab != null)
        {
            activeNet = Instantiate(
                netPrefab,
                netPosition,
                Quaternion.Euler(90f, 0f, 0f)
            );

            // Different net scale for LV2 and LV3
            if (fishermanLevel == 2) {                
                SoundManager.Instance.PlaySound("Fish Net LV1", transform.position);
                activeNet.transform.localScale = new Vector3(0.25f, 0.25f, 0.25f);
            }
            else if (fishermanLevel == 3) {
                SoundManager.Instance.PlaySound("Fish Net LV2", transform.position);
                activeNet.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            }
        }

        // Trap the selected enemies
        foreach (Enemy enemy in enemiesToTrap)
        {
            TrapEnemy(enemy);
        }
    }

    private void TryCatchAdditionalFish()
    {
        // Check for new enemies in range that aren't already trapped
        List<Enemy> newEnemies = new List<Enemy>();
        
        foreach (Enemy enemy in enemiesInRange)
        {
            if (enemy != null && !trappedEnemies.Contains(enemy))
            {
                newEnemies.Add(enemy);
            }
        }

        if (newEnemies.Count == 0) return;

        // Try to trap additional enemies
        foreach (Enemy enemy in newEnemies)
        {
            if (enemy == null) continue;

            int fishSize = enemy.CurrentHealth;

            // Stop if net is force-full
            if (netForceFull) break;

            // Check if we can add this fish
            if (currentNetSize + fishSize > netCapacity)
            {
                // Allow size 3+ fish even if exceeding capacity
                if (fishSize >= 3)
                {
                    TrapEnemy(enemy);
                    netForceFull = true; // Net becomes full after this catch
                }
                continue;
            }

            // Otherwise trap normally
            TrapEnemy(enemy);
        }

        // Update net position to center of all trapped enemies
        if (activeNet != null && trappedEnemies.Count > 0)
        {
            Vector3 newCenter = CalculateCenterPosition(trappedEnemies);
            activeNet.transform.position = newCenter;
        }
    }

    private List<Enemy> SelectEnemiesToTrap()
    {
        List<Enemy> enemiesToTrap = new List<Enemy>();

        foreach (Enemy enemy in enemiesInRange)
        {
            if (enemy == null) continue;

            int fishSize = enemy.CurrentHealth;

            // Stop if net is force-full
            if (netForceFull) break;

            // Check capacity
            if (currentNetSize + fishSize > netCapacity)
            {
                // Allow size 3+ fish even if exceeding capacity
                if (fishSize >= 3)
                {
                    enemiesToTrap.Add(enemy);
                    currentNetSize += fishSize;
                    netForceFull = true;
                }
                continue;
            }

            // Otherwise catch normally
            enemiesToTrap.Add(enemy);
            currentNetSize += fishSize;
        }

        return enemiesToTrap;
    }

    private bool IsNetFull()
    {
        return netForceFull || currentNetSize >= netCapacity;
    }

    private void CollectNet()
    {
        // Kill all trapped enemies
        foreach (Enemy enemy in new List<Enemy>(trappedEnemies))
        {
            if (enemy != null)
            {
                // Deal massive damage to kill the enemy instantly
                enemy.TakeDamage(999);
            }
        }

        // Clear trapped enemies list
        trappedEnemies.Clear();

        // Despawn the net
        DespawnNet();

        // Reset tracking variables
        currentNetSize = 0;
        netForceFull = false;
        netTimer = 0f;
    }

    private Vector3 CalculateCenterPosition(List<Enemy> enemies)
    {
        if (enemies.Count == 0) return transform.position;

        Vector3 sum = Vector3.zero;
        int validCount = 0;
        
        foreach (Enemy enemy in enemies)
        {
            if (enemy != null)
            {
                sum += enemy.transform.position;
                validCount++;
            }
        }

        if (validCount == 0) return transform.position;

        Vector3 center = sum / validCount;
        
        // Place net slightly above ground
        center.y = 0.1f;
        
        return center;
    }

    private void LookAtTarget(Vector3 targetPosition)
    {
        Vector3 lookPos = targetPosition;
        lookPos.y = transform.position.y; // Keep same Y level to prevent tilting
        
        Vector3 direction = lookPos - transform.position;
        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }
    }

    private void TrapEnemy(Enemy enemy)
    {
        if (enemy == null || trappedEnemies.Contains(enemy)) return;

        int fishSize = enemy.CurrentHealth;
        currentNetSize += fishSize;

        // Stop the enemy's NavMeshAgent
        UnityEngine.AI.NavMeshAgent agent = enemy.GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (agent != null)
        {
            agent.isStopped = true;
        }

        trappedEnemies.Add(enemy);

        // Start monitoring this trapped enemy
        StartCoroutine(MonitorTrappedEnemy(enemy));
    }

    private IEnumerator MonitorTrappedEnemy(Enemy enemy)
    {
        while (enemy != null && trappedEnemies.Contains(enemy) && activeNet != null)
        {
            // Check if enemy died naturally (not from collection)
            if (enemy.CurrentHealth <= 0)
            {
                int fishSize = enemy.CurrentHealth;
                currentNetSize -= fishSize;
                trappedEnemies.Remove(enemy);
                
                // Check if net should despawn (no more trapped enemies)
                if (trappedEnemies.Count == 0)
                {
                    DespawnNet();
                }
                yield break;
            }

            yield return null;
        }
    }

    private void ReleaseAllTrappedEnemies()
    {
        foreach (Enemy enemy in new List<Enemy>(trappedEnemies))
        {
            if (enemy != null)
            {
                // Resume the enemy's NavMeshAgent
                UnityEngine.AI.NavMeshAgent agent = enemy.GetComponent<UnityEngine.AI.NavMeshAgent>();
                if (agent != null)
                {
                    agent.isStopped = false;
                }
            }
        }
        trappedEnemies.Clear();
    }

    private void DespawnNet()
    {
        if (activeNet != null)
        {
            Destroy(activeNet);
            activeNet = null;
        }
    }

    private IEnumerator NetCooldownTimer()
    {
        yield return new WaitForSeconds(netCooldown);
        netOnCooldown = false;
    }

    private IEnumerator CollectNetDelayed()
    {
        // Optional: prevent multiple calls
        if (activeNet == null) yield break;

        // Delay for visual effect
        yield return new WaitForSeconds(0.5f); // Adjust as needed (0.3–0.8 sec works great)

        CollectNet();
    }
}