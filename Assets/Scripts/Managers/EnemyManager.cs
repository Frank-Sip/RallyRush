using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using Object = UnityEngine.Object;

[BurstCompile]
public struct EnemyTargetJob : IJobParallelFor
{
    [ReadOnly] public NativeArray<float3> EnemyPositions;
    [ReadOnly] public NativeArray<float3> RunnerPositions;
    [WriteOnly] public NativeArray<int> NearestRunnerIndices;
    [WriteOnly] public NativeArray<int> NearestEnemyIndices;
    public int EnemyCount;
    public int RunnerCount;

    public void Execute(int index)
    {
        if (index < EnemyCount)
        {
            int nearestRunnerIndex = 0;
            float nearestDistanceSqr = float.MaxValue;

            for (int runnerIndex = 0; runnerIndex < RunnerCount; runnerIndex++)
            {
                float3 distance = RunnerPositions[runnerIndex] - EnemyPositions[index];
                distance.y = 0f;
                float distanceSqr = math.lengthsq(distance);

                if (distanceSqr < nearestDistanceSqr)
                {
                    nearestDistanceSqr = distanceSqr;
                    nearestRunnerIndex = runnerIndex;
                }
            }

            NearestRunnerIndices[index] = nearestRunnerIndex;
        }

        if (index < RunnerCount)
        {
            int nearestEnemyIndex = 0;
            float nearestDistanceSqr = float.MaxValue;

            for (int enemyIndex = 0; enemyIndex < EnemyCount; enemyIndex++)
            {
                float3 distance = EnemyPositions[enemyIndex] - RunnerPositions[index];
                distance.y = 0f;
                float distanceSqr = math.lengthsq(distance);

                if (distanceSqr < nearestDistanceSqr)
                {
                    nearestDistanceSqr = distanceSqr;
                    nearestEnemyIndex = enemyIndex;
                }
            }

            NearestEnemyIndices[index] = nearestEnemyIndex;
        }
    }
}

public struct EnemyMovementData
{
    public float3 Position;
    public quaternion Rotation;
    public float3 TargetPosition;
    public float MoveSpeed;
    public float RotationSpeed;
}

[BurstCompile]
public struct EnemyMovementJob : IJobParallelFor
{
    public float DeltaTime;
    public NativeArray<EnemyMovementData> Enemies;

    public void Execute(int index)
    {
        EnemyMovementData enemy = Enemies[index];
        enemy.TargetPosition.y = enemy.Position.y;
        float3 direction = enemy.TargetPosition - enemy.Position;

        if (math.lengthsq(direction) > 0f)
        {
            quaternion targetRotation = quaternion.LookRotationSafe(direction, math.up());
            enemy.Rotation = RotateTowards(enemy.Rotation, targetRotation, enemy.RotationSpeed * DeltaTime);
        }

        enemy.Position = MoveTowards(enemy.Position, enemy.TargetPosition, enemy.MoveSpeed * DeltaTime);
        Enemies[index] = enemy;
    }

    private static float3 MoveTowards(float3 current, float3 target, float maxDistanceDelta)
    {
        float3 direction = target - current;
        float distance = math.length(direction);

        if (distance <= maxDistanceDelta || distance == 0f)
        {
            return target;
        }

        return current + direction / distance * maxDistanceDelta;
    }

    private static quaternion RotateTowards(quaternion current, quaternion target, float maxDegreesDelta)
    {
        float dot = math.min(math.abs(math.dot(current.value, target.value)), 1f);
        float angle = math.degrees(2f * math.acos(dot));

        if (angle == 0f)
        {
            return target;
        }

        return math.slerp(current, target, math.min(1f, maxDegreesDelta / angle));
    }
}

public class EnemyManager
{
    private readonly List<EnemyGroup> enemyGroups = new List<EnemyGroup>();
    private readonly List<BigEnemyReposition> bigEnemies = new List<BigEnemyReposition>();
    private readonly List<MeleeEnemy> meleeEnemies = new List<MeleeEnemy>();
    private readonly List<Runner> targetRunners = new List<Runner>();

    private NativeArray<float3> enemyPositions;
    private NativeArray<float3> runnerPositions;
    private NativeArray<int> nearestRunnerIndices;
    private NativeArray<int> nearestEnemyIndices;
    private NativeArray<EnemyMovementData> enemyMovementData;

    public void CustomAwake()
    {
        ServiceProvider.RegisterService(this);
        enemyGroups.AddRange(Object.FindObjectsByType<EnemyGroup>(FindObjectsInactive.Include));
        bigEnemies.AddRange(Object.FindObjectsByType<BigEnemyReposition>(FindObjectsInactive.Include));

        for (int i = 0; i < enemyGroups.Count; i++)
        {
            enemyGroups[i].CustomAwake();
        }

        for (int i = 0; i < bigEnemies.Count; i++)
        {
            bigEnemies[i].CustomAwake();
        }
    }

    public void CustomStart()
    {
        for (int i = 0; i < enemyGroups.Count; i++)
        {
            enemyGroups[i].CustomStart();
        }

        for (int i = 0; i < bigEnemies.Count; i++)
        {
            bigEnemies[i].CustomStart();
        }
    }

    public void CustomUpdate()
    {
        UpdateEnemyGroups();
        UpdateBigEnemies();
    }

    public void UpdateEnemyGroup(IReadOnlyList<Enemy> enemies, IReadOnlyList<Runner> runners)
    {
        meleeEnemies.Clear();
        targetRunners.Clear();

        for (int i = 0; i < enemies.Count; i++)
        {
            if (enemies[i] is MeleeEnemy meleeEnemy && !meleeEnemy.IsDead)
            {
                meleeEnemies.Add(meleeEnemy);
            }
        }

        for (int i = 0; i < runners.Count; i++)
        {
            targetRunners.Add(runners[i]);
        }

        if (meleeEnemies.Count == 0 || targetRunners.Count == 0)
        {
            return;
        }

        EnsureEnemyCapacity(meleeEnemies.Count);
        EnsureRunnerCapacity(targetRunners.Count);

        for (int i = 0; i < meleeEnemies.Count; i++)
        {
            enemyPositions[i] = meleeEnemies[i].transform.position;
        }

        for (int i = 0; i < targetRunners.Count; i++)
        {
            runnerPositions[i] = targetRunners[i].transform.position;
        }

        EnemyTargetJob targetJob = new EnemyTargetJob
        {
            EnemyPositions = enemyPositions,
            RunnerPositions = runnerPositions,
            NearestRunnerIndices = nearestRunnerIndices,
            NearestEnemyIndices = nearestEnemyIndices,
            EnemyCount = meleeEnemies.Count,
            RunnerCount = targetRunners.Count
        };

        int targetJobCount = Mathf.Max(meleeEnemies.Count, targetRunners.Count);
        JobHandle targetHandle = targetJob.Schedule(targetJobCount, 64);
        targetHandle.Complete();

        for (int i = 0; i < meleeEnemies.Count; i++)
        {
            if (!meleeEnemies[i].HasTarget)
            {
                meleeEnemies[i].SetTarget(targetRunners[nearestRunnerIndices[i]]);
            }

            enemyMovementData[i] = new EnemyMovementData
            {
                Position = meleeEnemies[i].transform.position,
                Rotation = meleeEnemies[i].transform.rotation,
                TargetPosition = meleeEnemies[i].Target.transform.position,
                MoveSpeed = meleeEnemies[i].MoveSpeed,
                RotationSpeed = meleeEnemies[i].RotationSpeed
            };
        }

        for (int i = 0; i < targetRunners.Count; i++)
        {
            if (!targetRunners[i].HasCombatEnemyTarget)
            {
                targetRunners[i].SetCombatEnemyTarget(meleeEnemies[nearestEnemyIndices[i]]);
            }
        }

        EnemyMovementJob movementJob = new EnemyMovementJob
        {
            DeltaTime = Time.deltaTime,
            Enemies = enemyMovementData
        };

        JobHandle movementHandle = movementJob.Schedule(meleeEnemies.Count, 32);
        movementHandle.Complete();

        for (int i = 0; i < meleeEnemies.Count; i++)
        {
            EnemyMovementData movementData = enemyMovementData[i];
            meleeEnemies[i].transform.SetPositionAndRotation(movementData.Position, movementData.Rotation);
        }
    }

    public void CustomDestroy()
    {
        DisposeEnemyCapacity();
        DisposeRunnerCapacity();
    }

    private void EnsureEnemyCapacity(int requiredCapacity)
    {
        if (enemyPositions.IsCreated && enemyPositions.Length >= requiredCapacity)
        {
            return;
        }

        DisposeEnemyCapacity();
        int capacity = Mathf.NextPowerOfTwo(requiredCapacity);
        enemyPositions = new NativeArray<float3>(capacity, Allocator.Persistent);
        nearestRunnerIndices = new NativeArray<int>(capacity, Allocator.Persistent);
        enemyMovementData = new NativeArray<EnemyMovementData>(capacity, Allocator.Persistent);
    }

    private void EnsureRunnerCapacity(int requiredCapacity)
    {
        if (runnerPositions.IsCreated && runnerPositions.Length >= requiredCapacity)
        {
            return;
        }

        DisposeRunnerCapacity();
        int capacity = Mathf.NextPowerOfTwo(requiredCapacity);
        runnerPositions = new NativeArray<float3>(capacity, Allocator.Persistent);
        nearestEnemyIndices = new NativeArray<int>(capacity, Allocator.Persistent);
    }

    private void DisposeEnemyCapacity()
    {
        if (enemyPositions.IsCreated)
        {
            enemyPositions.Dispose();
            nearestRunnerIndices.Dispose();
            enemyMovementData.Dispose();
        }
    }

    private void DisposeRunnerCapacity()
    {
        if (runnerPositions.IsCreated)
        {
            runnerPositions.Dispose();
            nearestEnemyIndices.Dispose();
        }
    }

    private void UpdateEnemyGroups()
    {
        for (int i = enemyGroups.Count - 1; i >= 0; i--)
        {
            EnemyGroup enemyGroup = enemyGroups[i];

            if (enemyGroup == null)
            {
                enemyGroups.RemoveAt(i);
                continue;
            }

            if (enemyGroup.isActiveAndEnabled)
            {
                enemyGroup.CustomUpdate();
            }
        }
    }

    private void UpdateBigEnemies()
    {
        for (int i = bigEnemies.Count - 1; i >= 0; i--)
        {
            BigEnemyReposition bigEnemy = bigEnemies[i];

            if (bigEnemy == null)
            {
                bigEnemies.RemoveAt(i);
                continue;
            }

            if (bigEnemy.isActiveAndEnabled)
            {
                bigEnemy.CustomUpdate();
            }
        }
    }
}
