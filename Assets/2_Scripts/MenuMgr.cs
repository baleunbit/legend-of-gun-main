using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuMgr : MonoBehaviour
{
    [SerializeField] string Scene_2_Game = "2_Game";  // �� ���� �̸� ����
    public void OnClickStart()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;
        SceneManager.LoadScene(Scene_2_Game);
    }
}
