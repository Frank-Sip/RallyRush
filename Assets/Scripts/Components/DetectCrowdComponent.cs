using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
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
    public virtual bool CanDetect => true;

    protected virtual void Start()
    {
        crowdSystem = ServiceProvider.GetService<CrowdSystem>();
    }

    public CrowdDetectorData GetDetectionJobData()
    {
        Transform detectionOrigin = DetectionOrigin;

        return new CrowdDetectorData
        {
            Position = detectionOrigin.position,
            InverseRotation = math.inverse((quaternion)detectionOrigin.rotation),
            HalfBoxSize = detectionBoxSize * 0.5f,
            UseRotation = UseDetectionRotation ? (byte)1 : (byte)0
        };
    }

    public void ApplyDetectionResults(IReadOnlyList<Runner> runners, NativeArray<byte> results, int resultOffset)
    {
        runnersDetectedThisFrame.Clear();

        for (int i = runners.Count - 1; i >= 0; i--)
        {
            Runner runner = runners[i];

            if (results[resultOffset + i] == 0 || !runner.isActiveAndEnabled)
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
