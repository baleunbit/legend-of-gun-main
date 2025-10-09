using UnityEngine;

public class WeaponSwitcher : MonoBehaviour
{
    [Header("무기 프리팹 (0=포크, 1=숟가락, 2=젓가락)")]
    public GameObject[] weaponPrefabs;

    [Header("장착 위치(소켓)")]
    public Transform weaponSocket;          // Player 하위 WeaponSocket
    [Header("크로스헤어")]
    public Transform crosshair;             // Crosshair Transform

    [Header("입력 옵션")]
    public int defaultIndex = 0;
    public bool wrapAround = true;
    public float switchCooldown = 0.2f;

    int currentIndex = -1;
    float nextSwitchTime;

    void Awake()
    {
        if (!weaponSocket) weaponSocket = transform;
        // 혹시 플레이어 루트에 무기(Gun) 달린 애가 직접 붙어 있으면 제거(스크린샷 케이스)
        PurgeStrayGunsOnPlayerRoot();
    }

    void Start()
    {
        TryEquip(defaultIndex);
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
        if (weaponPrefabs == null || weaponPrefabs.Length == 0) return;
        int idx = currentIndex + 1;
        if (idx >= weaponPrefabs.Length) idx = wrapAround ? 0 : weaponPrefabs.Length - 1;
        TryEquip(idx);
    }

    public void Prev()
    {
        if (weaponPrefabs == null || weaponPrefabs.Length == 0) return;
        int idx = currentIndex - 1;
        if (idx < 0) idx = wrapAround ? weaponPrefabs.Length - 1 : 0;
        TryEquip(idx);
    }

    public void TryEquip(int idx)
    {
        if (weaponPrefabs == null || idx < 0 || idx >= weaponPrefabs.Length) return;
        if (idx == currentIndex) { /*같은 무기면 재생성 안 함*/ return; }

        nextSwitchTime = Time.time + switchCooldown;

        // 1) 소켓 비우기(겹침 방지: 비활성화가 아니라 '파괴')
        ClearSocket();

        // 2) 새 무기 생성
        var prefab = weaponPrefabs[idx];
        if (!prefab) return;

        var go = Instantiate(prefab, weaponSocket);
        go.transform.localPosition = Vector3.zero;
        go.transform.localRotation = Quaternion.identity;
        go.transform.localScale = Vector3.one;

        currentIndex = idx;

        // 3) 크로스헤어 주입 + UI 갱신
        var gun = go.GetComponent<Gun>();
        if (gun)
        {
            if (!crosshair)
            {
                // 안전망: 태그나 이름으로 찾아서 세팅
                crosshair = GameObject.FindWithTag("Crosshair")?.transform
                         ?? GameObject.Find("Crosshair")?.transform;
            }
            gun.Crosshair = crosshair;
            UIManager.Instance?.RegisterGun(gun);
        }
    }

    void ClearSocket()
    {
        for (int i = weaponSocket.childCount - 1; i >= 0; i--)
        {
            var child = weaponSocket.GetChild(i);
            if (Application.isPlaying) Destroy(child.gameObject);
            else DestroyImmediate(child.gameObject);
        }
    }

    void PurgeStrayGunsOnPlayerRoot()
    {
        // 소켓 바깥(플레이어 루트)에 붙어있는 Gun은 모두 제거
        var guns = GetComponentsInChildren<Gun>(true);
        foreach (var g in guns)
        {
            if (weaponSocket && g.transform.IsChildOf(weaponSocket)) continue; // 소켓 아래는 놔둠
            if (Application.isPlaying) Destroy(g.gameObject);
            else DestroyImmediate(g.gameObject);
        }
    }
}
