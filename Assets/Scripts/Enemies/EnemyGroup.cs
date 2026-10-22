using System;
using System.Collections.Generic;
using UnityEngine;

[SelectionBase]
public class EnemyGroup : MonoBehaviour
{
    [SerializeField] private float detectionRange = 5f;
    [Min(0)][SerializeField] private float spawnRadius = 2f;
    [SerializeField] private float placementAngle = 137.5f;
    [Min(1)][SerializeField] private int minEnemies = 1;
    [Min(1)][SerializeField] private int maxEnemies = 5;

    private readonly List<Enemy> enemies = new List<Enemy>();

    private string attackAnimationTrigger = "Attack";
    private CrowdSystem crowdSystem;
    private EnemyManager enemyManager;
    private float formationSpacing;
    private bool crowdDetected;

    public event Action<int> EnemiesAmountChanged;

    public virtual void CustomAwake()
    {
        RefreshEnemies();
    }

    public virtual void CustomStart()
    {
        crowdSystem = ServiceProvider.GetService<CrowdSystem>();
        enemyManager = ServiceProvider.GetService<EnemyManager>();
        SpawnEnemies();

        for (int i = 0; i < enemies.Count; i++)
        {
            enemies[i].CustomStart();
        }
    }

    public virtual void CustomUpdate()
    {
        if (!crowdDetected && IsRunnerInsideRange(detectionRange))
        {
            DetectCrowd();
        }

        if (crowdDetected)
        {
            enemyManager.UpdateEnemyGroup(enemies, crowdSystem.GetRunners());
        }

        UpdateEnemies();
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
            newEnemy.CustomAwake();
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

    protected virtual void DetectCrowd()
    {
        crowdDetected = true;
        crowdSystem.EnterCombat(transform);

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

    private void RefreshEnemies()
    {
        enemies.Clear();
        GetComponentsInChildren(false, enemies);

        for (int i = 0; i < enemies.Count; i++)
        {
            enemies[i].SetEnemyGroup(this);
            enemies[i].CustomAwake();
        }
    }

    private void UpdateEnemies()
    {
        int enemyIndex = 0;

        while (enemyIndex < enemies.Count)
        {
            Enemy enemy = enemies[enemyIndex];

            if (enemy is not MeleeEnemy)
            {
                enemy.CustomUpdate();
            }

            if (enemyIndex < enemies.Count && enemies[enemyIndex] == enemy)
            {
                enemyIndex++;
            }
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

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, spawnRadius);
    }
}
