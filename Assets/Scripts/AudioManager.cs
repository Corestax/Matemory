using UnityEngine;

public class AudioManager : Singleton<AudioManager>
{
    public AudioClip music_bg;

    public AudioClip audio_click;
    public AudioClip audio_clickSuccess;
    public AudioClip audio_spawn;
    public AudioClip audio_star;
    public AudioClip audio_win;
    public AudioClip audio_lose;

    private AudioSource[] audioSources;
    private int currentIndex;

    private float fVolumeReductionCoeffient = .5f;

    void Start()
    {
        audioSources = GetComponentsInChildren<AudioSource>();
        currentIndex = 2;
    }

    public void PlaySound(AudioClip _clip, float _delay = 0, float _volume = 1f, float _pitch = 1f, bool _loop = false)
    {
        // Find available audiosource
        while (audioSources[currentIndex].isPlaying)
        {
            currentIndex++;

            if (currentIndex == audioSources.Length)
                currentIndex = 2;
        }

        // Play audio with specified parameters
        audioSources[currentIndex].clip = _clip;
        audioSources[currentIndex].loop = _loop;
        audioSources[currentIndex].volume = _volume;
        audioSources[currentIndex].pitch = _pitch;
        audioSources[currentIndex].PlayDelayed(_delay);
    }

    public void PlayMusic(AudioClip _clip, float _delay = 0)
    {
        audioSources[0].clip = _clip;
        audioSources[0].PlayDelayed(_delay);
    }

    public void PlayMusic()
    {
        audioSources[0].Play();
    }

    public void StopMusic()
    {
        audioSources[0].Stop();
    }

    public void SetMusicVolume(float _volume)
    {
        audioSources[0].volume = _volume * fVolumeReductionCoeffient;
    }

    public float GetMusicVolume()
    {
        return audioSources[0].volume / fVolumeReductionCoeffient;
    }

    public AudioClip GetPlayingMusic()
    {
        return audioSources[0].clip;
    }
}
