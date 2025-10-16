// SoundManager.cs (기존 파일 대체)
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager I { get; private set; }

    [Header("공통 BGM")]
    public AudioClip bgmMenu;
    public AudioClip bgmGameOver;

    [Header("스테이지별 BGM (index = stage-1)")]
    public AudioClip[] stageBgms;   // 0: 1스테, 1: 2스테, 2: 3스테, ...

    [Range(0f, 1f)] public float bgmVolume = 0.8f;
    public bool dontDestroyOnLoad = true;

    AudioSource _src;

    void Awake()
    {
        if (I && I != this) { Destroy(gameObject); return; }
        I = this;
        _src = gameObject.AddComponent<AudioSource>();
        _src.loop = true; _src.playOnAwake = false; _src.volume = bgmVolume;
        if (dontDestroyOnLoad) DontDestroyOnLoad(gameObject);
    }

    public void PlayMenu() => Play(bgmMenu);
    public void PlayGameOver() => Play(bgmGameOver);

    public void PlayStageBgm(int stage)
    {
        if (stage <= 0) { Stop(); return; }
        int idx = stage - 1;
        if (stageBgms != null && idx < stageBgms.Length && stageBgms[idx])
            Play(stageBgms[idx]);
    }

    void Play(AudioClip clip)
    {
        if (!clip) return;
        if (_src.clip == clip && _src.isPlaying) return;
        _src.clip = clip; _src.volume = bgmVolume; _src.Play();
    }

    public void Stop() { _src.Stop(); _src.clip = null; }

    public void SetVolume(float v)
    { bgmVolume = Mathf.Clamp01(v); if (_src) _src.volume = bgmVolume; }
}
