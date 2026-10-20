using UnityEngine;

public class AudioManager
{
    private readonly AudioContainer audioContainer;
    private readonly float[] nextSfxPlayTimes;
    private AudioSource loopingSfxSource;
    private AudioSource sfxSource;

    public AudioManager(AudioContainer container)
    {
        audioContainer = container;
        nextSfxPlayTimes = new float[audioContainer.GetSfxCount()];
    }

    public void Init(AudioSource loopingSource, AudioSource oneShotSource)
    {
        loopingSfxSource = loopingSource;
        sfxSource = oneShotSource;

        loopingSfxSource.playOnAwake = false;
        loopingSfxSource.spatialBlend = 0f;
        sfxSource.playOnAwake = false;
        sfxSource.spatialBlend = 0f;
    }

    public void PlaySFX(int audioIndex)
    {
        if (Time.unscaledTime < nextSfxPlayTimes[audioIndex])
        {
            return;
        }

        SfxData sfxData = audioContainer.GetSfx(audioIndex);
        sfxSource.PlayOneShot(sfxData.Clip);
        nextSfxPlayTimes[audioIndex] = Time.unscaledTime + sfxData.Cooldown;
    }

    public void PlayLoopingSFX(int audioIndex)
    {
        AudioClip audioClip = audioContainer.GetLoopingSfx(audioIndex);
        audioClip.LoadAudioData();

        loopingSfxSource.Stop();
        loopingSfxSource.clip = audioClip;
        loopingSfxSource.loop = true;
        loopingSfxSource.Play();
    }

    public void StopLoopingSFX()
    {
        loopingSfxSource.Stop();
        loopingSfxSource.loop = false;
        loopingSfxSource.clip = null;
    }
}
