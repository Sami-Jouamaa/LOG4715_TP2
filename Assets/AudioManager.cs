using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    public Sound[] sounds;

    private AudioSource musicSource;   // single shared source for music crossfades
    private Coroutine fadeRoutine;
    private string currentTrack;

    public AudioMixerGroup mixerGroup; 

    void Awake()
    {
        // Singleton: keep the first AudioManager, destroy any duplicate (e.g. when returning to the menu scene)
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // One AudioSource per sound, for SFX via Play()
        foreach (Sound s in sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;
            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;
            s.source.outputAudioMixerGroup = mixerGroup;
        }

        // Dedicated source for music
        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.playOnAwake = false;
        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.playOnAwake = false;
        musicSource.outputAudioMixerGroup = mixerGroup;
        
    }

    void Start()
    {
        if (Instance != this) return;
        PlayMusic("Theme");
    }

    public void Play(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning("Sound: " + name + " not found!");
            return;
        }
        s.source.Play();
    }

    // --- Music with crossfade ---
    public void PlayMusic(string trackName, float fadeDuration = 0.5f)
    {
        Sound s = Array.Find(sounds, sound => sound.name == trackName);
        if (s == null)
        {
            Debug.LogWarning("Music: " + trackName + " not found!");
            return;
        }

        if (currentTrack == trackName && musicSource.isPlaying) return;
        currentTrack = trackName;

        if (fadeRoutine != null) StopCoroutine(fadeRoutine);
        fadeRoutine = StartCoroutine(AnimateMusicCrossfade(s, fadeDuration));
    }

    IEnumerator AnimateMusicCrossfade(Sound next, float fadeDuration)
    {
        if (musicSource.isPlaying)
        {
            float startVolume = musicSource.volume;
            float percent = 0f;
            while (percent < 1f)
            {
                percent += Time.unscaledDeltaTime / fadeDuration;
                musicSource.volume = Mathf.Lerp(startVolume, 0f, percent);
                yield return null;
            }
        }

        musicSource.clip = next.clip;
        musicSource.pitch = next.pitch;
        musicSource.loop = next.loop;
        musicSource.volume = 0f;
        musicSource.Play();

        float p = 0f;
        while (p < 1f)
        {
            p += Time.unscaledDeltaTime / fadeDuration;
            musicSource.volume = Mathf.Lerp(0f, next.volume, p);
            yield return null;
        }
        musicSource.volume = next.volume;
    }
}