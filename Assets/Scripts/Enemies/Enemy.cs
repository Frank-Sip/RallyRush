using UnityEngine;

public abstract class Enemy : MonoBehaviour
{
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
