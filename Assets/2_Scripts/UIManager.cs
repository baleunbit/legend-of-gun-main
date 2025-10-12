// 파일: UIManager.cs
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
    [SerializeField] private GameObject reloadCircle;

    [Header("Weapon Icon (0=Fork, 1=Spoon, 2=Chopsticks)")]
    [SerializeField] private Image weaponIcon;          // 우하단 아이콘 Image
    [SerializeField] private Sprite[] weaponIcons;      // 순서대로 아이콘 스프라이트 넣기

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
        HideReloadCircle();
    }

    // ── HUD ─────────────────────────────────────────────
    public void UpdateAmmoText(int current, int max)
    {
        if (!AmmoText) return;
        AmmoText.text = (current < 0) ? $"∞ / {max}" : $"{current} / {max}";
    }

    public void UpdateWeaponIconFromPrefab(GameObject weaponPrefab)
    {
        if (!weaponIcon) return;
        var sr = weaponPrefab ? weaponPrefab.GetComponentInChildren<SpriteRenderer>() : null;
        weaponIcon.sprite = sr ? sr.sprite : null;
        weaponIcon.enabled = weaponIcon.sprite != null;
    }

    public void RegisterGun(Gun gun)
    {
        currentGun = gun;
        if (gun != null)
        {
            UpdateAmmoText(gun.GetCurrentAmmo(), gun.maxAmmo);
            HideReloadCircle();
        }
    }

    // ── Reload UI ───────────────────────────────────────
    public void ShowReloadCircle() { if (reloadCircle) reloadCircle.SetActive(true); }
    public void HideReloadCircle() { if (reloadCircle) reloadCircle.SetActive(false); }

    // ── Weapon Icon ─────────────────────────────────────
    // WeaponSwitcher에서 index(0/1/2)로 호출하는 용
    public void UpdateWeaponIcon(int index)
    {
        if (!weaponIcon) return;

        if (weaponIcons != null && index >= 0 && index < weaponIcons.Length && weaponIcons[index] != null)
        {
            weaponIcon.sprite = weaponIcons[index];
            weaponIcon.enabled = true;
        }
        else
        {
            // 설정이 비었으면 아이콘 숨김
            weaponIcon.enabled = false;
        }
    }

    // 혹시 프리팹에 아이콘 스프라이트가 달려있어 직접 넘길 때 쓰는 오버로드
    public void UpdateWeaponIcon(Sprite sprite)
    {
        if (!weaponIcon) return;
        if (sprite != null)
        {
            weaponIcon.sprite = sprite;
            weaponIcon.enabled = true;
        }
        else weaponIcon.enabled = false;
    }

    // ── Died Panel ──────────────────────────────────────
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
