// UIManager.cs — 최소 수정/복구 버전
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Panels (옵션)")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject diedPanel;
    [SerializeField] private GameObject settingsPanel;

    [Header("Ammo UI (옵션)")]
    [SerializeField] private TextMeshProUGUI ammoText;

    [Header("Reload Circle (원래 쓰던 오브젝트 연결)")]
    [SerializeField] private GameObject reloadCircleGO;   // ← 네가 쓰던 리로드 원 GameObject

    [Header("Weapon Icons (원래 쓰던 오브젝트 배열)")]
    [SerializeField] private GameObject[] weaponIconGOs;  // ← 무기 아이콘 GameObject 배열

    private Gun currentGun;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    // ─────────────────────────────────────────────
    // 🔫 Gun/WeaponManager 연동 — 기존 시그니처 그대로
    // ─────────────────────────────────────────────
    public void RegisterGun(Gun gun)
    {
        currentGun = gun;
        if (currentGun != null)
            UpdateAmmoText(currentGun.GetCurrentAmmo(), currentGun.maxAmmo);
        else
            UpdateAmmoText(0, 0);
    }

    public void UpdateAmmoText(int current, int max)
    {
        if (ammoText)
            ammoText.text = $"{current} / {max}";
        // 텍스트가 없으면 아무것도 안 함(원래 UI 유지)
    }

    // 리로드 서클 — 네가 쓰던대로 SetActive만
    // (호환 위해 3가지 시그니처 모두 제공)
    public void ShowReloadCircle() { if (reloadCircleGO) reloadCircleGO.SetActive(true); }
    public void HideReloadCircle() { if (reloadCircleGO) reloadCircleGO.SetActive(false); }
    public void ShowReloadCircle(bool on) { if (reloadCircleGO) reloadCircleGO.SetActive(on); }

    // 무기 아이콘 — 인덱스만 활성화, 나머지 비활성 (원래 방식)
    public void SetWeaponIconActive(int activeIndex)
    {
        if (weaponIconGOs == null) return;
        for (int i = 0; i < weaponIconGOs.Length; i++)
        {
            var go = weaponIconGOs[i];
            if (!go) continue;
            go.SetActive(i == activeIndex);
        }
    }

    // ─────────────────────────────────────────────
    // ☠️ 사망 패널(옵션) — 기존에 썼다면 유지, 아니면 무시
    // ─────────────────────────────────────────────
    public void ShowDiedPanel()
    {
        if (diedPanel) diedPanel.SetActive(true);
        Time.timeScale = 0f;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    // ─────────────────────────────────────────────
    // ⏸ 일시정지/설정(옵션) — 연결 안 했으면 그냥 무시됨
    // ─────────────────────────────────────────────
    public void ShowPausePanel(bool on)
    {
        if (pausePanel) pausePanel.SetActive(on);
        Time.timeScale = on ? 0f : 1f;
        Cursor.visible = on;
        Cursor.lockState = on ? CursorLockMode.None : CursorLockMode.Confined;
    }

    public void ShowSettingsPanel(bool on)
    {
        if (settingsPanel) settingsPanel.SetActive(on);
    }

    // 🔁 재시작 버튼(옵션)
    public void OnClick_Restart()
    {
        Time.timeScale = 1f;
        var s = SceneManager.GetActiveScene();
        SceneManager.LoadScene(s.buildIndex);
    }
}
