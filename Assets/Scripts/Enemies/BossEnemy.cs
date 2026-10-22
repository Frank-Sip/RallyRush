using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossEnemy : BigEnemyReposition
{
    [Min(0.01f)] [SerializeField] private float rotationSpeed = 180f;

    [Header("Audio")]
    [Min(0)] [SerializeField] private int attackHitSfxIndex;

    private RunnerAttackerComponent runnerAttackerComponent;
    private HealthUI healthUI;

    public override void CustomAwake()
    {
        base.CustomAwake();
        healthComponent.CustomAwake();
        healthUI = GetComponentInChildren<HealthUI>(true);
        healthUI.CustomAwake();
        ServiceProvider.RegisterService(this);
        runnerAttackerComponent = GetComponent<RunnerAttackerComponent>();
        runnerAttackerComponent.OnRunnerHit += PlayAttackHitSfx;
    }
    
    public override void CustomStart()
    {
        base.CustomStart();
        runnerAttackerComponent.IsAttacking = false;
    }
    
    public override void CustomUpdate()
    {
        base.CustomUpdate();
        healthUI.CustomUpdate();
    }

    protected override void OnCombatStarted()
    {
        SetAnimTrigger("Attack");
    }

    public override void Die()
    {
        PlayDeathEffect();
        ServiceProvider.GetService<CoinManager>().AddBossReward();
        GameManager.Instance.SetVictoryStatus();
    }

    private void StartIsAttacking()
    {
        runnerAttackerComponent.IsAttacking = true;
    }

    private void StopIsAttacking()
    {
        runnerAttackerComponent.IsAttacking = false;
        RotateTowardsMostRunners();
    }

    private void PlayAttackHitSfx()
    {
        ServiceProvider.GetService<AudioManager>().PlaySFX(attackHitSfxIndex);
    }
    
    public void RotateTowardsMostRunners()
    {
        IReadOnlyList<Runner> runners = crowdSystem.GetRunners();

        if (runners.Count == 0)
        {
            return;
        }

        Vector3 runnersCenter = Vector3.zero;

        for (int i = 0; i < runners.Count; i++)
        {
            runnersCenter += runners[i].transform.position;
        }

        runnersCenter /= runners.Count;
        Vector3 direction = runnersCenter - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude > 0f)
        {
            StartCoroutine(RotateTowards(Quaternion.LookRotation(direction)));
        }
    }

    private IEnumerator RotateTowards(Quaternion targetRotation)
    {
        while (Quaternion.Angle(transform.rotation, targetRotation) > 0.1f)
        {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            yield return null;
        }

        transform.rotation = targetRotation;
    }
}
