using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [Header("�Ͻ����� �г�(��Ȱ������ ����)")]
    [SerializeField] GameObject menuRoot;

    [Header("�ɼ�")]
    [SerializeField] bool showCursorOnPause = true;

    bool paused;

    void Awake()
    {
        if (menuRoot) menuRoot.SetActive(false);
        paused = false;
        Time.timeScale = 1f;
        AudioListener.pause = false;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (paused) Resume();
            else Pause();
        }
    }

    public void Pause()
    {
        paused = true;
        Time.timeScale = 0f;
        AudioListener.pause = true;
        if (menuRoot) menuRoot.SetActive(true);
        if (showCursorOnPause) Cursor.visible = true;
    }

    public void Resume()
    {
        paused = false;
        Time.timeScale = 1f;
        AudioListener.pause = false;
        if (menuRoot) menuRoot.SetActive(false);
        // �ʿ��ϸ� Ŀ�� ����:
        // Cursor.visible = false;
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;
        var cur = SceneManager.GetActiveScene();
        SceneManager.LoadScene(cur.buildIndex);
    }

    public void BackToMenu(string menuSceneName = "1_Menu")
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;
        SceneManager.LoadScene(menuSceneName);
    }

    public bool IsPaused() => paused;
}
