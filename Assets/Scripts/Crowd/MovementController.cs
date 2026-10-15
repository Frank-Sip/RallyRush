using UnityEngine;
using UnityEngine.Serialization;

public class MovementController : MonoBehaviour
{
    [Min(0)] [SerializeField] private float regularSpeed = 5f;
    [Min(0)] [SerializeField] private float combatSpeed = 2.5f;
    [Min(0)] [SerializeField] private float speedChangeRate = 5f;
    [Min(0)] [SerializeField] private float slideSpeed = 5f;
    [SerializeField] private float screenLimit = 2f;

    private Vector3 clickScreenPosition;
    private Vector3 clickPlayerPosition;
    private CrowdSystem crowdSystem;
    private Transform combatTarget;
    private float currentSpeed;
    private float targetSpeed;
    
    private void Awake()
    {
        ServiceProvider.RegisterService(this);
        crowdSystem = GetComponentInChildren<CrowdSystem>();
        currentSpeed = regularSpeed;
        targetSpeed = regularSpeed;
    }
    
    public void CustomUpdate()
    {
        UpdateSpeed();
        Move();
    }

    private void UpdateSpeed()
    {
        currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, speedChangeRate * Time.deltaTime);
    }

    private void Move()
    {
        switch (crowdSystem.Behaviour)
        {
            case RunnerBehaviour.Running:
                MoveForward();
                LateralMovement();
                break;
            case RunnerBehaviour.Combat:
                MoveTowardsEnemyGroup();
                break;
            case RunnerBehaviour.Pyramid:
                MoveForward();
                break;
        }
    }

    private void MoveForward()
    {
        transform.Translate(Vector3.forward * currentSpeed * Time.deltaTime);
    }

    private void MoveTowardsEnemyGroup()
    {
        Vector3 direction = combatTarget.transform.position - transform.position;
        direction.y = 0f;

        float movementDistance = Mathf.Min(currentSpeed * Time.deltaTime, direction.magnitude);
        transform.position += direction.normalized * movementDistance;
    }

    public void SetSpeed(float speed)
    {
        targetSpeed = Mathf.Max(0f, speed);
    }

    public void ReduceSpeedForCombat()
    {
        SetSpeed(combatSpeed);
    }

    public void EnterCombat(Transform target)
    {
        combatTarget = target;
        ReduceSpeedForCombat();
    }

    public void EnterCombat(Transform target, float speed)
    {
        combatTarget = target;
        currentSpeed = Mathf.Max(0f, speed);
        targetSpeed = currentSpeed;
    }

    public void ExitCombat()
    {
        combatTarget = null;
        RestoreRegularSpeed();
    }

    public void RestoreRegularSpeed()
    {
        SetSpeed(regularSpeed);
    }

    public void CenterPlayer()
    {
        transform.Translate(new Vector3(-transform.position.x, 0f, 0f), Space.World);
    }

    public void SetInputStartPosition()
    {
        clickScreenPosition = Input.mousePosition;
        clickPlayerPosition = transform.position;
    }

    private void LateralMovement()
    {
        if (Input.GetMouseButtonDown(0))
        {
            SetInputStartPosition();
        }
        else if (Input.GetMouseButton(0))
        {
            float xScreenDifference = Input.mousePosition.x - clickScreenPosition.x;

            xScreenDifference /= Screen.width;
            xScreenDifference *= slideSpeed;

            Vector3 newPosition = transform.position;

            float targetX = clickPlayerPosition.x + xScreenDifference;
            newPosition.x = Mathf.Clamp(targetX, -screenLimit / 2 + crowdSystem.GetCrowdRadius(), screenLimit / 2 - crowdSystem.GetCrowdRadius());

            transform.position = newPosition;
        }
    }
    

}
