using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;

[SelectionBase]
public class PyramidFormation : MonoBehaviour
{
    [Header("Pyramid Settings")]
    [SerializeField] private GameObject pyramidObject;
    [Min(0)][SerializeField] private int objectAmount;
    [SerializeField] private float objectOffsetY;
    [SerializeField] private float objectOffsetZ;

    [Header("Runner Formation Settings")]
    [Min(0f)][SerializeField] private float horizontalSpacing = 0.6f;
    [Min(0f)][SerializeField] private float verticalSpacing = 1f;
    [Min(0f)][SerializeField] private float forwardSpacing = 1f;
    [Min(0f)][SerializeField] private float repositionDelayBetweenRunners = 0.03f;

    private CrowdSystem crowdSystem;
    private List<FormationAssignment> activeAssignments;
    private bool waitingForReposition;
    private CinemachineCamera cinemachineCamera;

    private readonly struct FormationAssignment
    {
        public readonly Runner Runner;
        public readonly Vector3 RepositionLocalPosition;
        public readonly Vector3 TargetWorldPosition;
        public readonly float RepositionDelay;

        public FormationAssignment(Runner runner, Vector3 repositionLocalPosition, Vector3 targetWorldPosition, float repositionDelay)
        {
            Runner = runner;
            RepositionLocalPosition = repositionLocalPosition;
            TargetWorldPosition = targetWorldPosition;
            RepositionDelay = repositionDelay;
        }
    }

    public int Levels { get; private set; }
    public int UsedRunners { get; private set; }
    public int RemainingRunners { get; private set; }

    private void Start()
    {
        cinemachineCamera = FindObjectOfType<CinemachineCamera>();
        crowdSystem = ServiceProvider.GetService<CrowdSystem>();
        SpawnPyramidObjects();
    }

    private void Update()
    {
        if (!waitingForReposition)
        {
            return;
        }

        for (int i = 0; i < activeAssignments.Count; i++)
        {
            if (!activeAssignments[i].Runner.PyramidRepositionCompleted)
            {
                return;
            }
        }

        for (int i = 0; i < activeAssignments.Count; i++)
        {
            FormationAssignment assignment = activeAssignments[i];
            assignment.Runner.StartPyramidAdvance(assignment.TargetWorldPosition, transform.forward);
        }

        crowdSystem.FinishPyramidReposition();
        waitingForReposition = false;
    }

    private void SpawnPyramidObjects()
    {
        for (int i = 0; i < objectAmount; i++)
        {
            Vector3 spawnPosition = transform.position + new Vector3(0f, objectOffsetY * i, objectOffsetZ * i);
            Instantiate(pyramidObject, spawnPosition, transform.rotation, transform);
        }
    }

    public void CalculateLevels()
    {
        int runnerAmount = crowdSystem.GetRunnersAmount();
        Levels = 0;
        UsedRunners = 0;

        while (UsedRunners + Levels + 1 <= runnerAmount)
        {
            Levels++;
            UsedRunners += Levels;
        }

        RemainingRunners = runnerAmount - UsedRunners;

        if (runnerAmount == 2)
        {
            Levels = 2;
            UsedRunners = 2;
            RemainingRunners = 0;
        }
    }

    public void StartPyramidFormation()
    {
        CalculateLevels();
        activeAssignments = CalculateRunnerAssignments();
        PlaceRunners(activeAssignments);
        cinemachineCamera.Follow = activeAssignments[0].Runner.transform;
        crowdSystem.EnterPyramid();
        waitingForReposition = true;
    }

    private List<FormationAssignment> CalculateRunnerAssignments()
    {
        List<Runner> availableRunners = new List<Runner>(crowdSystem.GetRunners());
        List<FormationAssignment> assignments = new List<FormationAssignment>(availableRunners.Count);
        bool twoRunnerFormation = availableRunners.Count == 2;
        availableRunners.Sort((firstRunner, secondRunner) => firstRunner.FormationIndex.CompareTo(secondRunner.FormationIndex));
        int assignmentIndex = 0;

        for (int level = Levels - 1; level >= 0; level--)
        {
            int runnersInLevel = Levels - level;

            if (level == 0)
            {
                runnersInLevel += RemainingRunners;
            }

            if (twoRunnerFormation)
            {
                runnersInLevel = 1;
            }

            for (int positionIndex = 0; positionIndex < runnersInLevel; positionIndex++)
            {
                float horizontalOffset = (positionIndex - (runnersInLevel - 1) * 0.5f) * horizontalSpacing;
                Vector3 repositionWorldPosition = crowdSystem.transform.position
                    + transform.right * horizontalOffset
                    + transform.up * level * verticalSpacing;
                Vector3 targetWorldPosition = transform.position + transform.right * horizontalOffset + transform.up * level * verticalSpacing + transform.forward * level * forwardSpacing;

                Runner runner = availableRunners[0];
                Vector3 repositionLocalPosition = runner.transform.parent.InverseTransformPoint(repositionWorldPosition);
                float repositionDelay = assignmentIndex * repositionDelayBetweenRunners;

                assignments.Add(new FormationAssignment(runner, repositionLocalPosition, targetWorldPosition, repositionDelay));
                availableRunners.RemoveAt(0);
                assignmentIndex++;
            }
        }

        return assignments;
    }

    private static void PlaceRunners(List<FormationAssignment> assignments)
    {
        for (int i = 0; i < assignments.Count; i++)
        {
            FormationAssignment assignment = assignments[i];
            assignment.Runner.SetPyramidRepositionPosition(
                assignment.RepositionLocalPosition,
                assignment.RepositionDelay);
        }
    }
}
