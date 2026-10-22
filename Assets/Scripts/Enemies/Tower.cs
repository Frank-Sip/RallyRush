using UnityEngine;

public class Tower : BigEnemyReposition
{
    [Header("Damage Fall Settings")]
    [SerializeField] private Transform fallingTransform;
    [Min(0f)] [SerializeField] private float fallDistancePerHit = 0.1f;

    [Header("Audio")]
    [Min(0)] [SerializeField] private int deathSfxIndex;

    private RangedAttack rangedAttack;
    private HealthUI healthUI;

    public override void CustomAwake()
    {
        base.CustomAwake();
        healthComponent.CustomAwake();
        healthUI = GetComponentInChildren<HealthUI>(true);
        healthUI.CustomAwake();
        rangedAttack = GetComponent<RangedAttack>();
        rangedAttack.CustomAwake();
        healthComponent.OnDamageTaken += HandleDamageTaken;
    }

    public override void CustomUpdate()
    {
        base.CustomUpdate();
        healthUI.CustomUpdate();
        rangedAttack.CustomUpdate();
    }

    private void HandleDamageTaken()
    {
        fallingTransform.localPosition += Vector3.down * fallDistancePerHit;
    }

    public override void Die()
    {
        ServiceProvider.GetService<AudioManager>().PlaySFX(deathSfxIndex);
        base.Die();
    }
}
