using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    [HideInInspector] public EnemyPath pathToFollow;   // Assigned at spawn
    private Transform[] waypoints;
    private int waypointIndex = 0;
    private NavMeshAgent agent;

    [Header("Stats")]
    public int maxHealth = 10;
    public float speed = 3.5f;
    [HideInInspector] public int currentHealth;

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

        // Set agent speed
        agent.speed = speed;

        // Snap to NavMesh at spawn
        NavMeshHit hit;
        if (NavMesh.SamplePosition(pathToFollow.GetSpawnPoint(), out hit, 1f, NavMesh.AllAreas))
            agent.Warp(hit.position);

        // Assign waypoints
        waypoints = pathToFollow.nodes;

        // Set first destination
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
        currentHealth -= amount;
        currentHealth = Mathf.Max(currentHealth, 0);

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
