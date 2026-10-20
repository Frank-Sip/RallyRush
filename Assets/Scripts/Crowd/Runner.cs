using UnityEngine;
using UnityEngine.Serialization;

public class Runner : MonoBehaviour
{
    [Header("Reposition Settings")]
    [SerializeField] private float doorRepositionSpeed = 20f;
    [SerializeField] private float combatEnterRepositionSpeed = 5f;
    [SerializeField] private float combatExitRepositionSpeed = 5f;
    [SerializeField] private float deathRepositionSpeed = 8f;
    [SerializeField] private float pyramidCenteringSpeed = 10f;

    [Header("Movement Settings")]
    [FormerlySerializedAs("attackSpeed")]
    [SerializeField] private float normalEnemyAttackSpeed = 5f;
    [SerializeField] private float bigEnemyAttackSpeed = 5f;
    [SerializeField] private float rotationSpeed = 720f;
    [Min(0f)] [SerializeField] private float gravity = 20f;
    [Min(0)] [SerializeField] private float enemyCollisionDistance = 0.01f;

    [Header("Death Effect")]
    [SerializeField] private DeathEffectEmitter deathEffectEmitter;

    private Vector3 targetLocalPosition;
    private Vector3 pyramidTargetWorldPosition;
    private Vector3 pyramidForward;
    private Transform pyramidCenter;
    private Animator animator;
    private CrowdSystem crowdSystem;
    private Enemy combatEnemyTarget;
    private float currentAttackSpeed;
    private float currentRepositionSpeed;
    private float repositionDelay;
    private float verticalVelocity;
    private bool attackPositionReached;
    private bool pyramidCenteringEnabled;
    private bool pyramidAdvanceEnabled;
    private bool pyramidTargetReached;
    private bool gravityEnabled;
    private bool isDead;

    public int FormationIndex { get; private set; }
    public bool PyramidTargetReached => pyramidTargetReached;
    public bool HasCombatEnemyTarget => combatEnemyTarget != null;

    public void CustomAwake(CrowdSystem owner)
    {
        crowdSystem = owner;
        animator = GetComponent<Animator>();
        targetLocalPosition = transform.localPosition;
        currentAttackSpeed = normalEnemyAttackSpeed;
        currentRepositionSpeed = doorRepositionSpeed;
    }

    public void CustomStart()
    {
        if (GameManager.Instance.currentStatus == GameStatus.Gameplay)
        {
            SetAnimTrigger("Run");
        }
    }

    public void CustomUpdate()
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
                CenterForPyramid();
                break;
            case RunnerBehaviour.Victory:
                return;
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

        Vector3 combatTargetPosition = GetCombatTargetPosition();
        combatTargetPosition.y = transform.position.y;
        transform.position = Vector3.MoveTowards(transform.position, combatTargetPosition, currentAttackSpeed * Time.deltaTime);
    }

    private Vector3 GetCombatTargetPosition()
    {
        if (combatEnemyTarget != null)
        {
            return combatEnemyTarget.transform.position;
        }

        return crowdSystem.CombatTarget.position;
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

    public void CustomLateUpdate()
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
                movementDirection = GetCombatTargetPosition() - transform.position;
                break;
            case RunnerBehaviour.Pyramid:
                movementDirection = crowdSystem.transform.forward;
                break;
            case RunnerBehaviour.Victory:
                return;
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

    public void UseNormalEnemyAttackSpeed()
    {
        currentAttackSpeed = normalEnemyAttackSpeed;
    }

    public void UseBigEnemyAttackSpeed()
    {
        currentAttackSpeed = bigEnemyAttackSpeed;
    }

    public void SetCombatEnemyTarget(Enemy enemy)
    {
        combatEnemyTarget = enemy;
    }

    public void SetCombatExitTargetPosition(Vector3 position, int formationIndex, float delay = 0f)
    {
        FormationIndex = formationIndex;
        SetTargetPosition(position, combatExitRepositionSpeed, delay);
    }

    public void SetDeathRepositionTargetPosition(Vector3 position, int formationIndex)
    {
        FormationIndex = formationIndex;
        SetTargetPosition(position, deathRepositionSpeed, 0f);
    }

    public void SetPyramidFormationPosition(Vector3 position)
    {
        transform.localPosition = position;
        pyramidCenteringEnabled = false;
        pyramidAdvanceEnabled = false;
        pyramidTargetReached = false;
    }

    public void StartPyramidCentering(Transform center)
    {
        pyramidCenter = center;
        pyramidCenteringEnabled = true;
        pyramidAdvanceEnabled = false;
        pyramidTargetReached = false;
    }

    private void CenterForPyramid()
    {
        if (!pyramidCenteringEnabled)
        {
            return;
        }

        Vector3 centerPosition = pyramidCenter.position;
        centerPosition.y = transform.position.y;
        transform.position = Vector3.MoveTowards(transform.position, centerPosition, pyramidCenteringSpeed * Time.deltaTime);

        pyramidCenteringEnabled = transform.position != centerPosition;
    }

    public void StartPyramidAdvance(Vector3 targetWorldPosition, Vector3 forward)
    {
        pyramidTargetWorldPosition = targetWorldPosition;
        pyramidForward = forward;
        pyramidCenteringEnabled = false;
        pyramidAdvanceEnabled = true;
        pyramidTargetReached = false;
    }

    private void StopAtPyramidTarget()
    {
        float distanceToTarget = Vector3.Dot(pyramidTargetWorldPosition - transform.position, pyramidForward);

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

    public void SetAnimTrigger(string triggerName)
    {
        animator.SetTrigger(triggerName);
    }

    public bool Die()
    {
        if (isDead)
        {
            return false;
        }

        isDead = true;

        if (crowdSystem.RemoveRunner(this))
        {
            crowdSystem.RestartDeathRepositionTimer();
        }

        deathEffectEmitter.Play();
        Destroy(gameObject);
        return true;
    }
}
