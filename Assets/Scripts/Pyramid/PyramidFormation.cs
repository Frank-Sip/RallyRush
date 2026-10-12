using System.Collections.Generic;
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

    private CrowdSystem crowdSystem;

    private readonly struct FormationAssignment
    {
        public readonly Runner Runner;
        public readonly Vector3 TargetLocalPosition;

        public FormationAssignment(Runner runner, Vector3 targetLocalPosition)
        {
            Runner = runner;
            TargetLocalPosition = targetLocalPosition;
        }
    }

    public int Levels { get; private set; }
    public int UsedRunners { get; private set; }
    public int RemainingRunners { get; private set; }

    private void Start()
    {
        crowdSystem = ServiceProvider.GetService<CrowdSystem>();
        SpawnPyramidObjects();
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
        List<FormationAssignment> assignments = CalculateRunnerAssignments();
        PlaceRunners(assignments);
        crowdSystem.EnterPyramid();
    }

    private List<FormationAssignment> CalculateRunnerAssignments()
    {
        List<Runner> availableRunners = new List<Runner>(crowdSystem.GetRunners());
        List<FormationAssignment> assignments = new List<FormationAssignment>(availableRunners.Count);
        bool twoRunnerFormation = availableRunners.Count == 2;

        for (int level = 0; level < Levels; level++)
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
                Vector3 targetWorldPosition = transform.position + transform.right * horizontalOffset + transform.up * level * verticalSpacing + transform.forward * level * forwardSpacing;

                int closestRunnerIndex = FindClosestRunnerIndex(availableRunners, targetWorldPosition);
                Runner runner = availableRunners[closestRunnerIndex];
                Vector3 targetLocalPosition = runner.transform.parent.InverseTransformPoint(targetWorldPosition);

                assignments.Add(new FormationAssignment(runner, targetLocalPosition));
                availableRunners.RemoveAt(closestRunnerIndex);
            }
        }

        return assignments;
    }

    private static void PlaceRunners(List<FormationAssignment> assignments)
    {
        for (int i = 0; i < assignments.Count; i++)
        {
            FormationAssignment assignment = assignments[i];
            assignment.Runner.SetPyramidTargetPosition(assignment.TargetLocalPosition);
        }
    }

    private static int FindClosestRunnerIndex(List<Runner> availableRunners, Vector3 targetPosition)
    {
        int closestRunnerIndex = 0;
        float closestDistanceSqr = float.MaxValue;

        for (int i = 0; i < availableRunners.Count; i++)
        {
            float distanceSqr = (availableRunners[i].transform.position - targetPosition).sqrMagnitude;

            if (distanceSqr < closestDistanceSqr)
            {
                closestDistanceSqr = distanceSqr;
                closestRunnerIndex = i;
            }
        }

        return closestRunnerIndex;
    }
}
