using UnityEngine;

public class Runner : MonoBehaviour
{
    [SerializeField] private float repositionSpeed = 5f;

    private Vector3 targetLocalPosition;
    private CrowdSystem crowdSystem;
    private bool isDead;

    private void Awake()
    {
        targetLocalPosition = transform.localPosition;
    }

    private void Start()
    {
        crowdSystem = ServiceProvider.GetService<CrowdSystem>();
    }

    private void Update()
    {
        transform.localPosition = Vector3.MoveTowards(transform.localPosition, targetLocalPosition, repositionSpeed * Time.deltaTime);
    }

    public void SetTargetPosition(Vector3 position)
    {
        targetLocalPosition = position;
    }

    public void Die()
    {
        if (isDead)
        {
            return;
        }

        isDead = true;
        crowdSystem = ServiceProvider.GetService<CrowdSystem>();

        crowdSystem.RemoveRunner(this);
        Destroy(gameObject);
    }
}
