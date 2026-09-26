using UnityEngine;

public class Runner : MonoBehaviour
{
    [SerializeField] private float repositionSpeed = 5f;
    [SerializeField] private float rotationSpeed = 720f;

    private Vector3 targetLocalPosition;
    private Vector3 previousWorldPosition;
    private CrowdSystem crowdSystem;
    private bool isDead;

    private void Awake()
    {
        targetLocalPosition = transform.localPosition;
        previousWorldPosition = transform.position;
    }

    private void Start()
    {
        crowdSystem = ServiceProvider.GetService<CrowdSystem>();
    }

    private void Update()
    {
        transform.localPosition = Vector3.MoveTowards(transform.localPosition, targetLocalPosition, repositionSpeed * Time.deltaTime);
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
                movementDirection = crowdSystem.CombatTarget.transform.position - transform.position;
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

    public void SetTargetPosition(Vector3 position)
    {
        targetLocalPosition = position;
    }

    public bool Die()
    {
        if (isDead)
        {
            return false;
        }

        isDead = true;
        crowdSystem = ServiceProvider.GetService<CrowdSystem>();

        crowdSystem.RemoveRunner(this);
        Destroy(gameObject);
        return true;
    }
}
