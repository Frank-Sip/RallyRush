using UnityEngine;

public class MovementController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float slideSpeed = 5f;
    [SerializeField] private float screenLimit = 2f;
    
    private float minX;
    private float maxX;

    private Vector3 clickScreenPosition;
    private Vector3 clickPlayerPosition;

    private void Awake()
    {
        minX = -screenLimit;
        maxX = screenLimit;
    }

    private void Update()
    {
        Move();
    }

    private void Move()
    {
        MoveForward();
        LateralMovement();
    }

    private void MoveForward()
    {
        transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime);
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
            newPosition.x = Mathf.Clamp(targetX, minX, maxX);
            
            transform.position = newPosition;
        }
    }
}