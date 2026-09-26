using UnityEngine;

public abstract class Enemy : MonoBehaviour
{
    [Min(0)] [SerializeField] protected float attackDistance = 0.5f;

    public Runner Target { get; private set; }
    public bool HasTarget => Target != null;
    public bool IsDead { get; private set; }

    private EnemyGroup enemyGroup;
    private Animator animator;

    protected virtual void Awake()
    {
        animator = GetComponentInChildren<Animator>();
    }

    public virtual void SetTarget(Runner target)
    {
        Target = target;
    }

    public void SetEnemyGroup(EnemyGroup group)
    {
        enemyGroup = group;
    }

    public void SetAnimTrigger(string triggerName)
    {
        animator.SetTrigger(triggerName);
    }

    public virtual bool IsTargetInAttackRange()
    {
        Vector3 distance = Target.transform.position - transform.position;
        distance.y = 0f;
        return distance.sqrMagnitude <= attackDistance * attackDistance;
    }

    public virtual void Die()
    {
        if (IsDead)
        {
            return;
        }

        IsDead = true;
        Target = null;
        enemyGroup.RemoveEnemy(this);
        enemyGroup.Die();
    }
}
