using UnityEngine;

public class Runner : MonoBehaviour
{
    [Header("Reposition Settings")]
    [SerializeField] private float doorRepositionSpeed = 20f;
    [SerializeField] private float combatEnterRepositionSpeed = 5f;
    [SerializeField] private float combatExitRepositionSpeed = 5f;

    [Header("Movement Settings")]
    [SerializeField] private float attackSpeed = 5f;
    [SerializeField] private float rotationSpeed = 720f;
    [Min(0f)] [SerializeField] private float gravity = 20f;
    [Min(0)] [SerializeField] private float enemyCollisionDistance = 0.01f;

    private Vector3 targetLocalPosition;
    private Vector3 pyramidTargetWorldPosition;
    private Vector3 pyramidForward;
    private CrowdSystem crowdSystem;
    private float currentRepositionSpeed;
    private float repositionDelay;
    private float verticalVelocity;
    private bool attackPositionReached;
    private bool pyramidAdvanceEnabled;
    private bool pyramidTargetReached;
    private bool gravityEnabled;
    private bool isDead;

    public int FormationIndex { get; private set; }
    public bool PyramidTargetReached => pyramidTargetReached;

    private void Awake()
    {
        targetLocalPosition = transform.localPosition;
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
                UpdateRunningMovement();
                break;
            case RunnerBehaviour.Combat:
                AttackTarget();
                break;
            case RunnerBehaviour.Pyramid:
                break;
        }

        ApplyGravity();
        CheckEnemyCollision();
    }

    private void UpdateRunningMovement()
    {
        if (repositionDelay > 0f)
        {
            repositionDelay -= Time.deltaTime;
            return;
        }

        MoveTowardsTarget(currentRepositionSpeed);
    }

    private void MoveTowardsTarget(float speed)
    {
        Vector3 horizontalTargetPosition = targetLocalPosition;
        horizontalTargetPosition.y = transform.localPosition.y;
        transform.localPosition = Vector3.MoveTowards(transform.localPosition, horizontalTargetPosition, speed * Time.deltaTime);
    }

    private void AttackTarget()
    {
        if (!attackPositionReached)
        {
            MoveTowardsTarget(currentRepositionSpeed);
            Vector3 distanceToTarget = targetLocalPosition - transform.localPosition;
            distanceToTarget.y = 0f;
            attackPositionReached = distanceToTarget.sqrMagnitude == 0f;
            return;
        }

        Vector3 combatTargetPosition = crowdSystem.CombatTarget.position;
        combatTargetPosition.y = transform.position.y;
        transform.position = Vector3.MoveTowards(transform.position, combatTargetPosition, attackSpeed * Time.deltaTime);
    }

    private void ApplyGravity()
    {
        if (!gravityEnabled)
        {
            return;
        }

        verticalVelocity -= gravity * Time.deltaTime;
        transform.position += Vector3.up * verticalVelocity * Time.deltaTime;
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
        if (crowdSystem.Behaviour == RunnerBehaviour.Pyramid && pyramidAdvanceEnabled)
        {
            StopAtPyramidTarget();
        }

        Vector3 movementDirection;

        switch (crowdSystem.Behaviour)
        {
            case RunnerBehaviour.Running:
                movementDirection = crowdSystem.transform.forward;
                break;
            case RunnerBehaviour.Combat:
                movementDirection = crowdSystem.CombatTarget.position - transform.position;
                break;
            case RunnerBehaviour.Pyramid:
                movementDirection = crowdSystem.transform.forward;
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

    }

    public void SetDoorTargetPosition(Vector3 position, int formationIndex)
    {
        FormationIndex = formationIndex;
        SetTargetPosition(position, doorRepositionSpeed, 0f);
    }

    public void SetCombatEnterTargetPosition(Vector3 position)
    {
        SetTargetPosition(position, combatEnterRepositionSpeed, 0f);
    }

    public void SetCombatExitTargetPosition(Vector3 position, int formationIndex, float delay = 0f)
    {
        FormationIndex = formationIndex;
        SetTargetPosition(position, combatExitRepositionSpeed, delay);
    }

    public void SetPyramidFormationPosition(Vector3 position)
    {
        transform.localPosition = position;
        pyramidAdvanceEnabled = false;
        pyramidTargetReached = false;
    }

    public void StartPyramidAdvance(Vector3 targetWorldPosition, Vector3 forward)
    {
        pyramidTargetWorldPosition = targetWorldPosition;
        pyramidForward = forward;
        pyramidAdvanceEnabled = true;
        pyramidTargetReached = false;
    }

    private void StopAtPyramidTarget()
    {
        float distanceToTarget = Vector3.Dot(
            pyramidTargetWorldPosition - transform.position,
            pyramidForward);

        if (!pyramidTargetReached && distanceToTarget > 0f)
        {
            return;
        }

        pyramidTargetReached = true;
        transform.position += pyramidForward * distanceToTarget;
    }

    private void SetTargetPosition(Vector3 position, float repositionSpeed, float delay)
    {
        targetLocalPosition = position;
        currentRepositionSpeed = repositionSpeed;
        repositionDelay = delay;
        attackPositionReached = false;
    }

    public void SetFormationIndex(int formationIndex)
    {
        FormationIndex = formationIndex;
    }

    public void ToggleGravity()
    {
        gravityEnabled = !gravityEnabled;

        if (!gravityEnabled)
        {
            verticalVelocity = 0f;
        }
    }

    public void Jump(float jumpForce)
    {
        verticalVelocity = jumpForce;
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
