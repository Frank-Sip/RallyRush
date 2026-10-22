using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public abstract class DetectCrowdComponent : MonoBehaviour
{
    [FormerlySerializedAs("runnerAttackBoxSize")]
    [SerializeField] protected Vector3 detectionBoxSize = new Vector3(4f, 2f, 1f);

    private readonly HashSet<Runner> runnersInside = new HashSet<Runner>();
    private readonly HashSet<Runner> runnersDetectedThisFrame = new HashSet<Runner>();

    protected CrowdSystem crowdSystem;
    protected virtual Transform DetectionOrigin => transform;
    protected virtual bool UseDetectionRotation => false;
    protected virtual Color DetectionGizmoColor => Color.yellow;

    protected virtual void Start()
    {
        crowdSystem = ServiceProvider.GetService<CrowdSystem>();
    }

    public virtual void CustomUpdate()
    {
        DetectRunner();
    }

    protected void DetectRunner()
    {
        IReadOnlyList<Runner> runners = crowdSystem.GetRunners();
        runnersDetectedThisFrame.Clear();

        for (int i = runners.Count - 1; i >= 0; i--)
        {
            Runner runner = runners[i];

            if (!IsRunnerInside(runner))
            {
                continue;
            }

            runnersDetectedThisFrame.Add(runner);

            if (runnersInside.Add(runner))
            {
                DetectionAction(runner);
            }

            if (!enabled)
            {
                return;
            }
        }

        runnersInside.IntersectWith(runnersDetectedThisFrame);
    }

    protected abstract void DetectionAction(Runner runner);

    protected void ResetDetectedRunners()
    {
        runnersInside.Clear();
    }

    private bool IsRunnerInside(Runner runner)
    {
        Transform detectionOrigin = DetectionOrigin;
        Vector3 runnerPosition = runner.transform.position - detectionOrigin.position;

        if (UseDetectionRotation)
        {
            runnerPosition = Quaternion.Inverse(detectionOrigin.rotation) * runnerPosition;
        }

        Vector3 halfBoxSize = detectionBoxSize * 0.5f;

        return Mathf.Abs(runnerPosition.x) <= halfBoxSize.x
            && Mathf.Abs(runnerPosition.y) <= halfBoxSize.y
            && Mathf.Abs(runnerPosition.z) <= halfBoxSize.z;
    }

    protected virtual void OnDrawGizmos()
    {
        Transform detectionOrigin = DetectionOrigin;
        Gizmos.color = DetectionGizmoColor;

        if (!UseDetectionRotation)
        {
            Gizmos.DrawWireCube(detectionOrigin.position, detectionBoxSize);
            return;
        }

        Matrix4x4 previousMatrix = Gizmos.matrix;
        Gizmos.matrix = Matrix4x4.TRS(detectionOrigin.position, detectionOrigin.rotation, Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, detectionBoxSize);
        Gizmos.matrix = previousMatrix;
    }
}
