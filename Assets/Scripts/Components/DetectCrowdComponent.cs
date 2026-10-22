using System.Collections.Generic;
using UnityEngine;

public abstract class DetectCrowdComponent : MonoBehaviour
{
    [SerializeField] protected Vector3 detectionBoxSize = new Vector3(4f, 2f, 1f);

    protected CrowdSystem crowdSystem;

    protected virtual void Start()
    {
        crowdSystem = ServiceProvider.GetService<CrowdSystem>();
    }

    protected void DetectRunner()
    {
        IReadOnlyList<Runner> runners = crowdSystem.GetRunners();
        Vector3 halfBoxSize = detectionBoxSize * 0.5f;

        for (int i = 0; i < runners.Count; i++)
        {
            Runner runner = runners[i];
            Vector3 runnerPosition = runner.transform.position - transform.position;

            bool isInside = Mathf.Abs(runnerPosition.x) <= halfBoxSize.x
                && Mathf.Abs(runnerPosition.y) <= halfBoxSize.y
                && Mathf.Abs(runnerPosition.z) <= halfBoxSize.z;

            if (isInside)
            {
                DetectionAction(runner);
                return;
            }
        }
    }

    protected abstract void DetectionAction(Runner runner);

    protected virtual void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, detectionBoxSize);
    }
}
