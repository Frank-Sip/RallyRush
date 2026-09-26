using System;
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

    private CrowdSystem crowdSystem;
    private float formationSpacing;
    private bool crowdDetected;

    public event Action<int> EnemiesAmountChanged;

    private void Awake()
    {
        RefreshEnemies();
    }

    private void Start()
    {
        crowdSystem = ServiceProvider.GetService<CrowdSystem>();
        SpawnEnemies();
    }

    private void SpawnEnemies()
    {
        int minimum = Mathf.Max(1, minEnemies);
        int maximum = Mathf.Max(minimum, maxEnemies);
        int enemiesToSpawn = UnityEngine.Random.Range(minimum, maximum + 1);

        Enemy enemyTemplate = enemies[0];

        while (enemies.Count < enemiesToSpawn)
        {
            Enemy newEnemy = Instantiate(enemyTemplate, transform);
            newEnemy.name = enemyTemplate.name;
            newEnemy.SetEnemyGroup(this);
            enemies.Add(newEnemy);
        }

        formationSpacing = spawnRadius / Mathf.Sqrt(Mathf.Max(1, enemies.Count - 1));
        PlaceEnemies();
        EnemiesAmountChanged?.Invoke(enemies.Count);
    }

    private void PlaceEnemies()
    {
        for (int i = 0; i < enemies.Count; i++)
        {
            enemies[i].transform.localPosition = CalculateEnemyPosition(i);
        }
    }

    private Vector3 CalculateEnemyPosition(int index)
    {
        float distanceFromCenter = formationSpacing * Mathf.Sqrt(index);
        float angleInRadians = Mathf.Deg2Rad * index * placementAngle;

        float x = distanceFromCenter * Mathf.Cos(angleInRadians);
        float z = distanceFromCenter * Mathf.Sin(angleInRadians);

        return new Vector3(x, 0f, z);
    }

    private void Update()
    {
        if (!crowdDetected)
        {
            DetectCrowd();
        }

        if (crowdDetected)
        {
            AssignTargets();
        }
    }

    private void DetectCrowd()
    {
        if (!IsRunnerInsideRange(detectionRange))
        {
            return;
        }

        crowdDetected = true;
        crowdSystem.EnterCombat(this);
        SetEnemiesAttackAnimation();
    }

    private void SetEnemiesAttackAnimation()
    {
        for (int i = 0; i < enemies.Count; i++)
        {
            enemies[i].SetAnimTrigger(attackAnimationTrigger);
        }
    }

    private bool IsRunnerInsideRange(float range)
    {
        IReadOnlyList<Runner> runners = crowdSystem.GetRunners();
        float rangeSqr = range * range;

        for (int i = 0; i < runners.Count; i++)
        {
            Vector3 distance = runners[i].transform.position - transform.position;
            distance.y = 0f;

            if (distance.sqrMagnitude <= rangeSqr)
            {
                return true;
            }
        }

        return false;
    }

    private void AssignTargets()
    {
        for (int i = 0; i < enemies.Count; i++)
        {
            Enemy enemy = enemies[i];
            if (!enemy.HasTarget)
            {
                enemy.SetTarget(FindNearestRunner(enemy.transform.position));
            }
        }
    }

    private Runner FindNearestRunner(Vector3 enemyPosition)
    {
        IReadOnlyList<Runner> runners = crowdSystem.GetRunners();
        Runner nearestRunner = null;
        float nearestDistanceSqr = float.MaxValue;

        for (int i = 0; i < runners.Count; i++)
        {
            Runner runner = runners[i];
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

        EnemiesAmountChanged?.Invoke(enemies.Count);
    }

    public int GetEnemiesAmount()
    {
        return enemies.Count;
    }

    public void Die()
    {
        if (enemies.Count > 0)
        {
            return;
        }

        if (crowdDetected)
        {
            crowdSystem.ExitCombat();
        }

        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, spawnRadius);
    }

}
