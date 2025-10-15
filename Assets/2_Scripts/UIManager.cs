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

    [Header("Reload Circle (원래 쓰던 오브젝트 연결)")]
    [SerializeField] private GameObject reloadCircleGO;

    [Header("Weapon Icons (원래 쓰던 오브젝트 배열)")]
    [SerializeField] private GameObject[] weaponIconGOs;

    [Header("Exp UI")]
    [SerializeField] private Image expFillImage;           // Filled 타입 Image
    [SerializeField] private TextMeshProUGUI levelText;    // "LV.1" 표기

    [Header("LevelUp Panel (GameObject 버전)")]
    [SerializeField] private GameObject levelUpPanelGO;    // ← 패널 루트 오브젝트
    [SerializeField] private Button btn1;
    [SerializeField] private Button btn2;
    [SerializeField] private Button btn3;
    [SerializeField] private Button btn4;

    private Gun currentGun;

    void Awake()
    {
        if (Instance && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        if (btn1) btn1.onClick.AddListener(() => OnChooseAbility(1));
        if (btn2) btn2.onClick.AddListener(() => OnChooseAbility(2));
        if (btn3) btn3.onClick.AddListener(() => OnChooseAbility(3));
        if (btn4) btn4.onClick.AddListener(() => OnChooseAbility(4));

        HideLevelUpPanelImmediate(); // GameObject.SetActive(false)
    }

    // ==== 레벨업 패널 (GameObject) ====
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
        player?.ApplyLevelUpChoice(idx);
        HideLevelUpPanel();
    }

    // ==== 경험치 UI ====
    public void SetExpUI(int level, int exp, int toNext)
    {
        if (expFillImage)
        {
            float ratio = (toNext > 0) ? (float)exp / toNext : 0f;
            expFillImage.fillAmount = Mathf.Clamp01(ratio);
        }
        if (levelText)
        {
            levelText.text = $"LV.{level}";
        }
    }

    // ==== Gun/Weapon UI ====
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

    // ==== 사망 패널 ====
    public void ShowDiedPanel()
    {
        if (diedPanel) diedPanel.SetActive(true);
        Time.timeScale = 0f;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    // ==== 일시정지 / 설정 ====
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

    // ==== 재시작 버튼 ====
    public void OnClick_Restart()
    {
        Time.timeScale = 1f;
        var s = SceneManager.GetActiveScene();
        SceneManager.LoadScene(s.buildIndex);
    }
}
