using UnityEngine;

public class MeleeEnemy : Enemy
{
    [Min(0)] [SerializeField] private float moveSpeed = 6f;
    [Min(0)] [SerializeField] private float rotationSpeed = 720f;

    public float MoveSpeed => moveSpeed;
    public float RotationSpeed => rotationSpeed;

    public override void CustomUpdate()
    {
        if (!HasTarget)
        {
            return;
        }

        Vector3 targetPosition = Target.transform.position;
        targetPosition.y = transform.position.y;

        RotateTowards(targetPosition);
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

    }

    private void RotateTowards(Vector3 targetPosition)
    {
        Vector3 direction = targetPosition - transform.position;

        if (direction.sqrMagnitude > 0f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    public override void TakeDamage()
    {
        Die();
    }

    public override void Die()
    {
        if (IsDead)
        {
            return;
        }

        IsDead = true;
        SetTarget(null);
        enemyGroup.RemoveEnemy(this);
        enemyGroup.Die();
        PlayDeathEffect();
        Destroy(gameObject);
    }
}
