using System.Collections.Generic;
using UnityEngine;

public class EnemyGroup : MonoBehaviour
{
    [SerializeField] private float detectionRange = 5f;
    [Min(0)] [SerializeField] private float spawnRadius = 2f;
    [SerializeField] private float placementAngle = 137.5f;
    [Min(1)] [SerializeField] private int minEnemies = 1;
    [Min(1)] [SerializeField] private int maxEnemies = 5;
    [SerializeField] private string attackAnimationTrigger = "Attack";

    private readonly List<Enemy> enemies = new List<Enemy>();
    private readonly HashSet<Runner> assignedRunners = new HashSet<Runner>();

    private CrowdSystem crowdSystem;
    private MovementController movementController;
    private bool isAttacking;

    private void Awake()
    {
        RefreshEnemies();
    }

    private void Start()
    {
        crowdSystem = ServiceProvider.GetService<CrowdSystem>();
        movementController = crowdSystem.GetComponentInParent<MovementController>();
        SpawnEnemies();
    }

    private void SpawnEnemies()
    {
        int minimum = Mathf.Max(1, minEnemies);
        int maximum = Mathf.Max(minimum, maxEnemies);
        int enemiesToSpawn = Random.Range(minimum, maximum + 1);

        Enemy enemyTemplate = enemies[0];

        while (enemies.Count < enemiesToSpawn)
        {
            Enemy newEnemy = Instantiate(enemyTemplate, transform);
            newEnemy.name = enemyTemplate.name;
            newEnemy.SetEnemyGroup(this);
            enemies.Add(newEnemy);
        }

        PlaceEnemies();
    }

    private void PlaceEnemies()
    {
        int outerIndex = Mathf.Max(1, enemies.Count - 1);

        for (int i = 0; i < enemies.Count; i++)
        {
            float normalizedRadius = Mathf.Sqrt((float)i / outerIndex);
            float distanceFromCenter = spawnRadius * normalizedRadius;
            float angleInRadians = Mathf.Deg2Rad * i * placementAngle;

            float x = distanceFromCenter * Mathf.Cos(angleInRadians);
            float z = distanceFromCenter * Mathf.Sin(angleInRadians);

            enemies[i].transform.localPosition = new Vector3(x, 0f, z);
        }
    }

    private void Update()
    {
        if (!isAttacking)
        {
            if (!IsCrowdInRange())
            {
                return;
            }

            isAttacking = true;
            movementController.ReduceSpeedForCombat();
            SetEnemiesAttackAnimation();
        }

        AssignTargets();
    }

    private void SetEnemiesAttackAnimation()
    {
        for (int i = 0; i < enemies.Count; i++)
        {
            enemies[i].SetAnimTrigger(attackAnimationTrigger);
        }
    }

    private bool IsCrowdInRange()
    {
        Vector3 distance = crowdSystem.transform.position - transform.position;
        distance.y = 0f;

        float totalDetectionRange = detectionRange + crowdSystem.GetCrowdRadius();
        return distance.sqrMagnitude <= totalDetectionRange * totalDetectionRange;
    }

    private void AssignTargets()
    {
        assignedRunners.Clear();

        for (int i = 0; i < enemies.Count; i++)
        {
            if (enemies[i].HasTarget)
            {
                assignedRunners.Add(enemies[i].Target);
            }
        }

        for (int i = 0; i < enemies.Count; i++)
        {
            Enemy enemy = enemies[i];
            if (enemy.HasTarget)
            {
                continue;
            }

            bool onlyAvailableRunners = assignedRunners.Count < crowdSystem.GetRunners().Count;
            Runner nearestRunner = FindNearestRunner(enemy.transform.position, onlyAvailableRunners);

            enemy.SetTarget(nearestRunner);
            assignedRunners.Add(nearestRunner);
        }
    }

    private Runner FindNearestRunner(Vector3 enemyPosition, bool onlyAvailableRunners)
    {
        IReadOnlyList<Runner> runners = crowdSystem.GetRunners();
        Runner nearestRunner = null;
        float nearestDistanceSqr = float.MaxValue;

        for (int i = 0; i < runners.Count; i++)
        {
            Runner runner = runners[i];
            if (onlyAvailableRunners && assignedRunners.Contains(runner))
            {
                continue;
            }

            Vector3 distance = runner.transform.position - enemyPosition;
            distance.y = 0f;
            float distanceSqr = distance.sqrMagnitude;

            if (distanceSqr < nearestDistanceSqr)
            {
                nearestDistanceSqr = distanceSqr;
                nearestRunner = runner;
            }
        }

        return nearestRunner;
    }

    private void RefreshEnemies()
    {
        enemies.Clear();
        GetComponentsInChildren(false, enemies);

        for (int i = 0; i < enemies.Count; i++)
        {
            enemies[i].SetEnemyGroup(this);
        }
    }

    public void RemoveEnemy(Enemy enemy)
    {
        if (!enemies.Remove(enemy))
        {
            return;
        }

        CheckRemainingEnemies();
    }

    private void CheckRemainingEnemies()
    {
        if (enemies.Count > 0)
        {
            return;
        }

        if (isAttacking)
        {
            movementController.RestoreRegularSpeed();
        }

        enabled = false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, spawnRadius);
    }

    private void OnValidate()
    {
        minEnemies = Mathf.Max(1, minEnemies);
        maxEnemies = Mathf.Max(minEnemies, maxEnemies);
    }
}
