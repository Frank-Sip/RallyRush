using System.Collections.Generic;
using UnityEngine;

[SelectionBase]
public abstract class Enemy : MonoBehaviour, IEnemy
{
    private static readonly List<Enemy> activeEnemies = new List<Enemy>();

    [Min(0)][SerializeField] protected float attackDistance = 0.5f;
    [Min(0)][SerializeField] protected float collisionRadius = 0.5f;
    [Min(0)][SerializeField] private float damageTakenPerHit = 1f;

    public Runner Target { get; private set; }
    public bool HasTarget => Target != null;
    public bool IsDead { get; private set; }

    private EnemyGroup enemyGroup;
    private Animator[] animators;
    protected HealthComponent healthComponent;

    public static IReadOnlyList<Enemy> ActiveEnemies => activeEnemies;
    public virtual float CollisionRadius => collisionRadius;

    protected virtual void OnEnable()
    {
        activeEnemies.Add(this);
    }

    protected virtual void OnDisable()
    {
        activeEnemies.Remove(this);
    }

    protected virtual void Awake()
    {
        animators = GetComponentsInChildren<Animator>();
        healthComponent = GetComponent<HealthComponent>();
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
        for (int i = 0; i < animators.Length; i++)
        {
            animators[i].SetTrigger(triggerName);
        }
    }

    public void Victory()
    {
        SetAnimTrigger("Victory");
    }

    public virtual bool IsTargetInAttackRange()
    {
        Vector3 distance = Target.transform.position - transform.position;
        distance.y = 0f;
        return distance.sqrMagnitude <= attackDistance * attackDistance;
    }

    public virtual void TakeDamage()
    {
        healthComponent.TakeDamage(damageTakenPerHit);
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

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, collisionRadius);
    }
}
