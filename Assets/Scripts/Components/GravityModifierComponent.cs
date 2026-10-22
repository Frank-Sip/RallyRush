using UnityEngine;

public class GravityModifierComponent : DetectCrowdComponent
{
    protected override void DetectionAction(Runner runner)
    {
        ToggleRunnerGravity(runner);
    }

    public void ToggleRunnerGravity(Runner runner)
    {
        runner.ToggleGravity();
    }

    protected override Color DetectionGizmoColor => Color.cyan;
}
