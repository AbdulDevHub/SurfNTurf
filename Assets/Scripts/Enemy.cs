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

    private void Awake()
    {
        // Assign name automatically from the GameObject
        enemyName = gameObject.name.Substring(0, gameObject.name.Length - 7);
    }

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        currentHealth = maxHealth;
        agent.speed = speed;

        // Do NOT destroy the enemy if path is not assigned yet.
        // Just wait until WaveManager assigns it.
        if (pathToFollow == null)
        {
            Debug.LogWarning($"{enemyName} spawned without path yet — waiting for assignment.");
            return;
        }

        InitializeMovement();
    }

    private void Update()
    {
        // If the path was not ready at Start(), initialize when it becomes available
        if (pathToFollow != null && agent.destination == Vector3.zero)
        {
            InitializeMovement();
        }

        if (waypoints == null || agent.pathPending) return;

        if (agent.remainingDistance <= agent.stoppingDistance)
        {
            waypointIndex++;
            if (waypointIndex < waypoints.Length)
                agent.destination = waypoints[waypointIndex].position;
            else
                ReachGoal();
        }
    }

    private void InitializeMovement()
    {
        NavMeshHit hit;
        if (NavMesh.SamplePosition(pathToFollow.GetSpawnPoint(), out hit, 1f, NavMesh.AllAreas))
            agent.Warp(hit.position);

        waypoints = pathToFollow.nodes;

        if (waypoints.Length > 0)
            agent.destination = waypoints[waypointIndex].position;
    }

    public void TakeDamage(int amount)
    {
        int previousHealth = currentHealth;
        currentHealth -= amount;
        currentHealth = Mathf.Max(currentHealth, 0);

        // Add score/scales for the **actual damage dealt**
        int damageDone = previousHealth - currentHealth;
        if (StatManager.Instance != null && damageDone > 0)
        {
            StatManager.Instance.AddScore(damageDone);  
        }

        if (currentHealth <= 0) {
   
            StatManager.Instance.AddScales(150);

            Die();
        }
    }

    private void Die()
    {
        // Play the death sound
        SoundManager.Instance.PlaySound("Fish Killed");

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
