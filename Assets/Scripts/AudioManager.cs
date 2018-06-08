using UnityEngine;

public class AudioManager : Singleton<AudioManager>
{
    public AudioClip music_bg;

    public AudioClip audio_click;
    public AudioClip audio_complete;
    public AudioClip audio_spawn;
    public AudioClip audio_spin;
    public AudioClip audio_explode;
    public AudioClip audio_collision;
    public AudioClip audio_snapCorrect;
    public AudioClip audio_snapIncorrect;
    public AudioClip audio_snapIdle;
    public AudioClip audio_snapComplete;
    public AudioClip audio_star;
    public AudioClip audio_win;
    public AudioClip audio_lose;

    private AudioSource[] audioSources;
    private int currentIndex;
    private const int START_INDEX = 3;

    private float fVolumeReductionCoeffient = .5f;

    void Start()
    {
        audioSources = GetComponentsInChildren<AudioSource>();

        // Preset audiosources
        audioSources[0].clip = music_bg;
        if (audioSources[0].playOnAwake)
            audioSources[0].Play();
        audioSources[1].clip = audio_collision;
        audioSources[2].clip = audio_spin;

        currentIndex = START_INDEX;
    }

    public void PlaySound(AudioClip _clip, float _delay = 0, float _volume = 1f, float _pitch = 1f, bool _loop = false)
    {
        // Find available audiosource
        while (audioSources[currentIndex].isPlaying)
        {
            currentIndex++;

            if (currentIndex == audioSources.Length)
                currentIndex = START_INDEX;
        }

        // Play audio with specified parameters
        audioSources[currentIndex].clip = _clip;
        audioSources[currentIndex].loop = _loop;
        audioSources[currentIndex].volume = _volume;
        audioSources[currentIndex].pitch = _pitch;
        audioSources[currentIndex].PlayDelayed(_delay);
    }
    
    public bool IsPlaying(AudioClip _clip)
    {
        for(int i=currentIndex; i<audioSources.Length; i++)
        {
            if(audioSources[i].clip == _clip)
            {
                if (audioSources[i].isPlaying)
                    return true;
            }
        }
        return false;
    }

    public bool IsPlayingMusic { get { return audioSources[0].isPlaying; } }

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

    public void PauseMusic()
    {
        audioSources[0].Pause();
    }

    public void UnpauseMusic()
    {
        audioSources[0].UnPause();
    } 

    public void PlayCollisionSound()
    {
        if (!audioSources[1].isPlaying)
            audioSources[1].Play();
    }

    public void PlaySpinSound()
    {
        StopSpinSound();
        audioSources[2].Play();
    }

    public void StopSpinSound()
    {
        if (audioSources[2].isPlaying)
            audioSources[2].Stop();
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
