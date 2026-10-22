using UnityEngine;

public class RouletteTrigger : DetectCrowdComponent
{
    private MovementController movementController;
    private BossEnemy bossEnemy;

    protected override void Start()
    {
        base.Start();
        movementController = ServiceProvider.GetService<MovementController>();
        bossEnemy = ServiceProvider.GetService<BossEnemy>();
    }

    private void Update()
    {
        DetectRunner();
    }

    protected override void DetectionAction(Runner runner)
    {
        enabled = false;
        movementController.SetRunningTarget(bossEnemy.transform);
        GameManager.Instance.SetGameStatus(GameStatus.Roullete);
    }
}
