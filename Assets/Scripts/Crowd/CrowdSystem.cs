using System.Collections.Generic;
using UnityEngine;

public class CrowdSystem : MonoBehaviour
{
    [SerializeField] private float radius = 1f;
    [SerializeField] private float angle = 1f;

    private readonly List<Runner> runners = new List<Runner>();

    private void Update()
    {
        PlaceRunners();
    }

    private void PlaceRunners()
    {
        runners.Clear();
        GetComponentsInChildren(false, runners);

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
}
