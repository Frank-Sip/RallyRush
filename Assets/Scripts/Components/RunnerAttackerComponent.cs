using System;
using UnityEngine;
using System.Collections.Generic;

[SelectionBase]
public class RunnerAttackerComponent : MonoBehaviour
{
    [SerializeField] private Transform attackPoint;
    [Tooltip("Attack box size in world units. It is not affected by Transform scale.")]
    [SerializeField] private Vector3 runnerAttackBoxSize = new Vector3(4f, 2f, 4f);

    private bool isAttacking = true;
    private bool hitRegisteredDuringAttack;
    private Enemy enemy;
    private CrowdSystem crowdSystem;

    public event Action OnRunnerHit;

    public bool IsAttacking
    {
        get => isAttacking;
        set
        {
            isAttacking = value;

            if (isAttacking)
            {
                hitRegisteredDuringAttack = false;
            }
        }
    }

    private void Awake()
    {
        enemy = GetComponent<Enemy>();
    }

    private void Start()
    {
        crowdSystem = ServiceProvider.GetService<CrowdSystem>();
    }

    private void Update()
    {
        if (isAttacking)
        {
            DetectRunners();
        }
    }

    public void DetectRunners()
    {
        IReadOnlyList<Runner> runners = crowdSystem.GetRunners();
        Vector3 halfBoxSize = runnerAttackBoxSize * 0.5f;
        Quaternion worldToAttackRotation = Quaternion.Inverse(attackPoint.rotation);
        bool runnerWasHit = false;

        for (int i = runners.Count - 1; i >= 0; i--)
        {
            Vector3 runnerPosition = worldToAttackRotation * (runners[i].transform.position - attackPoint.position);

            if (Mathf.Abs(runnerPosition.x) <= halfBoxSize.x && Mathf.Abs(runnerPosition.y) <= halfBoxSize.y && Mathf.Abs(runnerPosition.z) <= halfBoxSize.z)
            {
                runnerWasHit |= runners[i].Die();
            }
        }

        if (runnerWasHit && !hitRegisteredDuringAttack)
        {
            hitRegisteredDuringAttack = true;
            OnRunnerHit?.Invoke();
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.magenta;

        Transform boxOrigin = attackPoint != null ? attackPoint : transform;
        Matrix4x4 previousMatrix = Gizmos.matrix;
        Gizmos.matrix = Matrix4x4.TRS(boxOrigin.position, boxOrigin.rotation, Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, runnerAttackBoxSize);
        Gizmos.matrix = previousMatrix;
    }
}
