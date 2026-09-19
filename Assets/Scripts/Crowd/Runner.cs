using UnityEngine;

public class Runner : MonoBehaviour
{
    [SerializeField] private float repositionSpeed = 5f;

    private Vector3 targetLocalPosition;

    private void Awake()
    {
        targetLocalPosition = transform.localPosition;
    }

    private void Update()
    {
        transform.localPosition = Vector3.MoveTowards(transform.localPosition, targetLocalPosition, repositionSpeed * Time.deltaTime);
    }

    public void SetTargetPosition(Vector3 position)
    {
        targetLocalPosition = position;
    }
}
