using System.Collections.Generic;
using UnityEngine;

public class EnemyGroup : MonoBehaviour
{
    [SerializeField] private float detectionRange = 5f;
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

            Runner nearestRunner = FindNearestAvailableRunner(enemy.transform.position);
            enemy.SetTarget(nearestRunner);
            assignedRunners.Add(nearestRunner);
        }
    }

    private Runner FindNearestAvailableRunner(Vector3 enemyPosition)
    {
        IReadOnlyList<Runner> runners = crowdSystem.GetRunners();
        Runner nearestRunner = null;
        float nearestDistanceSqr = float.MaxValue;

        for (int i = 0; i < runners.Count; i++)
        {
            Runner runner = runners[i];
            if (assignedRunners.Contains(runner))
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
    }
}
