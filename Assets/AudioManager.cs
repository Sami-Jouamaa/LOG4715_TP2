using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    public Sound[] sounds;
    public AudioMixerGroup mixerGroup;

    private AudioSource musicSource;
    private Coroutine fadeRoutine;
    private string currentTrack;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.playOnAwake = false;
        musicSource.outputAudioMixerGroup = mixerGroup;

        foreach (Sound s in sounds)
            if (!s.music) CreateSource(s);   // pas de source dédiée pour la musique
    }

    void Start()
    {
        if (Instance != this) return;  // doublon en cours de destruction
        PlayMusic("Theme");
    }

    void CreateSource(Sound s)
    {
        s.source = gameObject.AddComponent<AudioSource>();
        s.source.playOnAwake = false;
        s.source.clip = s.clip;
        s.source.volume = s.volume;
        s.source.pitch = s.pitch;
        s.source.loop = s.loop;
        s.source.outputAudioMixerGroup = mixerGroup;
    }

    Sound Find(string name) => Array.Find(sounds, x => x.name == name);

    public void Play(string name)
    {
        Sound s = Find(name);
        if (s == null) { Debug.LogWarning("Sound: " + name + " not found!"); return; }

        if (s.music) { PlayMusic(name); return; }   // un seul chemin pour la musique

        if (s.source == null) CreateSource(s);      // filet de sécurité

        if (s.loop) { if (!s.source.isPlaying) s.source.Play(); }
        else s.source.PlayOneShot(s.clip);
    }

    public void Stop(string name)
    {
        Sound s = Find(name);
        if (s != null && s.source != null) s.source.Stop();
    }

    public void PlayAt(string name, Vector3 position, float minDistance = 3f, float maxDistance = 20f)
    {
        if (Instance != this) { if (Instance != null) Instance.PlayAt(name, position, minDistance, maxDistance); return; }

        Sound s = Find(name);
        if (s == null) { Debug.LogWarning("Sound: " + name + " not found!"); return; }

        GameObject go = new GameObject("SFX_" + name);
        go.transform.position = position;

        AudioSource src = go.AddComponent<AudioSource>();
        src.clip = s.clip;
        src.volume = s.volume;
        src.pitch = s.pitch;
        src.loop = false;
        src.outputAudioMixerGroup = mixerGroup;        // keep the options slider working
        src.spatialBlend = 1f;                         // 1 = fully 3D
        src.rolloffMode = AudioRolloffMode.Linear;
        src.minDistance = minDistance;
        src.maxDistance = maxDistance;
        src.Play();

        Destroy(go, s.clip.length / Mathf.Max(0.01f, Mathf.Abs(s.pitch)) + 0.1f);
    }

    public void PlayMusic(string trackName, float fadeDuration = 0.5f)
    {
        Sound s = Find(trackName);
        if (s == null) { Debug.LogWarning("Music: " + trackName + " not found!"); return; }

        Debug.Log("PlayMusic: " + trackName);        // la trace montre qui l'appelle

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
            float p = 0f;
            while (p < 1f)
            {
                p += Time.unscaledDeltaTime / fadeDuration;
                musicSource.volume = Mathf.Lerp(startVolume, 0f, p);
                yield return null;
            }
        }

        musicSource.clip = next.clip;
        musicSource.pitch = next.pitch;
        musicSource.loop = next.loop;
        musicSource.volume = 0f;
        musicSource.Play();

        float q = 0f;
        while (q < 1f)
        {
            q += Time.unscaledDeltaTime / fadeDuration;
            musicSource.volume = Mathf.Lerp(0f, next.volume, q);
            yield return null;
        }
        musicSource.volume = next.volume;
    }
}