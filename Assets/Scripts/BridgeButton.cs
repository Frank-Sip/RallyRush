using UnityEngine;

public class BridgeButton : DetectCrowdComponent
{
    [SerializeField] private Animator leftBridge;
    [SerializeField] private Animator rightBridge;
    [SerializeField] private GameObject gravityPoint;

    [Header("Audio")]
    [Min(0)] [SerializeField] private int buttonSfxIndex;
    [Min(0)] [SerializeField] private int bridgeAnimationSfxIndex;

    private void Update()
    {
        DetectRunner();
    }

    protected override void DetectionAction(Runner runner)
    {
        enabled = false;
        AudioManager audioManager = ServiceProvider.GetService<AudioManager>();
        audioManager.PlaySFX(buttonSfxIndex);
        leftBridge.SetTrigger("Rotate");
        rightBridge.SetTrigger("Rotate");
        audioManager.PlaySFX(bridgeAnimationSfxIndex);
        gravityPoint.SetActive(false);
    }
}
