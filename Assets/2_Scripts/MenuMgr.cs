// MenuMgr.cs
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuMgr : MonoBehaviour
{
    [Header("Scene Names")]
    [SerializeField] string Scene_2_Game = "2_Game";

    [Header("Panels (optional)")]
    [SerializeField] GameObject ControlsPanel;

    void Awake()
    {
        // 시작 시 안전하게 숨김
        if (ControlsPanel) ControlsPanel.SetActive(false);
        Time.timeScale = 1f;
        AudioListener.pause = false;
    }

    // === Buttons ===
    public void OnClickStart()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;
        SceneManager.LoadScene(Scene_2_Game);
    }

    public void OnClickExit()
    {
        Debug.Log("[MenuMgr] Exit requested");
        Application.Quit();

#if UNITY_EDITOR
        // 에디터에선 Play 모드 종료
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    // === Controls panel ===
    public void ShowControls() { if (ControlsPanel) ControlsPanel.SetActive(true); }
    public void HideControls() { if (ControlsPanel) ControlsPanel.SetActive(false); }
    public void ToggleControls()
    {
        if (!ControlsPanel) return;
        ControlsPanel.SetActive(!ControlsPanel.activeSelf);
    }
}
