using UnityEngine;

public class RouletteTrigger : DetectCrowdComponent
{
    private MovementController movementController;

    protected override void Start()
    {
        base.Start();
        movementController = ServiceProvider.GetService<MovementController>();
    }

    protected override void DetectionAction(Runner runner)
    {
        BossEnemy bossEnemy = ServiceProvider.GetService<BossEnemy>();
        enabled = false;
        movementController.SetRunningTarget(bossEnemy.transform);
        GameManager.Instance.SetGameStatus(GameStatus.Roullete);
    }
}
