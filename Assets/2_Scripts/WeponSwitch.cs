using System.Collections.Generic;
using UnityEngine;

public class WeaponSwitch : MonoBehaviour
{
    [Header("무기 프리팹 (0=Fork, 1=Spoon, 2=ChopStick)")]
    [SerializeField] List<GameObject> weaponPrefabs;

    [Header("장착 위치(빈 GameObject)")]
    [SerializeField] Transform weaponSocket;

    [Header("입력/옵션")]
    [SerializeField] bool wrapAround = true;
    [SerializeField] float switchCooldown = 0.2f;
    [SerializeField] int defaultIndex = 0;

    [Header("참조(선택)")]
    [SerializeField] Transform crosshair;

    int currentIndex = -1;
    GameObject currentGO;
    float nextSwitchTime;

    void Start()
    {
        if (!weaponSocket)
        {
            Debug.LogError("[WeaponSwitcher] WeaponSocket()이 필요합니다. 빈 오브젝트를 만들어 할당하세요.");
            enabled = false; return;
        }
        // 시작 무기 장착
        TryEquip(Mathf.Clamp(defaultIndex, 0, (weaponPrefabs?.Count ?? 1) - 1));
    }

    void Update()
    {
        if (Time.time < nextSwitchTime) return;

        if (Input.GetKeyDown(KeyCode.Alpha1)) { TryEquip(0); return; }
        if (Input.GetKeyDown(KeyCode.Alpha2)) { TryEquip(1); return; }
        if (Input.GetKeyDown(KeyCode.Alpha3)) { TryEquip(2); return; }

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll > 0.01f) Next();
        else if (scroll < -0.01f) Prev();
    }

    public void Next()
    {
        int n = weaponPrefabs?.Count ?? 0;
        if (n == 0) return;
        int i = currentIndex + 1;
        if (i >= n) i = wrapAround ? 0 : n - 1;
        TryEquip(i);
    }

    public void Prev()
    {
        int n = weaponPrefabs?.Count ?? 0;
        if (n == 0) return;
        int i = currentIndex - 1;
        if (i < 0) i = wrapAround ? n - 1 : 0;
        TryEquip(i);
    }

    void TryEquip(int idx)
    {
        if (weaponPrefabs == null || idx < 0 || idx >= weaponPrefabs.Count) return;
        if (idx == currentIndex) return;

        // 이전 무기 제거
        if (currentGO) Destroy(currentGO);

        var prefab = weaponPrefabs[idx];
        if (!prefab) { Debug.LogError("[WeaponSwitcher] 무기 프리팹이 비어있음"); return; }

        // 소켓의 정확한 로컬 기준으로 장착 (worldPositionStays=false 중요!)
        currentGO = Instantiate(prefab);
        currentGO.transform.SetParent(weaponSocket, false);
        currentGO.transform.localPosition = Vector3.zero;
        currentGO.transform.localRotation = Quaternion.identity;
        currentGO.transform.localScale = Vector3.one;
        currentGO.name = prefab.name; // 깔끔하게

        currentIndex = idx;
        nextSwitchTime = Time.time + switchCooldown;

        // Gun 세팅
        var gun = currentGO.GetComponent<Gun>();
        if (!gun)
        {
            Debug.LogError("[WeaponSwitcher] 프리팹에 Gun 컴포넌트가 없습니다.", currentGO);
            return;
        }
        if (!gun.Crosshair && crosshair) gun.Crosshair = crosshair;

        // 탄 프리팹이 비어있으면 바로 알려줌 (이번에 뜬 에러의 원인)
        if (!gun.HasBulletPrefab())
            Debug.LogError($"[WeaponSwitcher] '{prefab.name}'의 Gun.bulletPrefab 미할당!");

        var mount = currentGO.GetComponent<WeaponMount>();
        if (mount)
        {
            currentGO.transform.localPosition += (Vector3)mount.localOffset;
            currentGO.transform.localRotation = Quaternion.Euler(
                0, 0, currentGO.transform.localEulerAngles.z + mount.localZRotation
            );
        }
        UIManager.Instance?.UpdateWeaponIconFromPrefab(prefab);

        // UI 갱신
        UIManager.Instance?.RegisterGun(gun);
        UIManager.Instance?.UpdateWeaponIcon(currentIndex);
    }
}
