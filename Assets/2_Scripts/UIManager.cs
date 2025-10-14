using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Ammo & Icon")]
    [SerializeField] TextMeshProUGUI ammoText;     // 예: Canvas/AmmoText
    [SerializeField] SpriteRenderer weaponIconSR;  // 예: Canvas/WeaponIcon (SpriteRenderer)

    [Header("Reload UI")]
    [SerializeField] GameObject reloadCircle;      // 예: Canvas/ReloadCircle (부모 오브젝트)

    [Header("Died Panel")]
    [SerializeField] GameObject diedPanel;

    Gun currentGun;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        if (reloadCircle) reloadCircle.SetActive(false);
        if (weaponIconSR) weaponIconSR.enabled = false;
    }

    // ───────── 외부에서 호출 ─────────
    public void RegisterGun(Gun gun)
    {
        currentGun = gun;
        UpdateAmmoText(gun.GetCurrentAmmo(), gun.GetMaxAmmo());

        // 무기 아이콘 = 현재 무기의 SpriteRenderer 스프라이트
        if (weaponIconSR)
        {
            var sr = gun ? gun.GetComponentInChildren<SpriteRenderer>(true) : null;
            weaponIconSR.sprite = sr ? sr.sprite : null;
            weaponIconSR.enabled = weaponIconSR.sprite != null;
        }
    }

    public void UpdateAmmoText(int cur, int max)
    {
        if (ammoText) ammoText.text = $"{cur} / {max}";
    }

    public void ShowReloadCircle()
    {
        if (reloadCircle && !reloadCircle.activeSelf) reloadCircle.SetActive(true);
    }

    public void HideReloadCircle()
    {
        if (reloadCircle && reloadCircle.activeSelf) reloadCircle.SetActive(false);
    }
    public void ShowDiedPanel()
    {
        if (diedPanel) diedPanel.SetActive(true);
    }

    public void HideDiedPanel()
    {
        if (diedPanel) diedPanel.SetActive(false);
    }
}
