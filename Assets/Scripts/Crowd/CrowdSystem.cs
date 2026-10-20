using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum RunnerBehaviour
{
    Running,
    Combat,
    Pyramid,
    Victory
}

public class CrowdSystem : MonoBehaviour
{
    [SerializeField] private float radius = 1f;
    [SerializeField] private float angle = 1f;
    [Min(0)] [SerializeField] private float combatExitStaggerDuration = 0.5f;
    [Min(0)] [SerializeField] private float deathRepositionDelay = 0.25f;
    [Min(1)] [SerializeField] private int maxRunners = 999;

    private readonly List<Runner> runners = new List<Runner>();

    private MovementController movementController;
    private CrowdCount crowdCount;
    private Coroutine deathRepositionCoroutine;
    private bool hasStarted;

    public RunnerBehaviour Behaviour { get; private set; } = RunnerBehaviour.Running;
    public Transform CombatTarget { get; private set; }
    public bool IsPyramidRepositioning { get; private set; }

    public event Action<int> RunnersAmountChanged;

    private readonly struct FormationAssignment
    {
        public readonly Runner Runner;
        public readonly Vector3 TargetPosition;
        public readonly int FormationIndex;

        public FormationAssignment(Runner runner, Vector3 targetPosition, int formationIndex)
        {
            Runner = runner;
            TargetPosition = targetPosition;
            FormationIndex = formationIndex;
        }
    }

    private void Awake()
    {
        ServiceProvider.RegisterService(this);
        movementController = GetComponentInParent<MovementController>();
        crowdCount = GetComponentInChildren<CrowdCount>();
        RefreshRunners();

        for (int i = 0; i < runners.Count; i++)
        {
            runners[i].CustomAwake(this);
            runners[i].SetFormationIndex(i);
        }
    }

    private void Start()
    {
        hasStarted = true;

        for (int i = 0; i < runners.Count; i++)
        {
            runners[i].CustomStart();
        }
    }

    private void Update()
    {
        int runnerIndex = 0;

        while (runnerIndex < runners.Count)
        {
            Runner runner = runners[runnerIndex];
            runner.CustomUpdate();

            if (runnerIndex < runners.Count && runners[runnerIndex] == runner)
            {
                runnerIndex++;
            }
        }
    }

    private void LateUpdate()
    {
        for (int i = 0; i < runners.Count; i++)
        {
            runners[i].CustomLateUpdate();
        }
    }

    public void PlaceRunnersForBigEnemy(Transform center)
    {
        Vector3 centerLocalPosition = transform.InverseTransformPoint(center.position);
        centerLocalPosition.y = 0f;
        List<Runner> availableRunners = new List<Runner>(runners);

        for (int i = 0; i < runners.Count; i++)
        {
            float distanceFromCenter = radius * Mathf.Sqrt(i);
            float angleInRadians = Mathf.Deg2Rad * i * angle;

            Vector3 runnerPosition = centerLocalPosition + new Vector3(
                distanceFromCenter * Mathf.Cos(angleInRadians),
                0f,
                distanceFromCenter * Mathf.Sin(angleInRadians));

            int closestRunnerIndex = FindClosestRunnerIndex(availableRunners, runnerPosition);
            Runner closestRunner = availableRunners[closestRunnerIndex];

            closestRunner.SetCombatEnterTargetPosition(runnerPosition);
            availableRunners.RemoveAt(closestRunnerIndex);
        }
    }

    private static int FindClosestRunnerIndex(List<Runner> availableRunners, Vector3 targetPosition)
    {
        int closestRunnerIndex = 0;
        float closestDistanceSqr = float.MaxValue;

        for (int i = 0; i < availableRunners.Count; i++)
        {
            float distanceSqr = (availableRunners[i].transform.localPosition - targetPosition).sqrMagnitude;

            if (distanceSqr < closestDistanceSqr)
            {
                closestDistanceSqr = distanceSqr;
                closestRunnerIndex = i;
            }
        }

        return closestRunnerIndex;
    }

    private Vector3 CalculateRunnerPosition(int index)
    {
        float x = radius * Mathf.Sqrt(index) * Mathf.Cos(Mathf.Deg2Rad * index * angle);
        float z = radius * Mathf.Sqrt(index) * Mathf.Sin(Mathf.Deg2Rad * index * angle);
        return new Vector3(x, 0f, z);
    }

    public float GetCrowdRadius()
    {
        float farthestDistanceSqr = 0f;

        for (int i = 0; i < runners.Count; i++)
        {
            Vector3 distanceFromCenter = runners[i].transform.position - transform.position;
            distanceFromCenter.y = 0f;
            farthestDistanceSqr = Mathf.Max(farthestDistanceSqr, distanceFromCenter.sqrMagnitude);
        }

        return Mathf.Sqrt(farthestDistanceSqr);
    }

    public IReadOnlyList<Runner> GetRunners()
    {
        return runners;
    }

    public int GetRunnersAmount()
    {
        return runners.Count;
    }

    public void SetVictoryAnimation()
    {
        Behaviour = RunnerBehaviour.Victory;
        CombatTarget = null;

        for (int i = 0; i < runners.Count; i++)
        {
            runners[i].SetAnimTrigger("Victory");
        }
    }

    public bool RemoveRunner(Runner runner)
    {
        if (!runners.Remove(runner))
        {
            return false;
        }

        RunnersAmountChanged?.Invoke(runners.Count);
        CrowdDeath();
        return true;
    }

    public void CrowdDeath()
    {
        if (runners.Count <= 0)
        {
            crowdCount.gameObject.SetActive(false);
            GameManager.Instance.SetGameStatus(GameStatus.GameOver);
        }
    }

    public void RestartDeathRepositionTimer()
    {
        if (Behaviour != RunnerBehaviour.Running || runners.Count == 0)
        {
            return;
        }

        if (deathRepositionCoroutine != null)
        {
            StopCoroutine(deathRepositionCoroutine);
        }

        deathRepositionCoroutine = StartCoroutine(WaitForDeathReposition());
    }

    private IEnumerator WaitForDeathReposition()
    {
        yield return new WaitForSeconds(deathRepositionDelay);
        deathRepositionCoroutine = null;

        if (Behaviour == RunnerBehaviour.Running)
        {
            RepositionAfterRunnerDeaths();
        }
    }

    private void RepositionAfterRunnerDeaths()
    {
        List<Runner> availableRunners = new List<Runner>(runners);

        for (int formationIndex = 0; formationIndex < runners.Count; formationIndex++)
        {
            Vector3 targetPosition = CalculateRunnerPosition(formationIndex);
            int closestRunnerIndex = FindClosestRunnerIndex(availableRunners, targetPosition);
            Runner closestRunner = availableRunners[closestRunnerIndex];

            closestRunner.SetDeathRepositionTargetPosition(targetPosition, formationIndex);
            availableRunners.RemoveAt(closestRunnerIndex);
        }

        runners.Sort((firstRunner, secondRunner) =>
            firstRunner.FormationIndex.CompareTo(secondRunner.FormationIndex));
    }

    public void EnterCombat(Transform combatTarget)
    {
        Behaviour = RunnerBehaviour.Combat;
        CombatTarget = combatTarget;

        for (int i = 0; i < runners.Count; i++)
        {
            runners[i].UseNormalEnemyAttackSpeed();
        }

        movementController.EnterCombat(combatTarget);
    }

    public void EnterCombat(Transform combatTarget, float combatSpeed)
    {
        Behaviour = RunnerBehaviour.Combat;
        CombatTarget = combatTarget;

        for (int i = 0; i < runners.Count; i++)
        {
            runners[i].UseBigEnemyAttackSpeed();
        }

        movementController.EnterCombat(combatTarget, combatSpeed);
    }

    public void ExitCombat()
    {
        Behaviour = RunnerBehaviour.Running;
        CloseFormationGaps();
        ClearRunnerCombatTargets();
        CombatTarget = null;
        movementController.ExitCombat();
    }

    public void ExitBigEnemyCombat()
    {
        Behaviour = RunnerBehaviour.Running;
        RestoreRunningFormation();
        ClearRunnerCombatTargets();
        CombatTarget = null;
        movementController.ExitCombat();
    }

    private void ClearRunnerCombatTargets()
    {
        for (int i = 0; i < runners.Count; i++)
        {
            runners[i].SetCombatEnemyTarget(null);
        }
    }

    public void EnterPyramid()
    {
        Behaviour = RunnerBehaviour.Pyramid;
        IsPyramidRepositioning = true;
        crowdCount.gameObject.SetActive(false);
    }

    public void FinishPyramidReposition()
    {
        IsPyramidRepositioning = false;
    }

    private void RestoreRunningFormation()
    {
        List<Runner> availableRunners = new List<Runner>(runners);
        List<FormationAssignment> assignments = new List<FormationAssignment>(runners.Count);

        for (int formationIndex = 0; formationIndex < runners.Count; formationIndex++)
        {
            Vector3 targetPosition = CalculateRunnerPosition(formationIndex);
            int closestRunnerIndex = FindClosestRunnerIndex(availableRunners, targetPosition);
            Runner closestRunner = availableRunners[closestRunnerIndex];

            assignments.Add(new FormationAssignment(closestRunner, targetPosition, formationIndex));
            availableRunners.RemoveAt(closestRunnerIndex);
        }

        assignments.Sort((firstAssignment, secondAssignment) =>
            firstAssignment.TargetPosition.z.CompareTo(secondAssignment.TargetPosition.z));

        for (int i = 0; i < assignments.Count; i++)
        {
            FormationAssignment assignment = assignments[i];
            float delay = combatExitStaggerDuration * i / Mathf.Max(1, assignments.Count - 1);

            assignment.Runner.SetCombatExitTargetPosition(
                assignment.TargetPosition,
                assignment.FormationIndex,
                delay);
        }

        runners.Sort((firstRunner, secondRunner) =>
            firstRunner.FormationIndex.CompareTo(secondRunner.FormationIndex));
    }

    private void CloseFormationGaps()
    {
        List<int> emptyPositionIndices = new List<int>();

        for (int positionIndex = 0; positionIndex < runners.Count; positionIndex++)
        {
            bool positionIsOccupied = false;

            for (int runnerIndex = 0; runnerIndex < runners.Count; runnerIndex++)
            {
                if (runners[runnerIndex].FormationIndex == positionIndex)
                {
                    positionIsOccupied = true;
                    break;
                }
            }

            if (!positionIsOccupied)
            {
                emptyPositionIndices.Add(positionIndex);
            }
        }

        List<Runner> availableRunners = new List<Runner>(runners);

        while (emptyPositionIndices.Count > 0)
        {
            int closestRunnerIndex = -1;
            int closestPositionIndex = -1;
            float closestDistanceSqr = float.MaxValue;

            for (int runnerIndex = 0; runnerIndex < availableRunners.Count; runnerIndex++)
            {
                Runner runner = availableRunners[runnerIndex];

                for (int positionIndex = 0; positionIndex < emptyPositionIndices.Count; positionIndex++)
                {
                    int emptyPositionIndex = emptyPositionIndices[positionIndex];

                    if (runner.FormationIndex <= emptyPositionIndex)
                    {
                        continue;
                    }

                    Vector3 targetPosition = CalculateRunnerPosition(emptyPositionIndex);
                    float distanceSqr = (runner.transform.localPosition - targetPosition).sqrMagnitude;

                    if (distanceSqr < closestDistanceSqr)
                    {
                        closestDistanceSqr = distanceSqr;
                        closestRunnerIndex = runnerIndex;
                        closestPositionIndex = positionIndex;
                    }
                }
            }

            Runner closestRunner = availableRunners[closestRunnerIndex];
            int previousPositionIndex = closestRunner.FormationIndex;
            int reservedPositionIndex = emptyPositionIndices[closestPositionIndex];

            closestRunner.SetCombatExitTargetPosition(
                CalculateRunnerPosition(reservedPositionIndex),
                reservedPositionIndex);

            availableRunners.RemoveAt(closestRunnerIndex);
            emptyPositionIndices.RemoveAt(closestPositionIndex);

            if (previousPositionIndex < runners.Count)
            {
                emptyPositionIndices.Add(previousPositionIndex);
            }
        }

        runners.Sort((firstRunner, secondRunner) =>
            firstRunner.FormationIndex.CompareTo(secondRunner.FormationIndex));
    }

    public void SetRunnersAmount(int amount)
    {
        runners.Sort((firstRunner, secondRunner) => firstRunner.FormationIndex.CompareTo(secondRunner.FormationIndex));

        int targetAmount = Mathf.Clamp(amount, 1, maxRunners);

        if (targetAmount > runners.Count)
        {
            AddRunners(targetAmount);
        }
        else if (targetAmount < runners.Count)
        {
            RemoveRunners(targetAmount);
        }

        RunnersAmountChanged?.Invoke(runners.Count);
    }

    private void AddRunners(int targetAmount)
    {
        Runner runnerTemplate = runners[0];
        Transform runnersParent = runnerTemplate.transform.parent;
        HashSet<int> occupiedFormationIndices = new HashSet<int>();

        for (int i = 0; i < runners.Count; i++)
        {
            occupiedFormationIndices.Add(runners[i].FormationIndex);
        }

        int formationIndex = 0;

        while (runners.Count < targetAmount)
        {
            while (occupiedFormationIndices.Contains(formationIndex))
            {
                formationIndex++;
            }

            Runner newRunner = Instantiate(runnerTemplate, runnersParent);
            newRunner.name = runnerTemplate.name;
            newRunner.transform.localRotation = runnerTemplate.transform.localRotation;
            RegisterRunner(newRunner);
            newRunner.SetDoorTargetPosition(CalculateRunnerPosition(formationIndex), formationIndex);
            occupiedFormationIndices.Add(formationIndex);
        }
    }

    private void RegisterRunner(Runner runner)
    {
        runners.Add(runner);
        runner.CustomAwake(this);

        if (hasStarted)
        {
            runner.CustomStart();
        }
    }

    private void RemoveRunners(int targetAmount)
    {
        runners.Sort((firstRunner, secondRunner) =>
            firstRunner.FormationIndex.CompareTo(secondRunner.FormationIndex));

        for (int i = runners.Count - 1; i >= targetAmount; i--)
        {
            Runner runnerToRemove = runners[i];
            runners.RemoveAt(i);
            Destroy(runnerToRemove.gameObject);
        }
    }

    private void RefreshRunners()
    {
        runners.Clear();
        GetComponentsInChildren(false, runners);
    }
}
