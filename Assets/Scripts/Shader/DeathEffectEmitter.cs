using UnityEngine;

public class DeathEffectEmitter : MonoBehaviour
{
    [SerializeField] private DeathEffect effectPrefab;
    [SerializeField] private Vector3 effectScale = Vector3.one;
    [ColorUsage(true, true)] [SerializeField] private Color effectColor = Color.white;

    public void Play()
    {
        DeathEffect effect = Instantiate(effectPrefab, transform.position, transform.rotation);
        effect.Play(effectScale, effectColor);
    }
}
