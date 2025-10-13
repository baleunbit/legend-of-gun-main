using System.Collections;
using UnityEngine;

public class Gun : MonoBehaviour
{
    public Transform Crosshair;

    [Header("탄창(개별 모드용 표시값)")]
    public int maxAmmo = 6;
    public float reloadTime = 2f;

    [Header("연사 속도")]
    public float fireRate = 0.2f;

    [Header("데미지/관통")]
    [SerializeField] float Damage = 5f;
    [SerializeField] int Pierce = 1;

    [SerializeField] GameObject bulletPrefab;

    // ✅ 공용 탄약이 있으면 그걸 우선 사용 (없으면 개별 currentAmmo 사용)
    [SerializeField] SharedAmmo sharedAmmo;

    int currentAmmo;   // 공용이 없을 때만 사용
    bool isReloading;
    float nextFireTime;
    Player player;
    bool deathHandled;
    public bool HasBulletPrefab() => bulletPrefab != null;
    bool ammoInitialized = false;

    void Awake()
    {
        // 개별 모드 보호용 기본값
        currentAmmo = maxAmmo;

        // 공용 탄약 자동 탐색(인스펙터에 안 꽂아도 동작)
        if (!sharedAmmo) sharedAmmo = FindObjectOfType<SharedAmmo>();
    }

    void Start()
    {
        if (!Crosshair)
        {
            Crosshair = GameObject.FindWithTag("Crosshair")?.transform
                     ?? GameObject.Find("Crosshair")?.transform;
        }
        var pObj = GameObject.FindGameObjectWithTag("Player");
        if (pObj) player = pObj.GetComponent<Player>();

        nextFireTime = 0f;

        // ⚠️ currentAmmo = maxAmmo; 절대 덮어쓰지 말 것(전환/공용탄약 유지)
        UIManager.Instance?.UpdateAmmoText(GetDisplayAmmo(), GetDisplayMax());
        UIManager.Instance?.RegisterGun(this);
    }

    void OnDisable()
    {
        UIManager.Instance?.HideReloadCircle();
    }

    // ───────── 공용/표시 헬퍼 ─────────
    int GetDisplayAmmo() => sharedAmmo ? sharedAmmo.currentAmmo : currentAmmo;
    int GetDisplayMax() => sharedAmmo ? sharedAmmo.maxAmmo : maxAmmo;

    public void SetCurrentAmmo(int value)
    {
        if (sharedAmmo)
            sharedAmmo.currentAmmo = Mathf.Clamp(value, 0, sharedAmmo.maxAmmo);
        else
            currentAmmo = Mathf.Clamp(value, 0, maxAmmo);

        ammoInitialized = true;
        UIManager.Instance?.UpdateAmmoText(GetDisplayAmmo(), GetDisplayMax());
    }

    // UIManager가 읽어갈 때 씀(공용이면 공용 값)
    public int GetCurrentAmmo() => GetDisplayAmmo();

    void Update()
    {
        if (!player) return;

        if (player.health <= 0)
        {
            if (!deathHandled)
            {
                deathHandled = true;
                UIManager.Instance?.ShowDiedPanel();
            }
            return;
        }

        if (isReloading) return;

        // 공용/개별 기준으로 판정
        if (Input.GetKeyDown(KeyCode.R) && GetDisplayAmmo() < GetDisplayMax())
        {
            StartCoroutine(Reload());
            return;
        }

        if (Input.GetMouseButton(0) && Time.time >= nextFireTime)
        {
            if (GetDisplayAmmo() > 0)
            {
                Fire();
                nextFireTime = Time.time + fireRate;
            }
            else
            {
                StartCoroutine(Reload());
            }
        }
    }

    IEnumerator Reload()
    {
        if (isReloading) yield break;
        if (GetDisplayAmmo() >= GetDisplayMax()) yield break;

        isReloading = true;
        UIManager.Instance?.ShowReloadCircle();

        yield return new WaitForSeconds(reloadTime);

        if (sharedAmmo) sharedAmmo.Refill();
        else currentAmmo = maxAmmo;

        UIManager.Instance?.UpdateAmmoText(GetDisplayAmmo(), GetDisplayMax());
        UIManager.Instance?.HideReloadCircle();
        isReloading = false;
    }

    void Fire()
    {
        // ✅ 공용 탄약 우선 차감
        bool consumed = false;
        if (sharedAmmo)
        {
            consumed = sharedAmmo.TryConsume(1);
        }
        else
        {
            if (currentAmmo > 0) { currentAmmo--; consumed = true; }
        }
        if (!consumed) return;

        UIManager.Instance?.UpdateAmmoText(GetDisplayAmmo(), GetDisplayMax());

        // 크로스헤어 안전망
        if (!Crosshair)
            Crosshair = GameObject.FindWithTag("Crosshair")?.transform
                     ?? GameObject.Find("Crosshair")?.transform;

        Vector3 origin = transform.position;
        Vector3 aim = Crosshair ? Crosshair.position : origin + transform.right;
        Vector2 dir = (aim - origin).normalized;

        float spawnOffset = GetPlayerRadius() + 0.1f;
        Vector3 spawnPos = origin + (Vector3)dir * spawnOffset;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        Quaternion rot = Quaternion.AngleAxis(angle + 270f, Vector3.forward);

        var go = Instantiate(bulletPrefab, spawnPos, rot);
        var b = go.GetComponent<Bullet>();
        if (b != null)
        {
            b.Init(Damage, Pierce, dir);
            b.Setup(dir);
        }
        else
        {
            Debug.LogError("[Gun] Bullet prefab에 Bullet 스크립트가 없습니다.", go);
        }

        // 플레이어와 충돌 무시
        if (player)
        {
            var bulletCol = go.GetComponent<Collider2D>();
            var ownerCols = player.GetComponentsInChildren<Collider2D>(true);
            foreach (var c in ownerCols)
                if (c && bulletCol) Physics2D.IgnoreCollision(bulletCol, c, true);
        }
    }

    float GetPlayerRadius()
    {
        float r = 0.3f;
        var cols = player.GetComponentsInChildren<Collider2D>(true);
        foreach (var c in cols)
        {
            if (!c) continue;
            var b = c.bounds;
            r = Mathf.Max(r, Mathf.Max(b.extents.x, b.extents.y));
        }
        return r;
    }
}
