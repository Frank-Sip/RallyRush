using UnityEngine;

public class MeleeEnemy : Enemy
{
    [Min(0)] [SerializeField] private float moveSpeed = 6f;
    [Min(0)] [SerializeField] private float attackDistance = 0.01f;

    private void Update()
    {
        if (!HasTarget)
        {
            return;
        }

        Vector3 targetPosition = Target.transform.position;
        targetPosition.y = transform.position.y;

        transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

        float attackDistanceSqr = attackDistance * attackDistance;
        if ((transform.position - targetPosition).sqrMagnitude <= attackDistanceSqr)
        {
            AttackTarget();
        }
    }

    private void AttackTarget()
    {
        Runner attackedRunner = Target;
        SetTarget(null);
        attackedRunner.Die();
        Die();
    }

    public override void Die()
    {
        if (IsDead)
        {
            return;
        }

        base.Die();
        Destroy(gameObject);
    }
}
