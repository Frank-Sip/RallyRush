using UnityEngine;

public class AudioManager
{
    private AudioContainer audioContainer;
    private AudioSource audioSource;

    public void Initialize(AudioContainer container, AudioSource source)
    {
        audioContainer = container;
        audioSource = source;
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;
    }

    public void PlaySFX(int audioIndex)
    {
        audioSource.PlayOneShot(audioContainer.GetAudioClip(audioIndex));
    }
}
