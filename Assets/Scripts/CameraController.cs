using Unity.Cinemachine;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private CinemachineFollow cinemachineFollow;
    [SerializeField] private CinemachineRotationComposer rotationComposer;

    [Header("Pyramid Start Camera Settings")]
    [SerializeField] private Vector3 pyramidStartFollowOffset = new Vector3(0f, 8f, -8f);
    [Min(0f)][SerializeField] private float pyramidStartOffsetChangeRate = 5f;

    [Header("First Level Camera Settings")]
    [SerializeField] private Vector3 firstLevelFollowOffset = new Vector3(0f, 8f, -8f);
    [Min(0f)][SerializeField] private float firstLevelOffsetChangeRate = 5f;

    [Header("Diagonal Movement")]
    [Min(0f)][SerializeField] private float movementSpeed = 5f;

    private PyramidTrigger pyramidTrigger;
    private PyramidFormation pyramidFormation;
    private Runner lastLevelRunner;
    private Vector3 targetFollowOffset;
    private float currentFollowOffsetChangeRate;
    private bool changingFollowOffset;
    private bool movingToLastRunner;

    private void Awake()
    {
        rotationComposer.enabled = false;
    }

    private void Start()
    {
        pyramidTrigger = ServiceProvider.GetService<PyramidTrigger>();
        pyramidFormation = ServiceProvider.GetService<PyramidFormation>();
        
        pyramidTrigger.StartPyramid += ChangeFollowOffset;
        pyramidFormation.FirstLevelComplete += StartDiagonalMovement;
    }

    private void OnDestroy()
    {
        pyramidTrigger.StartPyramid -= ChangeFollowOffset;
        pyramidFormation.FirstLevelComplete -= StartDiagonalMovement;
    }

    private void Update()
    {
        if (changingFollowOffset)
        {
            cinemachineFollow.FollowOffset = Vector3.MoveTowards(
                cinemachineFollow.FollowOffset,
                targetFollowOffset,
                currentFollowOffsetChangeRate * Time.deltaTime);

            changingFollowOffset = cinemachineFollow.FollowOffset != targetFollowOffset;
        }
    }

    private void LateUpdate()
    {
        if (!movingToLastRunner)
        {
            return;
        }

        transform.position = Vector3.MoveTowards(
            transform.position,
            lastLevelRunner.transform.position,
            movementSpeed * Time.deltaTime);
    }

    private void ChangeFollowOffset()
    {
        rotationComposer.enabled = true;
        targetFollowOffset = pyramidStartFollowOffset;
        currentFollowOffsetChangeRate = pyramidStartOffsetChangeRate;
        changingFollowOffset = true;
    }

    public void StartDiagonalMovement()
    {
        targetFollowOffset = firstLevelFollowOffset;
        currentFollowOffsetChangeRate = firstLevelOffsetChangeRate;
        changingFollowOffset = true;
        lastLevelRunner = pyramidFormation.LastLevelRunner;
        movingToLastRunner = true;
    }
}
