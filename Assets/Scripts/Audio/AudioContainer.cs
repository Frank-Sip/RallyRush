using UnityEngine;
using UnityEngine.Serialization;

[System.Serializable]
public class SfxData
{
    [SerializeField] private AudioClip clip;
    [Min(0f)] [SerializeField] private float cooldown;

    public AudioClip Clip => clip;
    public float Cooldown => cooldown;
}

[CreateAssetMenu(fileName = "AudioContainer", menuName = "Scriptable Objects/AudioContainer")]
public class AudioContainer : ScriptableObject
{
    [FormerlySerializedAs("audioClips")]
    [SerializeField] private AudioClip[] loopingSfxClips;
    [SerializeField] private SfxData[] sfxClips;

    public AudioClip GetLoopingSfx(int index)
    {
        return loopingSfxClips[index];
    }

    public SfxData GetSfx(int index)
    {
        return sfxClips[index];
    }

    public int GetSfxCount()
    {
        return sfxClips.Length;
    }
}
