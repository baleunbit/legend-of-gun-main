// MenuMgr.cs
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuMgr : MonoBehaviour
{
    [SerializeField] string Scene_2_Game = "2_Game";

    // ⬇⬇ 컨트롤 패널 참조만 추가
    [SerializeField] GameObject ControlsPanel;

    void Awake()
    {
        // 안전하게 시작 시 비활성
        if (ControlsPanel) ControlsPanel.SetActive(false);
    }

    public void OnClickStart()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;
        SceneManager.LoadScene(Scene_2_Game);
    }

    // ⬇⬇ 버튼에 연결할 간단한 열기/닫기/토글
    public void ShowControls() { if (ControlsPanel) ControlsPanel.SetActive(true); }
    public void HideControls() { if (ControlsPanel) ControlsPanel.SetActive(false); }
    public void ToggleControls()
    {
        if (!ControlsPanel) return;
        ControlsPanel.SetActive(!ControlsPanel.activeSelf);
    }
}
