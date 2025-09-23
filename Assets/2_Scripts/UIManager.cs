using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("HUD")]
    public TextMeshProUGUI AmmoText;

    [Header("Died UI")]
    [SerializeField] private GameObject diedPanel;   // 👈 Died 패널(비활성 시작)
    [SerializeField] private Button restartButton;   // 👈 Restart 버튼(선택)

    void Awake()
    {
        if (Instance == null) Instance = this;
        else if (Instance != this) { Destroy(gameObject); return; }

        // 씬 시작 시 항상 숨김 보장
        if (diedPanel) diedPanel.SetActive(false);

        // Restart 버튼을 코드로도 연결 가능 (에디터에서 이미 연결했다면 생략 OK)
        if (restartButton)
        {
            restartButton.onClick.RemoveAllListeners();
            restartButton.onClick.AddListener(Restart);
        }
    }

    public void UpdateAmmoText(int current, int max)
    {
        // NOTE: current < 0일 때 "¡Ä"는 글꼴/인코딩 깨짐처럼 보입니다.
        // 무한 표시를 원했다면 "∞" 사용을 권장합니다(폰트가 지원해야 함).
        // 아니면 "—" 같은 대시나 "0"으로 처리하세요.
        if (AmmoText == null) return;

        if (current < 0)
            AmmoText.text = $"∞ / {max}";
        else
            AmmoText.text = $"{current} / {max}";
    }

    // ===== 사망 UI 제어 =====
    public void ShowDiedPanel()
    {
        if (diedPanel == null)
        {
            Debug.LogError("[UIManager] DiedPanel이 연결되지 않았습니다.");
            return;
        }

        diedPanel.SetActive(true);

        // 게임 정지 & 오디오 일시정지
        Time.timeScale = 0f;
        AudioListener.pause = true;

        // 마우스 커서 보이기(필요시)
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    // Restart 버튼용
    public void Restart()
    {
        // 반드시 원복 후 로드
        Time.timeScale = 1f;
        AudioListener.pause = false;

        var cur = SceneManager.GetActiveScene();
        SceneManager.LoadScene(cur.buildIndex);
    }
}
