using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossEnemy : BigEnemyReposition
{
    [Header("Boss Attack Settings")]
    [SerializeField] private Transform attackPoint;
    [SerializeField] private Vector3 runnerAttackBoxSize = new Vector3(4f, 2f, 4f);
    [Min(0.01f)] [SerializeField] private float rotationSpeed = 180f;

    private bool isAttacking = false;

    protected override void Update()
    {
        base.Update();

        if (isAttacking)
        {
            DetectRunners();
        }
    }

    protected override void OnCombatStarted()
    {
        SetAnimTrigger("Attack");
    }

    private void StartIsAttacking()
    {
        isAttacking = true;
    }

    private void StopIsAttacking()
    {
        isAttacking = false;
        RotateTowardsMostRunners();
    }
    
    public void DetectRunners()
    {
        IReadOnlyList<Runner> runners = crowdSystem.GetRunners();
        Vector3 halfBoxSize = runnerAttackBoxSize * 0.5f;

        for (int i = runners.Count - 1; i >= 0; i--)
        {
            Vector3 runnerPosition = attackPoint.InverseTransformPoint(runners[i].transform.position);

            if (Mathf.Abs(runnerPosition.x) <= halfBoxSize.x &&
                Mathf.Abs(runnerPosition.y) <= halfBoxSize.y &&
                Mathf.Abs(runnerPosition.z) <= halfBoxSize.z)
            {
                runners[i].Die();
            }
        }
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
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime);

            yield return null;
        }

        transform.rotation = targetRotation;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.magenta;
        Matrix4x4 previousMatrix = Gizmos.matrix;
        Gizmos.matrix = Matrix4x4.TRS(attackPoint.position, attackPoint.rotation, Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, runnerAttackBoxSize);
        Gizmos.matrix = previousMatrix;
    }
}
