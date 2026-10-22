using System;
using UnityEngine;

[SelectionBase]
public class RunnerAttackerComponent : DetectCrowdComponent
{
    [SerializeField] private Transform attackPoint;

    private bool isAttacking = true;
    private bool hitRegisteredDuringAttack;

    public event Action OnRunnerHit;

    protected override Transform DetectionOrigin => attackPoint != null ? attackPoint : transform;
    protected override bool UseDetectionRotation => true;
    protected override Color DetectionGizmoColor => Color.magenta;

    public bool IsAttacking
    {
        get => isAttacking;
        set
        {
            isAttacking = value;

            if (isAttacking)
            {
                hitRegisteredDuringAttack = false;
                ResetDetectedRunners();
            }
        }
    }

    public override void CustomUpdate()
    {
        if (isAttacking)
        {
            base.CustomUpdate();
        }
    }

    protected override void DetectionAction(Runner runner)
    {
        if (runner.Die() && !hitRegisteredDuringAttack)
        {
            hitRegisteredDuringAttack = true;
            OnRunnerHit?.Invoke();
        }
    }
}
