using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Mixer - MainMixer con Master, Music y SFX")]
    [SerializeField] private AudioMixer audioMixer;
    private const string MASTER_PARAM = "MasterVolume";
    private const string MUSIC_PARAM = "MusicVolume";
    private const string SFX_PARAM = "SFXVolume";

    [Header("Sources - 2 AudioSource en el mismo GameObject")]
    [SerializeField] private AudioSource musicSource; // Loop = true, Output = Music
    [SerializeField] private AudioSource sfxSource;   // Loop = false, Output = SFX

    [Header("Musica - 1 clip por estado")]
    [SerializeField] private AudioClip menuMusic;
    [SerializeField] private AudioClip gameplayMusic;
    [SerializeField] private AudioClip victoryMusic;
    [SerializeField] private AudioClip gameOverMusic;
    [SerializeField] private AudioClip shopMusic;

    [Header("SFX - Genericos")]
    [SerializeField] private AudioClip jumpClip;
    [SerializeField] private AudioClip shootClip;
    [SerializeField] private AudioClip reloadClip;
    [SerializeField] private AudioClip pickupClip;
    [SerializeField] private AudioClip clickClip;
    [SerializeField] private AudioClip hurtClip;

    [Header("SFX - Pasos en asfalto")]
    [SerializeField] private AudioClip[] footstepClips;

    [Header("SFX - Zombis")]
    [SerializeField] private AudioClip[] zombieAttackClips;
    [SerializeField] private AudioClip[] zombieHurtClips;
    [SerializeField] private AudioClip[] zombieDeathClips;

    private GameState musicaActual = (GameState)(-1);
    private bool suscrito = false;
    private bool enTienda = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            if (musicSource != null) musicSource.enabled = true;
            if (sfxSource != null) sfxSource.enabled = true;
            CargarVolumenes();
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void OnEnable()
    {
        Suscribirse();
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    void OnDisable()
    {
        if (GameManager.Instance != null && suscrito)
        {
            GameManager.Instance.OnChangeState -= CambiarMusicaSegunEstado;
            suscrito = false;
        }
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }

    void Start()
    {
        Suscribirse();
        if (GameManager.Instance != null)
            CambiarMusicaSegunEstado(GameManager.Instance.currentState);
    }

    private void Suscribirse()
    {
        if (!suscrito && GameManager.Instance != null)
        {
            GameManager.Instance.OnChangeState += CambiarMusicaSegunEstado;
            suscrito = true;
        }
    }

    // --- TIENDA ---
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Tienda")
        {
            ReproducirMusicaTienda();
        }
    }

    private void OnSceneUnloaded(Scene scene)
    {
        // Cuando se descarga Tienda volvemos a la musica de Level1
        if (scene.name == "Tienda")
        {
            enTienda = false;
            ReproducirMusicaGameplay();
        }
    }

    private void ReproducirMusicaTienda()
    {
        if (musicSource == null || shopMusic == null) return;
        enTienda = true;
        if (musicSource.clip == shopMusic && musicSource.isPlaying) return;
        musicSource.Stop();
        musicaActual = GameState.Playing;
        musicSource.clip = shopMusic;
        musicSource.loop = true;
        musicSource.Play();
    }

    private void ReproducirMusicaGameplay()
    {
        if (musicSource == null || gameplayMusic == null) return;
        musicSource.Stop();
        musicaActual = GameState.Playing;
        musicSource.clip = gameplayMusic;
        musicSource.loop = true;
        musicSource.Play();
    }

    private void CambiarMusicaSegunEstado(GameState estado)
    {
        if (musicSource == null) return;
        if (enTienda) return;

        if (estado == GameState.Paused)
        {
            if (musicSource.isPlaying) musicSource.Pause();
            musicaActual = GameState.Paused;
            return;
        }

        if (estado == GameState.Playing && musicaActual == GameState.Paused)
        {
            musicSource.UnPause();
            musicaActual = GameState.Playing;
            return;
        }

        AudioClip clip = estado switch
        {
            GameState.Start => menuMusic,
            GameState.Playing => gameplayMusic,
            GameState.Victory => victoryMusic,
            GameState.GameOver => gameOverMusic,
            _ => null
        };

        if (clip == null) return;
        if (clip == musicSource.clip && musicSource.isPlaying && estado == musicaActual) return;

        if (estado == GameState.GameOver) StopSFX();

        musicaActual = estado;
        musicSource.clip = clip;
        musicSource.loop = true;
        musicSource.Play();
    }

    // --- SFX PUBLICOS ---
    public void PlayJump() => PlaySFX(jumpClip);
    public void PlayShoot() => PlaySFX(shootClip);
    public void PlayReload() => PlaySFX(reloadClip);
    public void PlayPickup() => PlaySFX(pickupClip);
    public void PlayClick() => PlaySFX(clickClip);
    public void PlayHurt() => PlaySFX(hurtClip);

    public void PlayFootstep()
    {
        if (footstepClips == null || footstepClips.Length == 0) return;
        AudioClip clip = footstepClips[Random.Range(0, footstepClips.Length)];
        PlaySFXRandomPitch(clip);
    }

    public void PlayZombieAttack(Transform origen) => PlaySFX(GetRandom(zombieAttackClips));
    public void PlayZombieHurt(Transform origen) => PlaySFX(GetRandom(zombieHurtClips));

    public void PlayZombieDeath(Transform origen)
    {
        AudioClip clip = GetRandom(zombieDeathClips);
        if (clip == null) return;
        Vector3 pos = origen != null ? origen.position : transform.position;
        GameObject temp = new GameObject("ZombieDeathSound");
        temp.transform.position = pos;
        AudioSource src = temp.AddComponent<AudioSource>();
        src.outputAudioMixerGroup = sfxSource != null ? sfxSource.outputAudioMixerGroup : null;
        src.clip = clip;
        src.spatialBlend = 0f;
        src.Play();
        Destroy(temp, clip.length + 0.1f);
    }

    private AudioClip GetRandom(AudioClip[] lista)
    {
        if (lista == null || lista.Length == 0) return null;
        return lista[Random.Range(0, lista.Length)];
    }

    public void PlaySFX(AudioClip clip)
    {
        if (clip == null || sfxSource == null) return;
        if (!sfxSource.enabled) sfxSource.enabled = true;
        if (!sfxSource.gameObject.activeInHierarchy) return;
        sfxSource.PlayOneShot(clip);
    }

    private void PlaySFXRandomPitch(AudioClip clip)
    {
        if (clip == null || sfxSource == null) return;
        if (!sfxSource.enabled) sfxSource.enabled = true;
        float pitchOriginal = sfxSource.pitch;
        sfxSource.pitch = Random.Range(0.9f, 1.1f);
        sfxSource.PlayOneShot(clip);
        sfxSource.pitch = pitchOriginal;
    }

    public void StopSFX()
    {
        if (sfxSource != null) sfxSource.Stop();
    }

    // --- VOLUMEN: Slider 0 a 1 -> dB ---
    public void SetMasterVolume(float valor) => SetVolumen(MASTER_PARAM, "MasterVolume", valor);
    public void SetMusicVolume(float valor) => SetVolumen(MUSIC_PARAM, "MusicVolume", valor);
    public void SetSFXVolume(float valor) => SetVolumen(SFX_PARAM, "SFXVolume", valor);

    private void SetVolumen(string parametro, string keyPlayerPrefs, float valor)
    {
        if (audioMixer == null) return;
        float db = valor <= 0.0001f ? -80f : Mathf.Log10(valor) * 20f;
        audioMixer.SetFloat(parametro, db);
        PlayerPrefs.SetFloat(keyPlayerPrefs, valor);
        PlayerPrefs.Save();
    }

    private void CargarVolumenes()
    {
        float master = PlayerPrefs.GetFloat("MasterVolume", 0.5f);
        float music = PlayerPrefs.GetFloat("MusicVolume", 0.3f);
        float sfx = PlayerPrefs.GetFloat("SFXVolume", 0.5f);
        SetVolumen(MASTER_PARAM, "MasterVolume", master);
        SetVolumen(MUSIC_PARAM, "MusicVolume", music);
        SetVolumen(SFX_PARAM, "SFXVolume", sfx);
    }

    public float GetMasterVolume() => PlayerPrefs.GetFloat("MasterVolume", 0.5f);
    public float GetMusicVolume() => PlayerPrefs.GetFloat("MusicVolume", 0.3f);
    public float GetSFXVolume() => PlayerPrefs.GetFloat("SFXVolume", 0.5f);
}