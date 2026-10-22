using UnityEngine;

public class RunnerPool
{
    private readonly int initialCount;
    private ObjectPool<Runner> runnerPool;
    private Runner runnerPrefab;
    private Transform parent;
    private CrowdSystem crowdSystem;

    public RunnerPool(Runner runnerPrefab, Transform parent, int initialCount, CrowdSystem crowdSystem)
    {
        this.initialCount = initialCount;
        this.parent = parent;
        this.runnerPrefab = runnerPrefab;
        this.crowdSystem = crowdSystem;

        Initialize();
    }

    private void Initialize()
    {
        runnerPool = new ObjectPool<Runner>(runnerPrefab, parent, initialCount, InitializeRunner);
    }

    private void InitializeRunner(Runner runner)
    {
        runner.CustomAwake(crowdSystem);
        runner.transform.position = crowdSystem.transform.position;
    }

    public Runner Get()
    {
        return runnerPool.Get();
    }

    public void Return(Runner runner)
    {
        runner.transform.position = crowdSystem.transform.position;
        runnerPool.Return(runner);
    }
}
