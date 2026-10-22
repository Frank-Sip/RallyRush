using System.Collections;
using CartoonFX;
using UnityEngine;

public class DeathEffect : MonoBehaviour
{
    [Min(0f)] [SerializeField] private float duration = 1f;

    private ParticleSystem[] particleSystems;
    private SplashPool splashPool;
    private WaitForSeconds returnDelay;

    public void CustomAwake(SplashPool splashPool)
    {
        this.splashPool = splashPool;
        particleSystems = GetComponentsInChildren<ParticleSystem>(true);
        CFXR_Effect[] effects = GetComponentsInChildren<CFXR_Effect>(true);

        for (int i = 0; i < effects.Length; i++)
        {
            effects[i].clearBehavior = CFXR_Effect.ClearBehavior.None;
        }

        returnDelay = new WaitForSeconds(duration);
    }

    public void Play(Vector3 scale, Color color)
    {
        transform.localScale = scale;

        for (int i = 0; i < particleSystems.Length; i++)
        {
            ParticleSystem.MainModule main = particleSystems[i].main;
            main.startColor = color;

            particleSystems[i].Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
            particleSystems[i].Play(false);
        }

        StartCoroutine(ReturnToPool());
    }

    private IEnumerator ReturnToPool()
    {
        yield return returnDelay;
        splashPool.Return(this);
    }
}
