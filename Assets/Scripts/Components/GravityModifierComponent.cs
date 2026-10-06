using System.Collections.Generic;
using UnityEngine;

public class GravityModifierComponent : MonoBehaviour
{
    [SerializeField] private Vector3 detectionBoxSize = new Vector3(4f, 2f, 1f);

    private readonly HashSet<Runner> runnersInside = new HashSet<Runner>();
    private CrowdSystem crowdSystem;

    private void Start()
    {
        crowdSystem = ServiceProvider.GetService<CrowdSystem>();
    }

    private void Update()
    {
        DetectRunners();
    }

    private void DetectRunners()
    {
        IReadOnlyList<Runner> runners = crowdSystem.GetRunners();
        Vector3 halfBoxSize = detectionBoxSize * 0.5f;

        for (int i = 0; i < runners.Count; i++)
        {
            Runner runner = runners[i];
            Vector3 runnerPosition = runner.transform.position - transform.position;

            bool isInside = Mathf.Abs(runnerPosition.x) <= halfBoxSize.x && Mathf.Abs(runnerPosition.y) <= halfBoxSize.y && Mathf.Abs(runnerPosition.z) <= halfBoxSize.z;

            if (isInside && runnersInside.Add(runner))
            {
                ToggleRunnerGravity(runner);
            }
            else if (!isInside)
            {
                runnersInside.Remove(runner);
            }
        }
    }

    public void ToggleRunnerGravity(Runner runner)
    {
        runner.ToggleGravity();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(transform.position, detectionBoxSize);
    }
}
