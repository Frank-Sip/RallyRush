using System;
using System.Collections.Generic;
using UnityEngine;

public enum RunnerBehaviour
{
    Running,
    Combat
}

public class CrowdSystem : MonoBehaviour
{
    [SerializeField] private float radius = 1f;
    [SerializeField] private float angle = 1f;
    [Min(1)] [SerializeField] private int maxRunners = 999;

    private readonly List<Runner> runners = new List<Runner>();

    private MovementController movementController;

    public RunnerBehaviour Behaviour { get; private set; } = RunnerBehaviour.Running;
    public Transform CombatTarget { get; private set; }

    public event Action<int> RunnersAmountChanged;

    private void Awake()
    {
        ServiceProvider.RegisterService(this);
        movementController = GetComponentInParent<MovementController>();
        RefreshRunners();

        for (int i = 0; i < runners.Count; i++)
        {
            runners[i].SetFormationIndex(i);
        }

        if (runners.Count > maxRunners)
        {
            RemoveRunners(maxRunners);
        }
    }

    private void PlaceRunners()
    {
        for (int i = 0; i < runners.Count; i++)
        {
            Vector3 runnerPosition = CalculateRunnerPosition(i);
            runners[i].SetDoorTargetPosition(runnerPosition, i);
        }
    }

    private void PlaceRunnersAfterCombat()
    {
        for (int i = 0; i < runners.Count; i++)
        {
            Vector3 runnerPosition = CalculateRunnerPosition(i);
            runners[i].SetCombatExitTargetPosition(runnerPosition, i);
        }
    }

    public void PlaceRunners(Transform center)
    {
        Vector3 centerLocalPosition = transform.InverseTransformPoint(center.position);
        centerLocalPosition.y = 0f;

        for (int i = 0; i < runners.Count; i++)
        {
            float distanceFromCenter = radius * Mathf.Sqrt(i);
            float angleInRadians = Mathf.Deg2Rad * i * angle;

            Vector3 runnerPosition = centerLocalPosition + new Vector3(
                distanceFromCenter * Mathf.Cos(angleInRadians),
                0f,
                distanceFromCenter * Mathf.Sin(angleInRadians));

            runners[i].SetCombatEnterTargetPosition(runnerPosition, i);
        }
    }

    private Vector3 CalculateRunnerPosition(int index)
    {
        float x = radius * Mathf.Sqrt(index) * Mathf.Cos(Mathf.Deg2Rad * index * angle);
        float z = radius * Mathf.Sqrt(index) * Mathf.Sin(Mathf.Deg2Rad * index * angle);
        return new Vector3(x, 0f, z);
    }

    public float GetCrowdRadius()
    {
        return radius * Mathf.Sqrt(runners.Count);
    }

    public IReadOnlyList<Runner> GetRunners()
    {
        return runners;
    }

    public int GetRunnersAmount()
    {
        return runners.Count;
    }

    public bool RemoveRunner(Runner runner)
    {
        if (!runners.Remove(runner))
        {
            return false;
        }

        RunnersAmountChanged?.Invoke(runners.Count);
        return true;
    }

    public void EnterCombat(Transform combatTarget)
    {
        Behaviour = RunnerBehaviour.Combat;
        CombatTarget = combatTarget;
        movementController.EnterCombat(combatTarget);
    }

    public void EnterCombat(Transform combatTarget, float combatSpeed)
    {
        Behaviour = RunnerBehaviour.Combat;
        CombatTarget = combatTarget;
        movementController.EnterCombat(combatTarget, combatSpeed);
    }

    public void ExitCombat()
    {
        Behaviour = RunnerBehaviour.Running;
        CloseFormationGaps();
        CombatTarget = null;
        movementController.ExitCombat();
    }

    public void ExitBigEnemyCombat()
    {
        Behaviour = RunnerBehaviour.Running;
        PlaceRunnersAfterCombat();
        CombatTarget = null;
        movementController.ExitCombat();
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
        RefreshRunners();
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

        PlaceRunners();
        RunnersAmountChanged?.Invoke(runners.Count);
    }

    private void AddRunners(int targetAmount)
    {
        Runner runnerTemplate = runners[0];
        Transform runnersParent = runnerTemplate.transform.parent;

        while (runners.Count < targetAmount)
        {
            Runner newRunner = Instantiate(runnerTemplate, runnersParent);
            newRunner.name = runnerTemplate.name;
            newRunner.transform.localRotation = runnerTemplate.transform.localRotation;
            int formationIndex = runners.Count;
            newRunner.SetDoorTargetPosition(CalculateRunnerPosition(formationIndex), formationIndex);
            runners.Add(newRunner);
        }
    }

    private void RemoveRunners(int targetAmount)
    {
        for (int i = runners.Count - 1; i >= targetAmount; i--)
        {
            Destroy(runners[i].gameObject);
            runners.RemoveAt(i);
        }
    }

    private void RefreshRunners()
    {
        runners.Clear();
        GetComponentsInChildren(false, runners);
    }
}
