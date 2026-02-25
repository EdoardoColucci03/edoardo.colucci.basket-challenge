using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Mixer")]
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private AudioMixerGroup musicMixerGroup;
    [SerializeField] private AudioMixerGroup sfxMixerGroup;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Music")]
    [SerializeField] private AudioClip menuMusic;
    [SerializeField] private AudioClip gameplayMusic;
    [SerializeField] private float musicFadeDuration = 1f;

    [Header("UI SFX")]
    [SerializeField] private AudioClip buttonClickPrimary;
    [SerializeField] private AudioClip buttonClickSecondary;

    [Header("Result SFX")]
    [SerializeField] private AudioClip winSound;
    [SerializeField] private AudioClip loseSound;
    [SerializeField] private AudioClip drawSound;

    [Header("Stars SFX")]
    [SerializeField] private AudioClip starsLow;
    [SerializeField] private AudioClip starsMid;
    [SerializeField] private AudioClip starsLegendary;

    [Header("Ball SFX")]
    [SerializeField] private AudioClip ballLaunch;
    [SerializeField] private AudioClip ballBackboard;
    [SerializeField] private AudioClip ballRim;
    [SerializeField] private AudioClip ballNet;

    [Header("Score SFX")]
    [SerializeField] private AudioClip perfectShot;
    [SerializeField] private AudioClip bonusActive;
    [SerializeField] private AudioClip bonusBasket;

    [Header("Fireball SFX")]
    [SerializeField] private AudioClip fireballActive;
    [SerializeField] private AudioClip fireballLoop;
    [SerializeField][Range(0f, 1f)] private float fireballLoopVolume = 0.4f;

    [Header("Volume Settings")]
    [SerializeField][Range(0f, 1f)] public float musicVolume = 0.5f;
    [SerializeField][Range(0f, 1f)] public float sfxVolume = 1f;

    private const string MIXER_MUSIC_VOL = "MusicVolume";
    private const string MIXER_SFX_VOL = "SFXVolume";

    private Coroutine fadeMusicCoroutine;
    private AudioSource fireballLoopSource;
    private string currentScene;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadVolumes();
            SetupFireballLoopSource();
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        PlayMusicForScene(SceneManager.GetActiveScene().name);
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public void SaveVolumes()
    {
        PlayerPrefs.SetFloat("MusicVolume", musicVolume);
        PlayerPrefs.SetFloat("SFXVolume", sfxVolume);
        PlayerPrefs.Save();
    }

    public void LoadVolumes()
    {
        musicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.5f);
        sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);
        UpdateAllMixerVolumes();
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        UpdateMixerVolume(MIXER_MUSIC_VOL, musicVolume);
        SaveVolumes();
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        UpdateMixerVolume(MIXER_SFX_VOL, sfxVolume);
        SaveVolumes();
    }

    private void UpdateAllMixerVolumes()
    {
        UpdateMixerVolume(MIXER_MUSIC_VOL, musicVolume);
        UpdateMixerVolume(MIXER_SFX_VOL, sfxVolume);
    }

    private void UpdateMixerVolume(string parameter, float volume)
    {
        float db = volume > 0.0001f ? Mathf.Log10(volume) * 20f : -80f;
        audioMixer?.SetFloat(parameter, db);
    }

    private void SetupFireballLoopSource()
    {
        fireballLoopSource = gameObject.AddComponent<AudioSource>();
        fireballLoopSource.loop = true;
        fireballLoopSource.volume = fireballLoopVolume * sfxVolume;
        fireballLoopSource.playOnAwake = false;
        if (sfxMixerGroup != null) fireballLoopSource.outputAudioMixerGroup = sfxMixerGroup;
        if (fireballLoop != null) fireballLoopSource.clip = fireballLoop;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StopFireballLoop();
        StartCoroutine(PlayMusicNextFrame(scene.name));
    }

    private IEnumerator PlayMusicNextFrame(string sceneName)
    {
        yield return null;
        PlayMusicForScene(sceneName);
    }

    private void PlayMusicForScene(string sceneName)
    {
        if (sceneName == currentScene) return;
        currentScene = sceneName;
        AudioClip target = sceneName == "Gameplay" ? gameplayMusic : menuMusic;
        if (target == null) return;
        if (musicSource.clip == target && musicSource.isPlaying) return;
        if (fadeMusicCoroutine != null) StopCoroutine(fadeMusicCoroutine);
        fadeMusicCoroutine = StartCoroutine(FadeMusicRoutine(target));
    }

    private IEnumerator FadeMusicRoutine(AudioClip newClip)
    {
        float elapsed = 0f;
        while (elapsed < musicFadeDuration)
        {
            float t = elapsed / musicFadeDuration;
            UpdateMixerVolume(MIXER_MUSIC_VOL, Mathf.Lerp(musicVolume, 0f, t));
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        musicSource.Stop();
        musicSource.clip = newClip;
        musicSource.loop = true;
        musicSource.Play();
        elapsed = 0f;
        while (elapsed < musicFadeDuration)
        {
            float t = elapsed / musicFadeDuration;
            UpdateMixerVolume(MIXER_MUSIC_VOL, Mathf.Lerp(0f, musicVolume, t));
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        UpdateMixerVolume(MIXER_MUSIC_VOL, musicVolume);
        fadeMusicCoroutine = null;
    }

    public void PlayButtonClickPrimary() => PlaySFX(buttonClickPrimary);
    public void PlayButtonClickSecondary() => PlaySFX(buttonClickSecondary);
    public void PlayWin() => PlaySFX(winSound);
    public void PlayLose() => PlaySFX(loseSound);
    public void PlayDraw() => PlaySFX(drawSound);

    public void PlayStarsSound(int starCount)
    {
        if (starCount < 2) PlaySFX(starsLow);
        else if (starCount < 4) PlaySFX(starsMid);
        else PlaySFX(starsLegendary);
    }

    public void PlayBallLaunch() => PlaySFX(ballLaunch);
    public void PlayBallBackboard() => PlaySFX(ballBackboard);
    public void PlayBallRim() => PlaySFX(ballRim);
    public void PlayBallNet() => PlaySFX(ballNet);

    public void PlayPerfectShot()
    {
        PlaySFX(ballNet);
        PlaySFX(perfectShot);
    }

    public void PlayBonusActive() => PlaySFX(bonusActive);
    public void PlayBonusBasket() => PlaySFX(bonusBasket);

    public void PlayFireballActive()
    {
        PlaySFX(fireballActive);
        StartFireballLoop();
    }

    public void StartFireballLoop()
    {
        if (fireballLoopSource == null || fireballLoop == null) return;
        if (fireballLoopSource.isPlaying) return;
        fireballLoopSource.volume = fireballLoopVolume * sfxVolume;
        fireballLoopSource.Play();
    }

    public void StopFireballLoop()
    {
        if (fireballLoopSource != null && fireballLoopSource.isPlaying)
            fireballLoopSource.Stop();
    }

    private void PlaySFX(AudioClip clip)
    {
        if (clip == null || sfxSource == null) return;
        sfxSource.PlayOneShot(clip);
    }
}
