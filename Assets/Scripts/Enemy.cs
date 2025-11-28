using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    [HideInInspector] public EnemyPath pathToFollow;
    private Transform[] waypoints;
    private int waypointIndex = 0;
    private NavMeshAgent agent;

    [Header("Stats")]
    [SerializeField] private int maxHealth = 10;
    [SerializeField] private float speed = 3.5f;
    public string enemyName = "Enemy";

    [HideInInspector] public int currentHealth;

    public int MaxHealth => maxHealth;
    public int CurrentHealth => currentHealth;
    public float Speed => speed;

    private void Start()
    {
        if (pathToFollow == null)
        {
            Debug.LogError("Enemy spawned without a path!");
            Destroy(gameObject);
            return;
        }

        agent = GetComponent<NavMeshAgent>();
        currentHealth = maxHealth;
        agent.speed = speed;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(pathToFollow.GetSpawnPoint(), out hit, 1f, NavMesh.AllAreas))
            agent.Warp(hit.position);

        waypoints = pathToFollow.nodes;
        if (waypoints.Length > 0)
            agent.destination = waypoints[waypointIndex].position;

        EnemyManager.aliveEnemies++;
    }

    private void Update()
    {
        if (waypoints == null || waypoints.Length == 0 || agent.pathPending) return;

        if (agent.remainingDistance <= agent.stoppingDistance)
        {
            waypointIndex++;
            if (waypointIndex < waypoints.Length)
                agent.destination = waypoints[waypointIndex].position;
            else
                ReachGoal();
        }
    }

    public void TakeDamage(int amount)
    {
        int previousHealth = currentHealth;
        currentHealth -= amount;
        currentHealth = Mathf.Max(currentHealth, 0);

        // Add score/scales for the **actual damage dealt**
        int damageDone = previousHealth - currentHealth;
        if (StatManager.Instance != null && damageDone > 0)
            StatManager.Instance.AddScore(damageDone);

        if (currentHealth <= 0)
            Die();
    }

    private void Die()
    {
        EnemyManager.aliveEnemies--;
        Destroy(gameObject);
    }

    private void ReachGoal()
    {
        PlayerHealth player = Object.FindFirstObjectByType<PlayerHealth>();
        if (player != null)
            player.TakeDamage(currentHealth);

        EnemyManager.aliveEnemies--;
        Destroy(gameObject);
    }
}
