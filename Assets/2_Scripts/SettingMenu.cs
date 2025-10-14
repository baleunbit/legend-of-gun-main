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

    // ���� Ű
    const string K_RES_W = "SET_RES_W";
    const string K_RES_H = "SET_RES_H";
    const string K_MODE = "SET_MODE";
    const string K_FPS = "SET_FPS";
    const string K_VOL_M = "SET_VOL_MASTER";
    const string K_VOL_B = "SET_VOL_MUSIC";
    const string K_VOL_U = "SET_VOL_UI";
    const string K_VOL_S = "SET_VOL_SFX";

    // ���� ����
    Resolution[] _resList;
    readonly int[] _fpsList = { 60, 90, 120, 144, 165, 240, -1 }; // -1 = ������

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
        // ������ �⺻: �׷���
        ShowPage("graphics");
    }

    // ---------- UI ä��� ----------

    void BuildResolutions()
    {
        // �ߺ� �ػ� ����(�������÷���Ʈ ����)
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
        // ���� ����: FullScreenWindow(�׵θ�����), ExclusiveFullScreen(��üȭ��), Windowed(â���)
        SetOptions(ddDisplayMode, new List<string> { "�׵θ� ����", "��ü ȭ��", "â ���" });
    }

    void BuildFps()
    {
        var labels = _fpsList.Select(v => v > 0 ? v.ToString() : "������").ToList();
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

    // ---------- ����/����/�ε� ----------

    public void OnApplyGraphics()
    {
        // �ػ�
        int ridx = GetIndex(ddResolution);
        if (ridx < 0 || ridx >= _resList.Length) ridx = _resList.Length - 1;
        var res = _resList[ridx];

        // ȭ�� ���
        int midx = GetIndex(ddDisplayMode);
        var mode = FullScreenMode.FullScreenWindow; // �׵θ�����
        if (midx == 1) mode = FullScreenMode.ExclusiveFullScreen; // ��üȭ��
        else if (midx == 2) mode = FullScreenMode.Windowed;       // â ���

        Screen.SetResolution(res.width, res.height, mode);

        // FPS (�������� -1)
        int fidx = GetIndex(ddTargetFps);
        int fps = _fpsList[Mathf.Clamp(fidx, 0, _fpsList.Length - 1)];
        QualitySettings.vSyncCount = 0;                     // ��������� vsync ����
        Application.targetFrameRate = fps < 0 ? -1 : fps;   // ����

        // ����
        PlayerPrefs.SetInt(K_RES_W, res.width);
        PlayerPrefs.SetInt(K_RES_H, res.height);
        PlayerPrefs.SetInt(K_MODE, (int)mode);
        PlayerPrefs.SetInt(K_FPS, fps);
        PlayerPrefs.Save();
    }

    public void OnApplyAudio()
    {
        // �����̴� ������ [0..1] ����
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
        // �׷��� �ε�
        int w = PlayerPrefs.GetInt(K_RES_W, Screen.currentResolution.width);
        int h = PlayerPrefs.GetInt(K_RES_H, Screen.currentResolution.height);
        int m = PlayerPrefs.GetInt(K_MODE, (int)Screen.fullScreenMode);
        int fps = PlayerPrefs.GetInt(K_FPS, Application.targetFrameRate <= 0 ? -1 : Application.targetFrameRate);

        // ��Ӵٿ� ���ð� ����ȭ
        int ridx = Array.FindIndex(_resList, r => r.width == w && r.height == h);
        if (ridx < 0) ridx = _resList.Length - 1;
        SetIndex(ddResolution, ridx);

        int midx = 0; // �׵θ�����
        if ((FullScreenMode)m == FullScreenMode.ExclusiveFullScreen) midx = 1;
        else if ((FullScreenMode)m == FullScreenMode.Windowed) midx = 2;
        SetIndex(ddDisplayMode, midx);

        int fidx = Array.IndexOf(_fpsList, fps);
        if (fidx < 0) fidx = _fpsList.Length - 1;
        SetIndex(ddTargetFps, fidx);

        if (applyGraphics) OnApplyGraphics();

        // ����� �ε�
        float vM = PlayerPrefs.GetFloat(K_VOL_M, 1f);
        float vB = PlayerPrefs.GetFloat(K_VOL_B, 1f);
        float vU = PlayerPrefs.GetFloat(K_VOL_U, 1f);
        float vS = PlayerPrefs.GetFloat(K_VOL_S, 1f);

        if (slMaster) slMaster.value = vM;
        if (slMusic) slMusic.value = vB;
        if (slUI) slUI.value = vU;
        if (slSFX) slSFX.value = vS;

        if (applyAudio) OnApplyAudio();

        // UI ������ ǥ�� ����
        if (applyUi) ShowPage("graphics");
    }

    // ����(0..1) -> dB ���� (0�� -80dB�� ��Ʈ)
    void SetMixerLinear(AudioMixer mix, string param, float linear)
    {
        if (!mix || string.IsNullOrEmpty(param)) return;
        float dB = (linear <= 0.0001f) ? -80f : Mathf.Lerp(-30f, 0f, Mathf.Clamp01(linear)); // ������ ���� ����
        mix.SetFloat(param, dB);
    }

    // ---------- ��/������ ----------

    public void ShowPage(string key)
    {
        key = key.ToLowerInvariant();
        if (pageGraphics) pageGraphics.SetActive(key.Contains("g"));
        if (pageAudio) pageAudio.SetActive(key.Contains("a"));
        if (pageSystem) pageSystem.SetActive(key.Contains("s"));
    }

    // ---------- ��ƿ ----------

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
