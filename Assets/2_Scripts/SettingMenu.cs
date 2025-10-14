using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
#if TMP_PRESENT || UNITY_TEXTMESHPRO
using TMPro;
#endif

public class SettingsMenu : MonoBehaviour
{
    [Header("Roots/Pages")]
    [SerializeField] GameObject settingsRoot;
    [SerializeField] GameObject pageGraphics;
    [SerializeField] GameObject pageAudio;
    [SerializeField] GameObject pageSystem;

    [Header("Graphics UI")]
#if TMP_PRESENT || UNITY_TEXTMESHPRO
    [SerializeField] TMP_Dropdown ddResolution;
    [SerializeField] TMP_Dropdown ddDisplayMode;
    [SerializeField] TMP_Dropdown ddTargetFps;
#else
    [SerializeField] Dropdown ddResolution;
    [SerializeField] Dropdown ddDisplayMode;
    [SerializeField] Dropdown ddTargetFps;
#endif

    [Header("Audio UI")]
    [SerializeField] Slider slMaster;
    [SerializeField] Slider slMusic;
    [SerializeField] Slider slUI;
    [SerializeField] Slider slSFX;

    [Header("Audio Mixer (optional but recommended)")]
    [SerializeField] AudioMixer mixer; // Master/Music/UI/SFX
    [SerializeField] string pMaster = "MasterVol";
    [SerializeField] string pMusic = "MusicVol";
    [SerializeField] string pUI = "UIVol";
    [SerializeField] string pSFX = "SFXVol";

    // 저장 키
    const string K_RES_W = "SET_RES_W";
    const string K_RES_H = "SET_RES_H";
    const string K_MODE = "SET_MODE";
    const string K_FPS = "SET_FPS";
    const string K_VOL_M = "SET_VOL_MASTER";
    const string K_VOL_B = "SET_VOL_MUSIC";
    const string K_VOL_U = "SET_VOL_UI";
    const string K_VOL_S = "SET_VOL_SFX";

    // 내부 상태
    Resolution[] _resList;
    readonly int[] _fpsList = { 60, 90, 120, 144, 165, 240, -1 }; // -1 = 무제한

    void Awake()
    {
        if (!settingsRoot) settingsRoot = gameObject;
        BuildResolutions();
        BuildDisplayModes();
        BuildFps();
        LoadAndApplyAll(applyGraphics: true, applyAudio: true, applyUi: true);
    }

    void OnEnable()
    {
        // 페이지 기본: 그래픽
        ShowPage("graphics");
    }

    // ---------- UI 채우기 ----------

    void BuildResolutions()
    {
        // 중복 해상도 제거(리프레시레이트 무시)
        _resList = Screen.resolutions
            .Select(r => new Resolution { width = r.width, height = r.height })
            .Distinct(new ResComparer())
            .OrderBy(r => r.width * r.height)
            .ToArray();

        var labels = _resList.Select(r => $"{r.width}x{r.height}").ToList();
        SetOptions(ddResolution, labels);
    }

    void BuildDisplayModes()
    {
        // 순서 유지: FullScreenWindow(테두리없음), ExclusiveFullScreen(전체화면), Windowed(창모드)
        SetOptions(ddDisplayMode, new List<string> { "테두리 없음", "전체 화면", "창 모드" });
    }

    void BuildFps()
    {
        var labels = _fpsList.Select(v => v > 0 ? v.ToString() : "무제한").ToList();
        SetOptions(ddTargetFps, labels);
    }

    void SetOptions(object dropdown, List<string> labels)
    {
#if TMP_PRESENT || UNITY_TEXTMESHPRO
        var dd = dropdown as TMP_Dropdown;
        dd.ClearOptions();
        dd.AddOptions(labels);
#else
        var dd = dropdown as Dropdown;
        dd.ClearOptions();
        dd.AddOptions(labels);
#endif
    }

    // ---------- 적용/저장/로드 ----------

    public void OnApplyGraphics()
    {
        // 해상도
        int ridx = GetIndex(ddResolution);
        if (ridx < 0 || ridx >= _resList.Length) ridx = _resList.Length - 1;
        var res = _resList[ridx];

        // 화면 모드
        int midx = GetIndex(ddDisplayMode);
        var mode = FullScreenMode.FullScreenWindow; // 테두리없음
        if (midx == 1) mode = FullScreenMode.ExclusiveFullScreen; // 전체화면
        else if (midx == 2) mode = FullScreenMode.Windowed;       // 창 모드

        Screen.SetResolution(res.width, res.height, mode);

        // FPS (무제한은 -1)
        int fidx = GetIndex(ddTargetFps);
        int fps = _fpsList[Mathf.Clamp(fidx, 0, _fpsList.Length - 1)];
        QualitySettings.vSyncCount = 0;                     // 명시적으로 vsync 끄고
        Application.targetFrameRate = fps < 0 ? -1 : fps;   // 설정

        // 저장
        PlayerPrefs.SetInt(K_RES_W, res.width);
        PlayerPrefs.SetInt(K_RES_H, res.height);
        PlayerPrefs.SetInt(K_MODE, (int)mode);
        PlayerPrefs.SetInt(K_FPS, fps);
        PlayerPrefs.Save();
    }

    public void OnApplyAudio()
    {
        // 슬라이더 범위는 [0..1] 권장
        SetMixerLinear(mixer, pMaster, slMaster ? slMaster.value : 1f);
        SetMixerLinear(mixer, pMusic, slMusic ? slMusic.value : 1f);
        SetMixerLinear(mixer, pUI, slUI ? slUI.value : 1f);
        SetMixerLinear(mixer, pSFX, slSFX ? slSFX.value : 1f);

        PlayerPrefs.SetFloat(K_VOL_M, slMaster ? slMaster.value : 1f);
        PlayerPrefs.SetFloat(K_VOL_B, slMusic ? slMusic.value : 1f);
        PlayerPrefs.SetFloat(K_VOL_U, slUI ? slUI.value : 1f);
        PlayerPrefs.SetFloat(K_VOL_S, slSFX ? slSFX.value : 1f);
        PlayerPrefs.Save();
    }

    public void LoadAndApplyAll(bool applyGraphics, bool applyAudio, bool applyUi)
    {
        // 그래픽 로드
        int w = PlayerPrefs.GetInt(K_RES_W, Screen.currentResolution.width);
        int h = PlayerPrefs.GetInt(K_RES_H, Screen.currentResolution.height);
        int m = PlayerPrefs.GetInt(K_MODE, (int)Screen.fullScreenMode);
        int fps = PlayerPrefs.GetInt(K_FPS, Application.targetFrameRate <= 0 ? -1 : Application.targetFrameRate);

        // 드롭다운 선택값 동기화
        int ridx = Array.FindIndex(_resList, r => r.width == w && r.height == h);
        if (ridx < 0) ridx = _resList.Length - 1;
        SetIndex(ddResolution, ridx);

        int midx = 0; // 테두리없음
        if ((FullScreenMode)m == FullScreenMode.ExclusiveFullScreen) midx = 1;
        else if ((FullScreenMode)m == FullScreenMode.Windowed) midx = 2;
        SetIndex(ddDisplayMode, midx);

        int fidx = Array.IndexOf(_fpsList, fps);
        if (fidx < 0) fidx = _fpsList.Length - 1;
        SetIndex(ddTargetFps, fidx);

        if (applyGraphics) OnApplyGraphics();

        // 오디오 로드
        float vM = PlayerPrefs.GetFloat(K_VOL_M, 1f);
        float vB = PlayerPrefs.GetFloat(K_VOL_B, 1f);
        float vU = PlayerPrefs.GetFloat(K_VOL_U, 1f);
        float vS = PlayerPrefs.GetFloat(K_VOL_S, 1f);

        if (slMaster) slMaster.value = vM;
        if (slMusic) slMusic.value = vB;
        if (slUI) slUI.value = vU;
        if (slSFX) slSFX.value = vS;

        if (applyAudio) OnApplyAudio();

        // UI 페이지 표시 갱신
        if (applyUi) ShowPage("graphics");
    }

    // 선형(0..1) -> dB 맵핑 (0은 -80dB로 뮤트)
    void SetMixerLinear(AudioMixer mix, string param, float linear)
    {
        if (!mix || string.IsNullOrEmpty(param)) return;
        float dB = (linear <= 0.0001f) ? -80f : Mathf.Lerp(-30f, 0f, Mathf.Clamp01(linear)); // 취향대로 범위 조절
        mix.SetFloat(param, dB);
    }

    // ---------- 탭/페이지 ----------

    public void ShowPage(string key)
    {
        key = key.ToLowerInvariant();
        if (pageGraphics) pageGraphics.SetActive(key.Contains("g"));
        if (pageAudio) pageAudio.SetActive(key.Contains("a"));
        if (pageSystem) pageSystem.SetActive(key.Contains("s"));
    }

    // ---------- 유틸 ----------

#if TMP_PRESENT || UNITY_TEXTMESHPRO
    int GetIndex(TMP_Dropdown dd) => dd ? dd.value : 0;
    void SetIndex(TMP_Dropdown dd, int i) { if (dd) dd.SetValueWithoutNotify(Mathf.Clamp(i, 0, dd.options.Count - 1)); }
#else
    int GetIndex(Dropdown dd) => dd ? dd.value : 0;
    void SetIndex(Dropdown dd, int i) { if (dd) dd.value = Mathf.Clamp(i, 0, dd.options.Count - 1); dd.RefreshShownValue(); }
#endif

    class ResComparer : IEqualityComparer<Resolution>
    {
        public bool Equals(Resolution a, Resolution b) => a.width == b.width && a.height == b.height;
        public int GetHashCode(Resolution r) => (r.width * 73856093) ^ (r.height * 19349663);
    }
}
