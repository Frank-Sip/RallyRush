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
    private CrowdSystem crowdSystem;
    private Transform runningTarget;
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
                if (runningTarget == null)
                {
                    MoveForward();
                    LateralMovement();
                }
                else
                {
                    MoveTowardsTarget(runningTarget);
                }
                break;
            case RunnerBehaviour.Combat:
                MoveTowardsTarget(combatTarget);
                break;
            case RunnerBehaviour.Pyramid:
                MoveForward();
                break;
            case RunnerBehaviour.Victory:
                break;
        }
    }

    private void MoveForward()
    {
        transform.Translate(Vector3.forward * currentSpeed * Time.deltaTime);
    }

    private void MoveTowardsTarget(Transform target)
    {
        Vector3 direction = target.position - transform.position;
        direction.y = 0f;

        float movementDistance = Mathf.Min(currentSpeed * Time.deltaTime, direction.magnitude);
        transform.position += direction.normalized * movementDistance;
    }

    public void SetRunningTarget(Transform target)
    {
        runningTarget = target;
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
    }

    private void LateralMovement()
    {
        if (Input.GetMouseButtonDown(0))
        {
            SetInputStartPosition();
        }
        else if (Input.GetMouseButton(0))
        {
            Vector3 currentScreenPosition = Input.mousePosition;
            float xScreenDifference = currentScreenPosition.x - clickScreenPosition.x;
            clickScreenPosition = currentScreenPosition;

            xScreenDifference /= Screen.width;

            Vector3 newPosition = transform.position;
            float lateralMovement = xScreenDifference * slideSpeed;
            Vector2 crowdBounds = crowdSystem.GetCrowdHorizontalBounds();
            float leftLimit = -screenLimit / 2f;
            float rightLimit = screenLimit / 2f;

            if (lateralMovement > 0f)
            {
                float availableMovement = Mathf.Max(0f, rightLimit - crowdBounds.y);
                lateralMovement = Mathf.Min(lateralMovement, availableMovement);
            }
            else if (lateralMovement < 0f)
            {
                float availableMovement = Mathf.Min(0f, leftLimit - crowdBounds.x);
                lateralMovement = Mathf.Max(lateralMovement, availableMovement);
            }

            newPosition.x += lateralMovement;

            transform.position = newPosition;
        }
    }
    

}
