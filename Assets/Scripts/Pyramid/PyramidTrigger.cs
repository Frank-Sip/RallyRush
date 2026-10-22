using System;
using UnityEngine;

public class PyramidTrigger : DetectCrowdComponent
{
    [SerializeField] private PyramidFormation pyramidFormation;

    private MovementController movementController;

    public event Action StartPyramid;

    private void Awake()
    {
        ServiceProvider.RegisterService(this);
    }

    protected override void Start()
    {
        base.Start();
        movementController = ServiceProvider.GetService<MovementController>();
    }

    private void Update()
    {
        DetectRunner();
    }

    protected override void DetectionAction(Runner runner)
    {
        enabled = false;
        movementController.CenterPlayer();
        StartPyramid?.Invoke();
        pyramidFormation.StartPyramidFormation();
    }
}
