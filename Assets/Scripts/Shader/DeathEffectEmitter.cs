using UnityEngine;

public class DeathEffectEmitter : MonoBehaviour
{
    [SerializeField] private DeathEffect effectPrefab;
    [SerializeField] private Vector3 effectScale = Vector3.one;
    [ColorUsage(true, true)] [SerializeField] private Color effectColor = Color.white;

    private SplashPool splashPool;

    public void CustomAwake()
    {
        splashPool = ServiceProvider.GetService<SplashPool>();
        splashPool.Preload(effectPrefab);
    }

    public void Play()
    {
        splashPool.Play(effectPrefab, transform, effectScale, effectColor);
    }
}
