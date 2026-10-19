using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossEnemy : BigEnemyReposition
{
    [Min(0.01f)] [SerializeField] private float rotationSpeed = 180f;

    private RunnerAttackerComponent runnerAttackerComponent;

    protected override void Awake()
    {
        base.Awake();
        ServiceProvider.RegisterService(this);
        runnerAttackerComponent = GetComponent<RunnerAttackerComponent>();
    }
    
    protected override void Start()
    {
        base.Start();
        runnerAttackerComponent.IsAttacking = false;
    }
    
    protected override void Update()
    {
        base.Update();
    }

    protected override void OnCombatStarted()
    {
        SetAnimTrigger("Attack");
    }

    public override void Die()
    {
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
