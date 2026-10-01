using System;
using System.Collections.Generic;
using UnityEngine;

public class RangedAttack : MonoBehaviour
{
    [Header("Detection")]
    [Min(0)] [SerializeField] private float shootingDetectionRange = 10f;

    [Header("Shooting")]
    [SerializeField] private GameObject projectilePrefab;
    [Min(0.01f)] [SerializeField] private float fireInterval = 1f;
    [SerializeField] private Transform[] shootPoints = new Transform[0];
    private Vector3 shootPointOffset;

    [Header("Projectile")]
    [Min(0)] [SerializeField] private float projectileSpeed = 10f;
    [Min(0)][SerializeField] private float projectileRotationSpeed = 720f;
    private float projectileHitDistance = 0.01f;

    private readonly List<ActiveProjectile> activeProjectiles = new List<ActiveProjectile>();

    private CrowdSystem crowdSystem;
    private float fireTimer;
    private bool canShoot = true;
    private bool isInsideShootingRange;

    public event Action OnStartShooting;

    private void Start()
    {
        crowdSystem = ServiceProvider.GetService<CrowdSystem>();
    }

    private void Update()
    {
        UpdateShooting();
        UpdateProjectiles();
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
            GameObject projectile = Instantiate(projectilePrefab, shootPosition, shootPoint.rotation);

            activeProjectiles.Add(new ActiveProjectile(projectile.transform, target));
        }
    }

    private void UpdateProjectiles()
    {
        float hitDistanceSqr = projectileHitDistance * projectileHitDistance;

        for (int i = activeProjectiles.Count - 1; i >= 0; i--)
        {
            ActiveProjectile projectile = activeProjectiles[i];

            if (!IsTargetAlive(projectile.Target))
            {
                if (crowdSystem.GetRunnersAmount() == 0)
                {
                    RemoveProjectile(i);
                    continue;
                }

                projectile.SetTarget(FindNearestRunner(projectile.Transform.position));
            }

            Vector3 targetPosition = projectile.Target.transform.position;
            Vector3 direction = targetPosition - projectile.Transform.position;

            if (direction.sqrMagnitude > 0f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                projectile.Transform.rotation = Quaternion.RotateTowards(projectile.Transform.rotation, targetRotation, projectileRotationSpeed * Time.deltaTime);
            }

            projectile.Transform.position = Vector3.MoveTowards(projectile.Transform.position, targetPosition, projectileSpeed * Time.deltaTime);

            if ((targetPosition - projectile.Transform.position).sqrMagnitude > hitDistanceSqr)
            {
                continue;
            }

            projectile.Target.Die();
            RemoveProjectile(i);
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

    private bool IsTargetAlive(Runner target)
    {
        IReadOnlyList<Runner> runners = crowdSystem.GetRunners();

        for (int i = 0; i < runners.Count; i++)
        {
            if (runners[i] == target)
            {
                return true;
            }
        }

        return false;
    }

    private void RemoveProjectile(int index)
    {
        Destroy(activeProjectiles[index].Transform.gameObject);
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
    }

    private void OnDestroy()
    {
        for (int i = 0; i < activeProjectiles.Count; i++)
        {
            Destroy(activeProjectiles[i].Transform.gameObject);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, shootingDetectionRange);
    }

    private class ActiveProjectile
    {
        public Transform Transform { get; }
        public Runner Target { get; private set; }

        public ActiveProjectile(Transform projectileTransform, Runner target)
        {
            Transform = projectileTransform;
            Target = target;
        }

        public void SetTarget(Runner target)
        {
            Target = target;
        }
    }
}