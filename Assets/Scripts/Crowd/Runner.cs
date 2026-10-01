using UnityEngine;

public class Runner : MonoBehaviour
{
    [UnityEngine.Serialization.FormerlySerializedAs("repositionSpeed")]
    [SerializeField] private float doorRepositionSpeed = 20f;
    [UnityEngine.Serialization.FormerlySerializedAs("combatRepositionSpeed")]
    [SerializeField] private float combatEnterRepositionSpeed = 5f;
    [SerializeField] private float combatExitRepositionSpeed = 5f;
    [SerializeField] private float attackSpeed = 5f;
    [SerializeField] private float rotationSpeed = 720f;
    [Min(0)] [SerializeField] private float enemyCollisionDistance = 0.01f;

    private Vector3 targetLocalPosition;
    private Vector3 previousWorldPosition;
    private CrowdSystem crowdSystem;
    private float currentRepositionSpeed;
    private bool attackPositionReached;
    private bool isDead;

    public int FormationIndex { get; private set; }

    private void Awake()
    {
        targetLocalPosition = transform.localPosition;
        previousWorldPosition = transform.position;
        currentRepositionSpeed = doorRepositionSpeed;
    }

    private void Start()
    {
        crowdSystem = ServiceProvider.GetService<CrowdSystem>();
    }

    private void Update()
    {
        switch (crowdSystem.Behaviour)
        {
            case RunnerBehaviour.Running:
                MoveTowardsTarget(currentRepositionSpeed);
                break;
            case RunnerBehaviour.Combat:
                AttackTarget();
                break;
            default:
                throw new System.ArgumentOutOfRangeException();
        }

        CheckEnemyCollision();
    }

    private void MoveTowardsTarget(float speed)
    {
        transform.localPosition = Vector3.MoveTowards(transform.localPosition, targetLocalPosition, speed * Time.deltaTime);
    }

    private void AttackTarget()
    {
        if (!attackPositionReached)
        {
            MoveTowardsTarget(currentRepositionSpeed);
            attackPositionReached = transform.localPosition == targetLocalPosition;
            return;
        }

        transform.position = Vector3.MoveTowards(transform.position, crowdSystem.CombatTarget.position, attackSpeed * Time.deltaTime);
    }

    private void CheckEnemyCollision()
    {
        for (int i = 0; i < Enemy.ActiveEnemies.Count; i++)
        {
            Enemy enemy = Enemy.ActiveEnemies[i];
            IEnemy damageTarget = enemy;
            Vector3 distance = enemy.transform.position - transform.position;
            distance.y = 0f;

            float totalCollisionDistance = enemyCollisionDistance + damageTarget.CollisionRadius;

            if (distance.sqrMagnitude <= totalCollisionDistance * totalCollisionDistance)
            {
                damageTarget.TakeDamage();
                Die();
                return;
            }
        }
    }

    private void LateUpdate()
    {
        Vector3 movementDirection;

        switch (crowdSystem.Behaviour)
        {
            case RunnerBehaviour.Running:
                movementDirection = transform.position - previousWorldPosition;
                break;
            case RunnerBehaviour.Combat:
                movementDirection = crowdSystem.CombatTarget.position - transform.position;
                break;
            default:
                throw new System.ArgumentOutOfRangeException();
        }

        movementDirection.y = 0f;

        if (movementDirection.sqrMagnitude > 0f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(movementDirection);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        previousWorldPosition = transform.position;
    }

    public void SetDoorTargetPosition(Vector3 position, int formationIndex)
    {
        SetTargetPosition(position, formationIndex, doorRepositionSpeed);
    }

    public void SetCombatEnterTargetPosition(Vector3 position, int formationIndex)
    {
        SetTargetPosition(position, formationIndex, combatEnterRepositionSpeed);
    }

    public void SetCombatExitTargetPosition(Vector3 position, int formationIndex)
    {
        SetTargetPosition(position, formationIndex, combatExitRepositionSpeed);
    }

    private void SetTargetPosition(Vector3 position, int formationIndex, float repositionSpeed)
    {
        targetLocalPosition = position;
        FormationIndex = formationIndex;
        currentRepositionSpeed = repositionSpeed;
        attackPositionReached = false;
    }

    public void SetFormationIndex(int formationIndex)
    {
        FormationIndex = formationIndex;
    }

    public bool Die()
    {
        if (isDead)
        {
            return false;
        }

        isDead = true;
        crowdSystem.RemoveRunner(this);
        Destroy(gameObject);
        return true;
    }
}
