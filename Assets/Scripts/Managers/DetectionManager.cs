using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public struct CrowdDetectorData
{
    public float3 Position;
    public quaternion InverseRotation;
    public float3 HalfBoxSize;
    public byte UseRotation;
}

[BurstCompile]
public struct CrowdDetectionJob : IJobParallelFor
{
    [ReadOnly] public NativeArray<CrowdDetectorData> Detectors;
    [ReadOnly] public NativeArray<float3> RunnerPositions;
    [WriteOnly] public NativeArray<byte> DetectionResults;
    public int RunnerCount;

    public void Execute(int index)
    {
        int detectorIndex = index / RunnerCount;
        int runnerIndex = index - detectorIndex * RunnerCount;
        CrowdDetectorData detector = Detectors[detectorIndex];
        float3 runnerPosition = RunnerPositions[runnerIndex] - detector.Position;

        if (detector.UseRotation == 1)
        {
            runnerPosition = math.mul(detector.InverseRotation, runnerPosition);
        }

        bool isInside = math.abs(runnerPosition.x) <= detector.HalfBoxSize.x
            && math.abs(runnerPosition.y) <= detector.HalfBoxSize.y
            && math.abs(runnerPosition.z) <= detector.HalfBoxSize.z;

        DetectionResults[index] = isInside ? (byte)1 : (byte)0;
    }
}

public class DetectionManager
{
    private DetectCrowdComponent[] detectors;
    private readonly List<DetectCrowdComponent> activeDetectors = new List<DetectCrowdComponent>();
    private readonly List<Runner> runnerSnapshot = new List<Runner>();
    private CrowdSystem crowdSystem;
    private NativeArray<CrowdDetectorData> detectorData;
    private NativeArray<float3> runnerPositions;
    private NativeArray<byte> detectionResults;

    public void CustomAwake()
    {
        ServiceProvider.RegisterService(this);
    }

    public void CustomStart()
    {
        detectors = Object.FindObjectsByType<DetectCrowdComponent>(FindObjectsInactive.Include);
        crowdSystem = ServiceProvider.GetService<CrowdSystem>();
    }

    public void CustomUpdate()
    {
        activeDetectors.Clear();
        runnerSnapshot.Clear();

        for (int i = 0; i < detectors.Length; i++)
        {
            if (detectors[i].isActiveAndEnabled && detectors[i].CanDetect)
            {
                activeDetectors.Add(detectors[i]);
            }
        }

        IReadOnlyList<Runner> runners = crowdSystem.GetRunners();

        for (int i = 0; i < runners.Count; i++)
        {
            runnerSnapshot.Add(runners[i]);
        }

        if (activeDetectors.Count == 0 || runnerSnapshot.Count == 0)
        {
            return;
        }

        EnsureDetectorCapacity(activeDetectors.Count);
        EnsureRunnerCapacity(runnerSnapshot.Count);
        EnsureResultCapacity(activeDetectors.Count * runnerSnapshot.Count);

        for (int i = 0; i < activeDetectors.Count; i++)
        {
            detectorData[i] = activeDetectors[i].GetDetectionJobData();
        }

        for (int i = 0; i < runnerSnapshot.Count; i++)
        {
            runnerPositions[i] = runnerSnapshot[i].transform.position;
        }

        CrowdDetectionJob detectionJob = new CrowdDetectionJob
        {
            Detectors = detectorData,
            RunnerPositions = runnerPositions,
            DetectionResults = detectionResults,
            RunnerCount = runnerSnapshot.Count
        };

        int resultCount = activeDetectors.Count * runnerSnapshot.Count;
        JobHandle detectionHandle = detectionJob.Schedule(resultCount, 64);
        detectionHandle.Complete();

        for (int i = 0; i < activeDetectors.Count; i++)
        {
            activeDetectors[i].ApplyDetectionResults(runnerSnapshot, detectionResults, i * runnerSnapshot.Count);
        }
    }

    public void CustomDestroy()
    {
        if (detectorData.IsCreated)
        {
            detectorData.Dispose();
        }

        if (runnerPositions.IsCreated)
        {
            runnerPositions.Dispose();
        }

        if (detectionResults.IsCreated)
        {
            detectionResults.Dispose();
        }
    }

    private void EnsureDetectorCapacity(int requiredCapacity)
    {
        if (detectorData.IsCreated && detectorData.Length >= requiredCapacity)
        {
            return;
        }

        if (detectorData.IsCreated)
        {
            detectorData.Dispose();
        }

        detectorData = new NativeArray<CrowdDetectorData>(Mathf.NextPowerOfTwo(requiredCapacity), Allocator.Persistent);
    }

    private void EnsureRunnerCapacity(int requiredCapacity)
    {
        if (runnerPositions.IsCreated && runnerPositions.Length >= requiredCapacity)
        {
            return;
        }

        if (runnerPositions.IsCreated)
        {
            runnerPositions.Dispose();
        }

        runnerPositions = new NativeArray<float3>(Mathf.NextPowerOfTwo(requiredCapacity), Allocator.Persistent);
    }

    private void EnsureResultCapacity(int requiredCapacity)
    {
        if (detectionResults.IsCreated && detectionResults.Length >= requiredCapacity)
        {
            return;
        }

        if (detectionResults.IsCreated)
        {
            detectionResults.Dispose();
        }

        detectionResults = new NativeArray<byte>(Mathf.NextPowerOfTwo(requiredCapacity), Allocator.Persistent);
    }
}
