using System.Collections.Generic;
using UnityEngine;

public class CrowdSystem : MonoBehaviour
{
    [SerializeField] private float radius = 1f;
    [SerializeField] private float angle = 1f;

    private readonly List<Runner> runners = new List<Runner>();

    private void Awake()
    {
        ServiceProvider.RegisterService(this);
        RefreshRunners();
    }

    private void Update()
    {
        PlaceRunners();
    }

    private void PlaceRunners()
    {
        RefreshRunners();

        for (int i = 0; i < runners.Count; i++)
        {
            Vector3 runnerPosition = CalculateRunnerPosition(i);
            runners[i].transform.localPosition = runnerPosition;
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

    public void SetRunnersAmount(int amount)
    {
        RefreshRunners();

        int targetAmount = Mathf.Max(1, amount);

        if (targetAmount > runners.Count)
        {
            AddRunners(targetAmount);
        }
        else if (targetAmount < runners.Count)
        {
            RemoveRunners(targetAmount);
        }
    }

    private void AddRunners(int targetAmount)
    {
        Runner runnerTemplate = runners[0];
        Transform runnersParent = runnerTemplate.transform.parent;

        while (runners.Count < targetAmount)
        {
            Runner newRunner = Instantiate(runnerTemplate, runnersParent);
            newRunner.name = runnerTemplate.name;
            newRunner.transform.localPosition = CalculateRunnerPosition(runners.Count);
            newRunner.transform.localRotation = runnerTemplate.transform.localRotation;
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
