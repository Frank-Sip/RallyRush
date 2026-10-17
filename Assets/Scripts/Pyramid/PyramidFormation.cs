using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[SelectionBase]
public class PyramidFormation : MonoBehaviour
{
    [Header("Step Generation")]
    [SerializeField] private GameObject stepPrefab;
    [Min(0)][SerializeField] private int stepAmount;
    [SerializeField] private float stepOffsetY = 1f;
    [SerializeField] private float stepOffsetZ = 1f;

    [Header("Runner Formation Settings")]
    [Min(0f)][SerializeField] private float horizontalSpacing = 0.6f;
    [Min(0f)][SerializeField] private float verticalSpacing = 1f;
    [Min(1)][SerializeField] private int remainingRunnerDistributionLevels = 2;
    [Min(0f)][SerializeField] private float formationTickInterval = 0.1f;

    private CrowdSystem crowdSystem;
    [SerializeField, HideInInspector] private Transform[] steps;
    private List<FormationAssignment> activeAssignments;
    private float formationTickTimer;
    private int formedLevelCount;
    private bool buildingFormation;
    private bool checkingTargetCompletion;
    private bool firstLevelEventSent;

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
    public Runner LastLevelRunner { get; private set; }

    public event Action FirstLevelComplete;
    public event Action<Runner> LastLevelComplete;

    private void Awake()
    {
        ServiceProvider.RegisterService(this);
    }

    private void Start()
    {
        crowdSystem = ServiceProvider.GetService<CrowdSystem>();
        GetSteps();
    }

    private void GetSteps()
    {
        steps = new Transform[transform.childCount];

        for (int i = 0; i < transform.childCount; i++)
        {
            steps[i] = transform.GetChild(i);
        }
    }

#if UNITY_EDITOR
    [ContextMenu("Create Steps")]
    private void CreateSteps()
    {
        steps = new Transform[stepAmount];

        for (int i = 0; i < stepAmount; i++)
        {
            GameObject newStep = (GameObject)PrefabUtility.InstantiatePrefab(stepPrefab, transform);
            Undo.RegisterCreatedObjectUndo(newStep, "Create Pyramid Step");

            newStep.name = $"{stepPrefab.name} {i}";
            newStep.transform.localPosition = new Vector3(0f, stepOffsetY * i, stepOffsetZ * i);
            newStep.transform.localRotation = Quaternion.identity;
            steps[i] = newStep.transform;
        }

        EditorUtility.SetDirty(this);
    }

    [ContextMenu("Delete Steps")]
    private void DeleteSteps()
    {
        for (int i = 0; i < steps.Length; i++)
        {
            Undo.DestroyObjectImmediate(steps[i].gameObject);
        }

        steps = Array.Empty<Transform>();
        EditorUtility.SetDirty(this);
    }
#endif

    private void Update()
    {
        if (buildingFormation)
        {
            formationTickTimer -= Time.deltaTime;

            if (formationTickTimer <= 0f)
            {
                BuildNextLevel();
            }

            return;
        }

        if (checkingTargetCompletion)
        {
            CheckTargetCompletion();
        }
    }

    public void CalculateLevels()
    {
        int runnerAmount = crowdSystem.GetRunnersAmount();
        Levels = 0;
        UsedRunners = 0;

        while (Levels < steps.Length && UsedRunners + Levels + 1 <= runnerAmount)
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
        LastLevelRunner = activeAssignments[0].Runner;
        crowdSystem.EnterPyramid();
        formedLevelCount = 0;
        firstLevelEventSent = false;
        checkingTargetCompletion = false;
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
                Transform step = steps[level];
                Vector3 targetWorldPosition = step.position + step.right * horizontalOffset;

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
        checkingTargetCompletion = true;
    }

    private void CheckTargetCompletion()
    {
        bool firstLevelCompleted = true;
        bool lastLevelCompleted = true;

        for (int i = 0; i < activeAssignments.Count; i++)
        {
            FormationAssignment assignment = activeAssignments[i];

            if (!assignment.Runner.PyramidTargetReached)
            {
                if (assignment.Level == 0)
                {
                    firstLevelCompleted = false;
                }

                if (assignment.Level == Levels - 1)
                {
                    lastLevelCompleted = false;
                }
            }
        }

        if (!firstLevelEventSent && firstLevelCompleted)
        {
            firstLevelEventSent = true;
            FirstLevelComplete?.Invoke();
        }

        if (lastLevelCompleted)
        {
            LastLevelComplete?.Invoke(activeAssignments[0].Runner);
            checkingTargetCompletion = false;
            GameManager.Instance.SetVictoryStatus();
        }
    }
}
