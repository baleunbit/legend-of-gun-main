using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject characterSelectPanel;

    private void Start()
    {
        mainMenuPanel.SetActive(true);
        characterSelectPanel.SetActive(false);
    }

    // ���� ��ư�� ����
    public void OnStartButton()
    {
        mainMenuPanel.SetActive(false);
        characterSelectPanel.SetActive(true);
    }

    // ĳ���� ���� ��ư�� ���� (��: ��ĳ�� "0", ��ĳ�� "1")
    public void OnCharacterSelect(int characterId)
    {
        // ������ ĳ���� ������ GameManager � ����

        // ���� ������ �̵� (��: "GameScene" �̸��� ��)
        SceneManager.LoadScene("GameScene");
    }
}
