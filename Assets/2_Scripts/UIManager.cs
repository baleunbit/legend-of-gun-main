// UIManager.cs
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Panels (옵션)")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject diedPanel;
    [SerializeField] private GameObject settingsPanel;

    [Header("Ammo UI (옵션)")]
    [SerializeField] private TextMeshProUGUI ammoText;

    [Header("Reload Circle (옵션)")]
    [SerializeField] private GameObject reloadCircleGO;

    [Header("Weapon Icons (옵션)")]
    [SerializeField] private GameObject[] weaponIconGOs;

    [Header("Exp UI (Fill Image + LV 텍스트)")]
    [SerializeField] private Image expFillImage;
    [SerializeField] private TextMeshProUGUI levelText;   // "LV.1"

    [Header("LevelUp Panel (GameObject)")]
    [SerializeField] private GameObject levelUpPanelGO;
    [SerializeField] private Button btn1;
    [SerializeField] private Button btn2;
    [SerializeField] private Button btn3;
    [SerializeField] private Button btn4;

    private Gun currentGun;

    void Awake()
    {
        if (Instance && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        // 레벨업 패널 자식의 모든 Button에서 인스펙터 OnClick 싹 비움
        if (levelUpPanelGO)
        {
            var allBtns = levelUpPanelGO.GetComponentsInChildren<UnityEngine.UI.Button>(true);
            foreach (var b in allBtns) b.onClick.RemoveAllListeners();
        }

        // 우리가 원하는 동작만 다시 연결
        if (btn1) btn1.onClick.AddListener(() => OnChooseAbility(1));
        if (btn2) btn2.onClick.AddListener(() => OnChooseAbility(2));
        if (btn3) btn3.onClick.AddListener(() => OnChooseAbility(3));
        if (btn4) btn4.onClick.AddListener(() => OnChooseAbility(4));

        HideLevelUpPanelImmediate();
    }

    // ===== 레벨업 패널 =====
    public void ShowLevelUpPanel()
    {
        if (levelUpPanelGO) levelUpPanelGO.SetActive(true);
        Time.timeScale = 0f;
    }
    public void HideLevelUpPanel()
    {
        if (levelUpPanelGO) levelUpPanelGO.SetActive(false);
        Time.timeScale = 1f;
    }
    public void HideLevelUpPanelImmediate()
    {
        if (levelUpPanelGO) levelUpPanelGO.SetActive(false);
    }
    void OnChooseAbility(int idx)
    {
        var p = GameObject.FindGameObjectWithTag("Player");
        var player = p ? p.GetComponent<Player>() : null;

        player?.ApplyLevelUpChoice(idx); // 능력 적용 (씬 로드 금지)
        HideLevelUpPanel();              // 내부에서 Time.timeScale = 1f
    }

    // ===== 경험치 UI =====
    public void SetExpUI(int level, int exp, int toNext)
    {
        if (expFillImage)
        {
            float ratio = (toNext > 0) ? (float)exp / toNext : 0f;
            expFillImage.fillAmount = Mathf.Clamp01(ratio);
        }
        if (levelText) levelText.text = $"LV.{level}";
    }

    // ===== Gun/Weapon UI (옵션) =====
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
        if (ammoText) ammoText.text = $"{current} / {max}";
    }
    public void ShowReloadCircle() { if (reloadCircleGO) reloadCircleGO.SetActive(true); }
    public void HideReloadCircle() { if (reloadCircleGO) reloadCircleGO.SetActive(false); }
    public void ShowReloadCircle(bool on) { if (reloadCircleGO) reloadCircleGO.SetActive(on); }
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

    // ===== 사망/일시정지/재시작 (옵션) =====
    public void ShowDiedPanel()
    {
        if (diedPanel) diedPanel.SetActive(true);
        Time.timeScale = 0f;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }
    public void ShowPausePanel(bool on)
    {
        if (pausePanel) pausePanel.SetActive(on);
        Time.timeScale = on ? 0f : 1f;
        Cursor.visible = on;
        Cursor.lockState = on ? CursorLockMode.None : CursorLockMode.Confined;
    }
    public void ShowSettingsPanel(bool on) { if (settingsPanel) settingsPanel.SetActive(on); }
    public void OnClick_Restart()
    {
        Time.timeScale = 1f;
        var s = SceneManager.GetActiveScene();
        SceneManager.LoadScene(s.buildIndex);
    }
}
