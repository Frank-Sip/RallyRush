using UnityEngine;

public class Tower : BigEnemyReposition
{
    [Header("Damage Fall Settings")]
    [SerializeField] private Transform fallingTransform;
    [Min(0f)] [SerializeField] private float fallDistancePerHit = 0.1f;

    [Header("Audio")]
    [Min(0)] [SerializeField] private int deathSfxIndex;

    protected override void Awake()
    {
        base.Awake();
        healthComponent.OnDamageTaken += HandleDamageTaken;
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
