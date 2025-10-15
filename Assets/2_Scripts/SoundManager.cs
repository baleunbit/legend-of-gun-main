using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager I { get; private set; }

    [Header("BGM Clips")]
    public AudioClip bgmMenu;
    public AudioClip bgmIngame;
    public AudioClip bgmGameOver;

    [Header("Mixer/Settings")]
    [Range(0f, 1f)] public float bgmVolume = 0.8f;
    public bool dontDestroyOnLoad = true;

    AudioSource _src;

    void Awake()
    {
        if (I && I != this) { Destroy(gameObject); return; }
        I = this;
        _src = gameObject.AddComponent<AudioSource>();
        _src.loop = true;
        _src.playOnAwake = false;
        _src.volume = bgmVolume;
        if (dontDestroyOnLoad) DontDestroyOnLoad(gameObject);
    }

    public void PlayMenu()
    {
        Play(bgmMenu);
    }

    public void PlayIngame()
    {
        Play(bgmIngame);
    }

    public void PlayGameOver()
    {
        Play(bgmGameOver);
    }

    void Play(AudioClip clip)
    {
        if (!clip) return;
        if (_src.clip == clip && _src.isPlaying) return;
        _src.clip = clip;
        _src.volume = bgmVolume;
        _src.Play();
    }

    public void SetVolume(float v)
    {
        bgmVolume = Mathf.Clamp01(v);
        if (_src) _src.volume = bgmVolume;
    }
}
