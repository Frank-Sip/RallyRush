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
    [Min(1)][SerializeField] private int remainingRunnerDistributionLevels = 2;
    [Min(0f)][SerializeField] private float formationTickInterval = 0.1f;

    private CrowdSystem crowdSystem;
    private List<FormationAssignment> activeAssignments;
    private float formationTickTimer;
    private int formedLevelCount;
    private bool buildingFormation;

    private readonly struct FormationAssignment
    {
        public readonly Runner Runner;
        public readonly int Level;
        public readonly float HorizontalOffset;
        public readonly Vector3 TargetWorldPosition;

        public FormationAssignment(Runner runner, int level, float horizontalOffset, Vector3 targetWorldPosition)
        {
            Runner = runner;
            Level = level;
            HorizontalOffset = horizontalOffset;
            TargetWorldPosition = targetWorldPosition;
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

    private void Update()
    {
        if (!buildingFormation)
        {
            return;
        }

        formationTickTimer -= Time.deltaTime;

        if (formationTickTimer <= 0f)
        {
            BuildNextLevel();
        }
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
        crowdSystem.EnterPyramid();
        formedLevelCount = 0;
        buildingFormation = true;
        BuildNextLevel();
    }

    private List<FormationAssignment> CalculateRunnerAssignments()
    {
        List<Runner> availableRunners = new List<Runner>(crowdSystem.GetRunners());
        List<FormationAssignment> assignments = new List<FormationAssignment>(availableRunners.Count);
        int[] runnersPerLevel = CalculateRunnersPerLevel();
        availableRunners.Sort((firstRunner, secondRunner) => firstRunner.FormationIndex.CompareTo(secondRunner.FormationIndex));

        for (int level = Levels - 1; level >= 0; level--)
        {
            int runnersInLevel = runnersPerLevel[level];

            for (int positionIndex = 0; positionIndex < runnersInLevel; positionIndex++)
            {
                float horizontalOffset = (positionIndex - (runnersInLevel - 1) * 0.5f) * horizontalSpacing;
                Vector3 targetWorldPosition = transform.position + transform.right * horizontalOffset + transform.up * level * verticalSpacing + transform.forward * level * forwardSpacing;

                Runner runner = availableRunners[0];

                assignments.Add(new FormationAssignment(runner, level, horizontalOffset, targetWorldPosition));
                availableRunners.RemoveAt(0);
            }
        }

        return assignments;
    }

    private int[] CalculateRunnersPerLevel()
    {
        int[] runnersPerLevel = new int[Levels];

        if (crowdSystem.GetRunnersAmount() == 2)
        {
            runnersPerLevel[0] = 1;
            runnersPerLevel[1] = 1;
            return runnersPerLevel;
        }

        for (int level = 0; level < Levels; level++)
        {
            runnersPerLevel[level] = Levels - level;
        }

        int distributionLevels = Mathf.Min(
            remainingRunnerDistributionLevels,
            Mathf.Max(1, Levels - 1));

        for (int i = 0; i < RemainingRunners; i++)
        {
            runnersPerLevel[i % distributionLevels]++;
        }

        return runnersPerLevel;
    }

    private void BuildNextLevel()
    {
        formedLevelCount++;
        int lowestFormedLevel = Levels - formedLevelCount;

        for (int i = 0; i < activeAssignments.Count; i++)
        {
            FormationAssignment assignment = activeAssignments[i];

            if (assignment.Level < lowestFormedLevel)
            {
                continue;
            }

            int currentLevel = assignment.Level - lowestFormedLevel;
            Vector3 formationWorldPosition = crowdSystem.transform.position
                + transform.right * assignment.HorizontalOffset
                + transform.up * currentLevel * verticalSpacing;
            Vector3 formationLocalPosition = assignment.Runner.transform.parent.InverseTransformPoint(formationWorldPosition);

            assignment.Runner.SetPyramidFormationPosition(formationLocalPosition);
        }

        if (formedLevelCount < Levels)
        {
            formationTickTimer = formationTickInterval;
            return;
        }

        for (int i = 0; i < activeAssignments.Count; i++)
        {
            FormationAssignment assignment = activeAssignments[i];
            assignment.Runner.StartPyramidAdvance(assignment.TargetWorldPosition, transform.forward);
        }

        crowdSystem.FinishPyramidReposition();
        buildingFormation = false;
    }
}
