using UnityEngine;

public class Chest : DetectCrowdComponent
{
    [Header("Audio")]
    [Min(0)] [SerializeField] private int openSfxIndex;

    private Animator animator;

    private void Awake()
    {
        animator = GetComponentInParent<Animator>();
    }

    private void Update()
    {
        DetectRunner();
    }

    protected override void DetectionAction(Runner runner)
    {
        enabled = false;
        ServiceProvider.GetService<AudioManager>().PlaySFX(openSfxIndex);
        ServiceProvider.GetService<CoinManager>().AddFinalChestReward();
        animator.SetTrigger("Open");
    }
}
