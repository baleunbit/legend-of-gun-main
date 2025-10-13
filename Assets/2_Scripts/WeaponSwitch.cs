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

    [SerializeField] bool verboseDebug = false;

    int currentIndex = -1;
    GameObject currentGO;
    float nextSwitchTime;

    // 무기별 탄약 저장(공용 탄약을 쓰면 제거 가능)
    //Dictionary<int, int> ammoStates = new Dictionary<int, int>();

    void Start()
    {
        if (!weaponSocket)
        {
            Debug.LogError("[WeaponSwitch] WeaponSocket(무기 장착 위치)이 필요합니다.");
            enabled = false;
            return;
        }

        // 1) 시작 시, 소켓 아래에 이미 무기가 붙어 있다면 그걸 사용 (중복 생성 방지)
        GameObject existing = FindExistingWeaponInSocket();
        if (existing != null)
        {
            currentGO = existing;
            currentIndex = GuessIndex(currentGO);
            if (verboseDebug) Debug.Log($"[WeaponSwitch] 기존 무기 감지: {currentGO.name}, index={currentIndex}");

            SetupCurrentWeaponAfterAttach(currentGO, currentIndex, restoreAmmo: true);
        }
        else
        {
            // 2) 없으면 defaultIndex 장착
            int startIdx = Mathf.Clamp(defaultIndex, 0, (weaponPrefabs?.Count ?? 1) - 1);
            if (verboseDebug) Debug.Log($"[WeaponSwitch] 시작 무기 없음 → default 장착: {startIdx}");
            TryEquip(startIdx);
        }

        // UI 아이콘 정리
        UIManager.Instance?.SetWeaponIconActive(currentIndex >= 0 ? currentIndex : 0);
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

        // ✅ 공용 탄약을 쓴다면 "저장" 자체를 하지 말아야 함
        if (currentGO)
        {
            if (verboseDebug) Debug.Log($"[WeaponSwitch] Remove: {currentGO.name}");
            Destroy(currentGO);
            currentGO = null;
        }

        var prefab = weaponPrefabs[idx];
        if (!prefab)
        {
            Debug.LogError("[WeaponSwitch] 무기 프리팹이 비어있음");
            return;
        }

        currentGO = Instantiate(prefab, weaponSocket);
        currentGO.transform.localPosition = Vector3.zero;
        currentGO.transform.localRotation = Quaternion.identity;
        currentGO.transform.localScale = Vector3.one;

        currentIndex = idx;
        if (verboseDebug) Debug.Log($"[WeaponSwitch] Equip new: {prefab.name}, index={currentIndex}");

        // ✅ 복원도 금지 (공용탄약이면 Gun이 SharedAmmo를 직접 봄)
        SetupCurrentWeaponAfterAttach(currentGO, currentIndex, restoreAmmo: false);

        nextSwitchTime = Time.time + switchCooldown;
    }

    void SetupCurrentWeaponAfterAttach(GameObject go, int idx, bool restoreAmmo)
    {
        var mount = go.GetComponent<WeaponMount>();
        if (mount)
        {
            go.transform.localPosition += (Vector3)mount.localOffset;
            go.transform.localRotation = Quaternion.Euler(0, 0, go.transform.localEulerAngles.z + mount.localZRotation);
        }

        var gun = go.GetComponent<Gun>();
        if (gun)
        {
            if (!crosshair)
            {
                var t = GameObject.FindWithTag("Crosshair")?.transform ?? GameObject.Find("Crosshair")?.transform;
                if (t) crosshair = t;
            }
            if (crosshair) gun.Crosshair = crosshair;

            // ❌ (중요) 공용 탄약 모드에서는 복원 금지
            // if (restoreAmmo && ammoStates.TryGetValue(idx, out int saved)) { gun.SetCurrentAmmo(saved); }

            UIManager.Instance?.RegisterGun(gun);
        }

        UIManager.Instance?.SetWeaponIconActive(idx);
    }

    // ───────── 헬퍼들 ─────────

    GameObject FindExistingWeaponInSocket()
    {
        if (weaponSocket.childCount == 0) return null;

        // 즉시 자식 중에서 Gun 달린 오브젝트를 찾기
        for (int i = 0; i < weaponSocket.childCount; i++)
        {
            var child = weaponSocket.GetChild(i);
            var gun = child.GetComponent<Gun>() ?? child.GetComponentInChildren<Gun>(true);
            if (gun != null) return gun.gameObject;
        }

        // 소켓 하위 어디든 Gun이 달린 것을 찾기
        var guns = weaponSocket.GetComponentsInChildren<Gun>(true);
        if (guns != null && guns.Length > 0) return guns[0].gameObject;

        // 최후: 그냥 첫 자식을 반환
        return weaponSocket.GetChild(0).gameObject;
    }

    // ✅ WeaponId 미사용 버전: 이름 매칭 → 실패 시 defaultIndex
    int GuessIndex(GameObject weaponObj)
    {
        int count = weaponPrefabs?.Count ?? 0;
        if (count == 0) return 0;

        string instName = CleanName(weaponObj.name);
        for (int i = 0; i < count; i++)
        {
            var p = weaponPrefabs[i];
            if (!p) continue;
            string prefabName = CleanName(p.name);
            if (instName.Contains(prefabName) || prefabName.Contains(instName))
                return i;
        }

        // 폴백
        return Mathf.Clamp(defaultIndex, 0, count - 1);
    }

    string CleanName(string s)
    {
        if (string.IsNullOrEmpty(s)) return "";
        s = s.ToLowerInvariant();
        s = s.Replace("(clone)", "");
        s = s.Trim();
        return s;
    }
}