﻿using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneMgr : MonoBehaviour
{
    public static SceneMgr I { get; private set; }

    [Header("씬 이름들")]
    [SerializeField] string sceneMenu = "1_Menu";
    [SerializeField] string sceneGame = "2_Game";
    [SerializeField] string sceneEnd = "3_End";

    [Header("UI (선택사항)")]
    [SerializeField] GameObject ControlsPanel;

    void Start()
    {
        // 메뉴 씬에서 처음 뜰 때 바로 메뉴 BGM 재생
        SoundManager.I?.PlayMenu();
    }

    void Awake()
    {
        if (I && I != this) { Destroy(gameObject); return; }
        I = this;
        DontDestroyOnLoad(gameObject);

        if (ControlsPanel) ControlsPanel.SetActive(false);

        Time.timeScale = 1f;
        AudioListener.pause = false;
    }

    // 🔸 메인 메뉴에서 호출되는 버튼
    public void OnClickStart()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;

        if (SceneFader.I)
            SceneFader.I.LoadSceneWithFade(sceneGame);
        else
            SceneManager.LoadScene(sceneGame);

        SoundManager.I?.PlayStageBgm(1);
    }

    public void OnClickExit()
    {
        Debug.Log("[SceneMgr] Exit requested");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void ShowControls() { if (ControlsPanel) ControlsPanel.SetActive(true); }
    public void HideControls() { if (ControlsPanel) ControlsPanel.SetActive(false); }
    public void ToggleControls()
    {
        if (ControlsPanel)
            ControlsPanel.SetActive(!ControlsPanel.activeSelf);
    }

    // 🔸 인게임 → 엔드씬 전환 (몹 전멸 시 호출)
    public void GoToEndScene()
    {
        Time.timeScale = 1f;
        SoundManager.I?.PlayMenu(); // 엔딩 BGM 없으면 메뉴용으로 재생

        if (SceneFader.I)
            SceneFader.I.LoadSceneWithFade(sceneEnd);
        else
            SceneManager.LoadScene(sceneEnd);
    }

    // 🔸 엔드씬 버튼용
    public void OnClickRestart()
    {
        Time.timeScale = 1f;
        SoundManager.I?.PlayStageBgm(1);
        if (SceneFader.I)
            SceneFader.I.LoadSceneWithFade(sceneGame);
        else
            SceneManager.LoadScene(sceneGame);
    }

    public void OnClickMenu()
    {
        Time.timeScale = 1f;
        SoundManager.I?.PlayMenu();
        if (SceneFader.I)
            SceneFader.I.LoadSceneWithFade(sceneMenu);
        else
            SceneManager.LoadScene(sceneMenu);
    }
}
