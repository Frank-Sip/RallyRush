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
    private float currentSpeed;
    private float targetSpeed;
    
    private void Awake()
    {
        crowdSystem = GetComponentInChildren<CrowdSystem>();
        currentSpeed = regularSpeed;
        targetSpeed = regularSpeed;
    }
    
    private void Update()
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
        MoveForward();
        LateralMovement();
    }

    private void MoveForward()
    {
        transform.Translate(Vector3.forward * currentSpeed * Time.deltaTime);
    }

    public void SetSpeed(float speed)
    {
        targetSpeed = Mathf.Max(0f, speed);
    }

    public void ReduceSpeedForCombat()
    {
        SetSpeed(combatSpeed);
    }

    public void RestoreRegularSpeed()
    {
        SetSpeed(regularSpeed);
    }

    private void LateralMovement()
    {
        if (Input.GetMouseButtonDown(0))
        {
            clickScreenPosition = Input.mousePosition;
            clickPlayerPosition = transform.position;
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
