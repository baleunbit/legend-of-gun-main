using System.Collections.Generic;
using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    [Header("���� ������ (0=Fork, 1=Spoon, 2=ChopStick)")]
    public List<GameObject> weaponPrefabs;

    [Header("���� ���� (Player �Ʒ� �� ������Ʈ)")]
    public Transform weaponSocket;

    [Header("�Է�/�ɼ�")]
    public bool wrapAround = true;
    public float switchCooldown = 0.2f;
    public int defaultIndex = 0;

    [Header("����(����)")]
    public Transform crosshair;        // ������ �ڵ����� ã��
    public SharedAmmo sharedAmmo;      // ���� ź�� ���� ����

    [Header("Stage Rules (optional)")]
    public bool useStageRules = false;
    public int stage1Index = 0;
    public int stage2Index = 0;
    public int stage3Index = 0;

    int currentIndex = -1;
    GameObject currentGO;
    float nextSwitchTime;

    void Start()
    {
        if (!weaponSocket)
        {
            Debug.LogError("[WeaponManager] weaponSocket ���� �ʿ�");
            enabled = false; return;
        }
        if (!crosshair)
            crosshair = GameObject.FindWithTag("Crosshair")?.transform
                     ?? GameObject.Find("Crosshair")?.transform;

        Equip(Mathf.Clamp(defaultIndex, 0, (weaponPrefabs?.Count ?? 1) - 1));
    }

    void Update()
    {
        if (Time.time < nextSwitchTime) return;

        if (Input.GetKeyDown(KeyCode.Alpha1)) { Equip(0); return; }
        if (Input.GetKeyDown(KeyCode.Alpha2)) { Equip(1); return; }
        if (Input.GetKeyDown(KeyCode.Alpha3)) { Equip(2); return; }

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll > 0.01f) Next();
        else if (scroll < -0.01f) Prev();
    }

    public void ApplyStageRules(int stage)
    {
        if (!useStageRules) return;

        int idx = defaultIndex;
        switch (stage)
        {
            case 1: idx = stage1Index; break;
            case 2: idx = stage2Index; break;
            case 3: idx = stage3Index; break;
            default: idx = defaultIndex; break;
        }

        Equip(Mathf.Clamp(idx, 0, (weaponPrefabs?.Count ?? 1) - 1));
    }

    public void Next()
    {
        int n = weaponPrefabs?.Count ?? 0; if (n == 0) return;
        int i = currentIndex + 1; if (i >= n) i = wrapAround ? 0 : n - 1;
        Equip(i);
    }
    public void Prev()
    {
        int n = weaponPrefabs?.Count ?? 0; if (n == 0) return;
        int i = currentIndex - 1; if (i < 0) i = wrapAround ? n - 1 : 0;
        Equip(i);
    }

    void Equip(int idx)
    {
        if (weaponPrefabs == null || idx < 0 || idx >= weaponPrefabs.Count) return;
        if (idx == currentIndex) return;

        // ���� ���� ����
        if (currentGO) { Destroy(currentGO); currentGO = null; }

        var prefab = weaponPrefabs[idx];
        if (!prefab) { Debug.LogError("[WeaponManager] ������ �������"); return; }

        // ������ '�ڽ�'���� ���� (���尪 ���� X �� ���� 0���� ����)
        currentGO = Instantiate(prefab, weaponSocket);
        currentGO.transform.localPosition = Vector3.zero;
        currentGO.transform.localRotation = Quaternion.identity;
        currentGO.transform.localScale = Vector3.one;

        currentIndex = idx;

        // ���� ����(WeaponMount)
        var mount = currentGO.GetComponent<WeaponMount>();
        if (mount)
        {
            currentGO.transform.localPosition += (Vector3)mount.localOffset;
            var e = currentGO.transform.localEulerAngles; e.z += mount.localZRotation;
            currentGO.transform.localEulerAngles = e;
        }

        // Gun ����
        var gun = currentGO.GetComponent<Gun>();
        if (gun)
        {
            if (crosshair) gun.Crosshair = crosshair;
            if (sharedAmmo) gun.sharedAmmo = sharedAmmo; // ���� ź�� ���
            UIManager.Instance?.RegisterGun(gun);       // ������/ź�� UI ����
        }

        nextSwitchTime = Time.time + switchCooldown;
    }
}
