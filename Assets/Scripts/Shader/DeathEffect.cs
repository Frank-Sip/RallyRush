using UnityEngine;

public class DeathEffect : MonoBehaviour
{
    [Min(0f)] [SerializeField] private float duration = 1f;

    private ParticleSystem[] particleSystems;

    private void Awake()
    {
        particleSystems = GetComponentsInChildren<ParticleSystem>(true);
    }

    public void Play(Vector3 scale, Color color)
    {
        gameObject.SetActive(true);
        transform.localScale = scale;

        for (int i = 0; i < particleSystems.Length; i++)
        {
            ParticleSystem.MainModule main = particleSystems[i].main;
            main.startColor = color;

            particleSystems[i].Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
            particleSystems[i].Play(false);
        }

        Destroy(gameObject, duration);
    }
}
