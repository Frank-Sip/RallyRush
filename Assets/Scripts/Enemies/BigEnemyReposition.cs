using System.Collections.Generic;
using UnityEngine;

public class BigEnemyReposition : Enemy
{
    protected CrowdSystem crowdSystem;
    private bool crowdDetected;

    public override void CustomAwake()
    {
        base.CustomAwake();
        healthComponent = GetComponent<HealthComponent>();
        healthComponent.OnDeath += Die;
    }

    public override void CustomStart()
    {
        crowdSystem = ServiceProvider.GetService<CrowdSystem>();
    }

    public override void CustomUpdate()
    {
        if (!crowdDetected)
        {
            DetectCrowd();
        }
    }

    private void DetectCrowd()
    {
        IReadOnlyList<Runner> runners = crowdSystem.GetRunners();
        float detectionDistanceSqr = attackDistance * attackDistance;

        for (int i = 0; i < runners.Count; i++)
        {
            Vector3 distance = runners[i].transform.position - transform.position;
            distance.y = 0f;

            if (distance.sqrMagnitude <= detectionDistanceSqr)
            {
                crowdDetected = true;
                crowdSystem.EnterCombat(transform, 0f);
                crowdSystem.PlaceRunnersForBigEnemy(transform);
                OnCombatStarted();
                return;
            }
        }
    }

    protected virtual void OnCombatStarted()
    {
    }

    public override void Die()
    {
        PlayDeathEffect();
        crowdSystem.ExitBigEnemyCombat();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackDistance);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, collisionRadius);
    }
}
