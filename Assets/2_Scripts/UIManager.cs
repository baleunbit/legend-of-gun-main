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

    [Header("Weapon Icons (0=Fork, 1=Spoon, 2=Chopstick)")]
    [SerializeField] private GameObject[] weaponIconObjs; // 캔버스에 있는 3개를 순서대로 드래그

    [Header("Died UI")]
    [SerializeField] private GameObject diedPanel;
    [SerializeField] private Button restartButton;

    private Gun currentGun;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        if (diedPanel) diedPanel.SetActive(false);
        if (restartButton)
        {
            restartButton.onClick.RemoveAllListeners();
            restartButton.onClick.AddListener(Restart);
        }

        HideReloadCircle();
        // 아이콘 초기화: 전부 끄기
        SetWeaponIconActive(-1);
    }

    // ====== 공개 API ======

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
            HideReloadCircle();
        }
    }

    public void ShowReloadCircle() { if (reloadCircle) reloadCircle.SetActive(true); }
    public void HideReloadCircle() { if (reloadCircle) reloadCircle.SetActive(false); }

    public void SetWeaponIconActive(int index)
    {
        if (weaponIconObjs == null) return;
        for (int i = 0; i < weaponIconObjs.Length; i++)
            if (weaponIconObjs[i]) weaponIconObjs[i].SetActive(i == index);
    }

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
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
