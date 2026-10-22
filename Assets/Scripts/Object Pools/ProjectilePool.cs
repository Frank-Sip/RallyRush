using System.Collections.Generic;
using UnityEngine;

public class ProjectilePool
{
    private readonly ObjectPool<Transform> projectilePool;
    private readonly Dictionary<Transform, Projectile> projectiles = new Dictionary<Transform, Projectile>();

    public ProjectilePool(GameObject projectilePrefab, int initialCount, int expandCount)
    {
        projectilePool = new ObjectPool<Transform>(projectilePrefab.transform, null, initialCount, expandCount, CreateProjectile);
    }

    private void CreateProjectile(Transform projectileTransform)
    {
        projectiles.Add(projectileTransform, new Projectile(projectileTransform));
    }

    public Projectile Get(Vector3 position, Runner target)
    {
        Transform projectileTransform = projectilePool.Get();
        Projectile projectile = projectiles[projectileTransform];
        projectile.Spawn(position, target);
        return projectile;
    }

    public void Return(Projectile projectile)
    {
        projectile.ClearTarget();
        projectilePool.Return(projectile.Transform);
    }

    public void Dispose()
    {
        foreach (Transform projectileTransform in projectiles.Keys)
        {
            Object.Destroy(projectileTransform.gameObject);
        }

        projectiles.Clear();
    }

    public class Projectile
    {
        public Transform Transform { get; }
        public Runner Target { get; private set; }

        public Projectile(Transform projectileTransform)
        {
            Transform = projectileTransform;
        }

        public void Spawn(Vector3 position, Runner target)
        {
            Transform.position = position;
            SetTarget(target);
            LookAtTarget();
        }

        public void SetTarget(Runner target)
        {
            Target = target;
        }

        public void LookAtTarget()
        {
            Vector3 direction = Target.transform.position - Transform.position;

            if (direction.sqrMagnitude > 0f)
            {
                Transform.rotation = Quaternion.LookRotation(direction);
            }
        }

        public void ClearTarget()
        {
            Target = null;
        }
    }
}
