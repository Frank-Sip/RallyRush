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
    private ScreenComposerSettings pyramidComposition;
    private float currentFollowOffsetChangeRate;
    private bool changingFollowOffset;
    private bool movingToLastRunner;

    private void Awake()
    {
        pyramidComposition = rotationComposer.Composition;

        ScreenComposerSettings gameplayComposition = pyramidComposition;
        gameplayComposition.DeadZone.Enabled = true;
        gameplayComposition.DeadZone.Size = new Vector2(2f, 2f);
        gameplayComposition.HardLimits.Enabled = false;

        rotationComposer.CenterOnActivate = false;
        rotationComposer.Composition = gameplayComposition;
        rotationComposer.enabled = true;
    }

    private void Start()
    {
        pyramidTrigger = FindAnyObjectByType<PyramidTrigger>();
        pyramidFormation = FindAnyObjectByType<PyramidFormation>();

        if (pyramidTrigger != null)
        {
            pyramidTrigger.StartPyramid += ChangeFollowOffset;
        }

        if (pyramidFormation != null)
        {
            pyramidFormation.FirstLevelComplete += StartDiagonalMovement;
        }

    }

    private void OnDestroy()
    {
        if (pyramidTrigger != null)
        {
            pyramidTrigger.StartPyramid -= ChangeFollowOffset;
        }

        if (pyramidFormation != null)
        {
            pyramidFormation.FirstLevelComplete -= StartDiagonalMovement;
        }

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
        rotationComposer.Composition = pyramidComposition;
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
