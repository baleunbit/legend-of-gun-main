using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("HUD")]
    public TextMeshProUGUI AmmoText;

    [Header("Reload UI")]
    [SerializeField] private GameObject reloadCircle;   // ← Canvas의 ReloadCircle 연결

    [Header("Died UI")]
    [SerializeField] private GameObject diedPanel;
    [SerializeField] private Button restartButton;

    private Gun currentGun;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else if (Instance != this) { Destroy(gameObject); return; }

        if (diedPanel) diedPanel.SetActive(false);
        if (restartButton)
        {
            restartButton.onClick.RemoveAllListeners();
            restartButton.onClick.AddListener(Restart);
        }

        // ✅ 씬 시작 시 리로드 서클은 꺼두기
        HideReloadCircle();
    }

    public void UpdateAmmoText(int current, int max)
    {
        if (!AmmoText) return;
        AmmoText.text = (current < 0) ? $"∞ / {max}" : $"{current} / {max}";
    }

    public void RegisterGun(Gun gun)
    {
        currentGun = gun;
        if (gun != null)
        {
            UpdateAmmoText(gun.GetCurrentAmmo(), gun.maxAmmo);
            HideReloadCircle(); // 무기 전환 시 항상 꺼두기(안전)
        }
    }

    public void ShowReloadCircle() { if (reloadCircle) reloadCircle.SetActive(true); }
    public void HideReloadCircle() { if (reloadCircle) reloadCircle.SetActive(false); }

    public void ShowDiedPanel()
    {
        if (!diedPanel) return;
        diedPanel.SetActive(true);
        Time.timeScale = 0f;
        AudioListener.pause = true;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;
        var cur = SceneManager.GetActiveScene();
        SceneManager.LoadScene(cur.buildIndex);
    }
}
