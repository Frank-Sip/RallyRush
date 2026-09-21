using UnityEngine;

public class MeleeEnemy : Enemy
{
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private float attackDistance = 0.01f;

    private CrowdSystem crowdSystem;

    private void Start()
    {
        crowdSystem = ServiceProvider.GetService<CrowdSystem>();
    }

    private void Update()
    {
        if (!HasTarget)
        {
            return;
        }

        Vector3 targetPosition = Target.transform.position;
        targetPosition.y = transform.position.y;

        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            moveSpeed * Time.deltaTime);

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

        if (crowdSystem.RemoveRunner(attackedRunner))
        {
            Destroy(gameObject);
        }
    }
}
