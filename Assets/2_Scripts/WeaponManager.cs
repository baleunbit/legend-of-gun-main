using UnityEngine;
using System;

[DisallowMultipleComponent]
public class WeaponManager : MonoBehaviour
{
    public enum WeaponType { Fork, Spoon, Chopstick }

    [Header("���� ������ (0=Fork, 1=Spoon, 2=Chopstick)")]
    [SerializeField] private GameObject[] weaponPrefabs;

    [Header("���� ����")]
    [SerializeField] private Transform weaponSocket;

    [Header("���� ��Ģ")]
    [SerializeField] private float enemyBaseHP = 30f; // �� �⺻ ü��

    [Header("�Է�/��ȯ")]
    [SerializeField] private bool wrapAround = true;
    [SerializeField] private float switchCooldown = 0.2f;
    private float nextSwitchTime;

    // ��Ÿ�� ����
    public int CurrentStage { get; private set; } = 1;
    public WeaponType MainWeapon { get; private set; } = WeaponType.Fork;
    public WeaponType CurrentWeapon { get; private set; } = WeaponType.Fork;

    public event Action<WeaponType> OnWeaponChanged;

    GameObject currentGO;
    int currentIndex = -1;

    void Start()
    {
        if (!weaponSocket)
        {
            Debug.LogError("[WeaponManager] weaponSocket ������");
            enabled = false; return;
        }
        // ���� �� �ֹ��� ����
        Equip((int)MainWeapon);
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

    // ���� �������� ��Ģ ��������������������������������������������������������������
    public void ApplyStageRules(int stage)
    {
        CurrentStage = stage;
        MainWeapon = stage switch
        {
            1 => WeaponType.Fork,
            2 => WeaponType.Fork,
            3 => WeaponType.Spoon,
            4 => WeaponType.Chopstick,
            _ => WeaponType.Fork
        };
        // ���������� �ٲ�� �ֹ��⸦ �տ� �� (���ϸ� �ּ� ó��)
        Equip((int)MainWeapon);
    }

    // �ֹ��� 3Ÿ / ���ֹ��� 6Ÿ
    public float GetDamage()
    {
        bool isMain = (CurrentWeapon == MainWeapon);
        return isMain ? (enemyBaseHP / 3f) : (enemyBaseHP / 6f);
    }

    // ���� ���� ��ȯ/���� ����������������������������������������������������������
    public void Next()
    {
        int n = weaponPrefabs?.Length ?? 0;
        if (n == 0) return;
        int i = currentIndex + 1;
        if (i >= n) i = wrapAround ? 0 : n - 1;
        Equip(i);
    }

    public void Prev()
    {
        int n = weaponPrefabs?.Length ?? 0;
        if (n == 0) return;
        int i = currentIndex - 1;
        if (i < 0) i = wrapAround ? n - 1 : 0;
        Equip(i);
    }

    public void Equip(int idx)
    {
        if (weaponPrefabs == null || idx < 0 || idx >= weaponPrefabs.Length) return;
        if (idx == currentIndex && currentGO) return;

        if (currentGO) Destroy(currentGO);

        var prefab = weaponPrefabs[idx];
        if (!prefab) return;

        currentGO = Instantiate(prefab, weaponSocket);
        currentGO.transform.localPosition = Vector3.zero;
        currentGO.transform.localRotation = Quaternion.identity;
        currentGO.transform.localScale = Vector3.one;

        currentIndex = idx;
        CurrentWeapon = (WeaponType)idx;
        OnWeaponChanged?.Invoke(CurrentWeapon);

        nextSwitchTime = Time.time + switchCooldown;
    }
}
