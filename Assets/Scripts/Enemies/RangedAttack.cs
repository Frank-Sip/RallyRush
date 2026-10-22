using System;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public struct ProjectileMovementData
{
    public float3 Position;
    public quaternion Rotation;
    public float3 TargetPosition;
    public float Speed;
    public float RotationSpeed;
    public float HitDistance;
    public byte HitTarget;
}

[BurstCompile]
public struct ProjectileMovementJob : IJobParallelFor
{
    public float DeltaTime;
    public NativeArray<ProjectileMovementData> Projectiles;

    public void Execute(int index)
    {
        ProjectileMovementData projectile = Projectiles[index];
        float3 direction = projectile.TargetPosition - projectile.Position;

        if (math.lengthsq(direction) > 0f)
        {
            quaternion targetRotation = quaternion.LookRotationSafe(direction, math.up());
            projectile.Rotation = RotateTowards(projectile.Rotation, targetRotation, projectile.RotationSpeed * DeltaTime);
        }

        projectile.Position = MoveTowards(projectile.Position, projectile.TargetPosition, projectile.Speed * DeltaTime);
        projectile.HitTarget = math.lengthsq(projectile.TargetPosition - projectile.Position) <= projectile.HitDistance * projectile.HitDistance ? (byte)1 : (byte)0;
        Projectiles[index] = projectile;
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

public class RangedAttack : MonoBehaviour
{
    [Header("Detection")]
    [Min(0)] [SerializeField] private float shootingDetectionRange = 10f;

    [Header("Shooting")]
    [SerializeField] private GameObject projectilePrefab;
    [Min(0f)] [SerializeField] private float shootActivationDelay = 3f;
    [Min(0.01f)] [SerializeField] private float fireInterval = 1f;
    [SerializeField] private Transform[] shootPoints = new Transform[0];
    private Vector3 shootPointOffset;

    [Header("Projectile")]
    [Min(0)] [SerializeField] private float projectileSpeed = 10f;
    [Min(0)] [SerializeField] private float projectileRotationSpeed = 720f;
    [Min(0)] [SerializeField] private int projectilePoolInitialCount = 10;
    [Min(1)] [SerializeField] private int projectilePoolExpandCount = 5;
    private float projectileHitDistance = 0.01f;

    private readonly List<ProjectilePool.Projectile> activeProjectiles = new List<ProjectilePool.Projectile>();
    private readonly HashSet<Runner> aliveRunners = new HashSet<Runner>();

    private CrowdSystem crowdSystem;
    private ProjectilePool projectilePool;
    private float shootActivationTimer;
    private float fireTimer;
    private bool canShoot = false;
    private bool isInsideShootingRange;
    private bool shootActivationTimerRunning;
    private NativeArray<ProjectileMovementData> projectileMovementData;

    public event Action OnStartShooting;

    public void CustomAwake()
    {
        crowdSystem = ServiceProvider.GetService<CrowdSystem>();
        projectilePool = new ProjectilePool(projectilePrefab, projectilePoolInitialCount, projectilePoolExpandCount);
    }

    public void CustomUpdate()
    {
        UpdateShootActivationTimer();
        UpdateShooting();
        UpdateProjectiles();
    }

    private void UpdateShootActivationTimer()
    {
        if (!shootActivationTimerRunning)
        {
            return;
        }

        shootActivationTimer -= Time.deltaTime;

        if (shootActivationTimer > 0f)
        {
            return;
        }

        shootActivationTimerRunning = false;
        canShoot = true;
    }

    private void UpdateShooting()
    {
        if (!canShoot)
        {
            return;
        }

        if (!IsRunnerInsideDetectionRange())
        {
            fireTimer = 0f;
            isInsideShootingRange = false;
            return;
        }

        if (!isInsideShootingRange)
        {
            isInsideShootingRange = true;
            OnStartShooting?.Invoke();
        }

        fireTimer -= Time.deltaTime;

        if (fireTimer > 0f)
        {
            return;
        }

        Shoot();
        fireTimer = fireInterval;
    }

    private bool IsRunnerInsideDetectionRange()
    {
        IReadOnlyList<Runner> runners = crowdSystem.GetRunners();
        float detectionRangeSqr = shootingDetectionRange * shootingDetectionRange;

        for (int i = 0; i < runners.Count; i++)
        {
            Vector3 distance = runners[i].transform.position - transform.position;
            distance.y = 0f;

            if (distance.sqrMagnitude <= detectionRangeSqr)
            {
                return true;
            }
        }

        return false;
    }

    private void Shoot()
    {
        for (int i = 0; i < shootPoints.Length; i++)
        {
            Transform shootPoint = shootPoints[i];
            Vector3 shootPosition = shootPoint.position + shootPointOffset;
            Runner target = FindNearestRunner(shootPosition);
            activeProjectiles.Add(projectilePool.Get(shootPosition, target));
        }
    }

    private void UpdateProjectiles()
    {
        if (activeProjectiles.Count == 0)
        {
            return;
        }

        IReadOnlyList<Runner> runners = crowdSystem.GetRunners();
        aliveRunners.Clear();

        for (int i = 0; i < runners.Count; i++)
        {
            aliveRunners.Add(runners[i]);
        }

        for (int i = activeProjectiles.Count - 1; i >= 0; i--)
        {
            ProjectilePool.Projectile projectile = activeProjectiles[i];

            if (!aliveRunners.Contains(projectile.Target))
            {
                if (runners.Count == 0)
                {
                    RemoveProjectile(i);
                    continue;
                }

                projectile.SetTarget(FindNearestRunner(projectile.Transform.position));
            }
        }

        if (activeProjectiles.Count == 0)
        {
            return;
        }

        EnsureProjectileMovementCapacity(activeProjectiles.Count);

        for (int i = 0; i < activeProjectiles.Count; i++)
        {
            ProjectilePool.Projectile projectile = activeProjectiles[i];
            projectileMovementData[i] = new ProjectileMovementData
            {
                Position = projectile.Transform.position,
                Rotation = projectile.Transform.rotation,
                TargetPosition = projectile.Target.transform.position,
                Speed = projectileSpeed,
                RotationSpeed = projectileRotationSpeed,
                HitDistance = projectileHitDistance,
                HitTarget = 0
            };
        }

        ProjectileMovementJob movementJob = new ProjectileMovementJob
        {
            DeltaTime = Time.deltaTime,
            Projectiles = projectileMovementData
        };

        JobHandle movementHandle = movementJob.Schedule(activeProjectiles.Count, 32);
        movementHandle.Complete();

        for (int i = 0; i < activeProjectiles.Count; i++)
        {
            ProjectileMovementData movementData = projectileMovementData[i];
            activeProjectiles[i].Transform.SetPositionAndRotation(movementData.Position, movementData.Rotation);
        }

        for (int i = activeProjectiles.Count - 1; i >= 0; i--)
        {
            if (projectileMovementData[i].HitTarget == 1)
            {
                activeProjectiles[i].Target.Die();
                RemoveProjectile(i);
            }
        }
    }

    private Runner FindNearestRunner(Vector3 origin)
    {
        IReadOnlyList<Runner> runners = crowdSystem.GetRunners();
        Runner nearestRunner = runners[0];
        float nearestDistanceSqr = (nearestRunner.transform.position - origin).sqrMagnitude;

        for (int i = 1; i < runners.Count; i++)
        {
            Runner runner = runners[i];
            float distanceSqr = (runner.transform.position - origin).sqrMagnitude;

            if (distanceSqr < nearestDistanceSqr)
            {
                nearestDistanceSqr = distanceSqr;
                nearestRunner = runner;
            }
        }

        return nearestRunner;
    }

    private void EnsureProjectileMovementCapacity(int requiredCapacity)
    {
        if (projectileMovementData.IsCreated && projectileMovementData.Length >= requiredCapacity)
        {
            return;
        }

        if (projectileMovementData.IsCreated)
        {
            projectileMovementData.Dispose();
        }

        projectileMovementData = new NativeArray<ProjectileMovementData>(Mathf.NextPowerOfTwo(requiredCapacity), Allocator.Persistent);
    }

    private void RemoveProjectile(int index)
    {
        projectilePool.Return(activeProjectiles[index]);
        activeProjectiles.RemoveAt(index);
    }

    public void SetShootPoints(Transform[] newShootPoints, Vector3 offset)
    {
        shootPoints = newShootPoints;
        shootPointOffset = offset;
    }

    public void SetCanShoot(bool value)
    {
        canShoot = value;
        shootActivationTimerRunning = false;
    }

    public void StartShootActivationTimer()
    {
        canShoot = false;
        shootActivationTimer = shootActivationDelay;
        shootActivationTimerRunning = true;
    }

    private void OnDestroy()
    {
        if (projectileMovementData.IsCreated)
        {
            projectileMovementData.Dispose();
        }

        projectilePool.Dispose();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, shootingDetectionRange);
    }

}
