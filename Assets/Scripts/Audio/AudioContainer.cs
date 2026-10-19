using UnityEngine;

[CreateAssetMenu(fileName = "AudioContainer", menuName = "Scriptable Objects/AudioContainer")]
public class AudioContainer : ScriptableObject
{
    [SerializeField] private AudioClip[] audioClips;

    public AudioClip GetAudioClip(int index)
    {
        return audioClips[index];
    }
}
