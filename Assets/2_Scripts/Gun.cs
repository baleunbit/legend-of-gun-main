using System.Collections;
using UnityEngine;

public class Gun : MonoBehaviour
{
    public Transform Crosshair;

    [Header("탄창")]
    public int maxAmmo = 6;
    public float reloadTime = 2f;

    [Header("연사 속도")]
    public float fireRate = 0.2f;

    [Header("데미지/관통")]
    [SerializeField] float Damage = 5f;
    [SerializeField] int Pierce = 1;

    [SerializeField] GameObject bulletPrefab;

    int currentAmmo;
    bool isReloading;
    float nextFireTime;
    Player player;
    bool deathHandled;
    public bool HasBulletPrefab() => bulletPrefab != null;

    void Start()
    {
        if (!Crosshair)
        {
            Crosshair = GameObject.FindWithTag("Crosshair")?.transform
                     ?? GameObject.Find("Crosshair")?.transform;
        }
        var pObj = GameObject.FindGameObjectWithTag("Player");
        if (pObj) player = pObj.GetComponent<Player>();

        currentAmmo = maxAmmo;
        nextFireTime = 0f;

        UIManager.Instance?.UpdateAmmoText(currentAmmo, maxAmmo);
        UIManager.Instance?.RegisterGun(this);
    }

    void OnDisable()
    {
        // 총이 파괴/비활성 될 때 리로드 UI가 남지 않게 안전망
        UIManager.Instance?.HideReloadCircle();
    }

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

        if (Input.GetKeyDown(KeyCode.R) && currentAmmo < maxAmmo)
        {
            StartCoroutine(Reload());
            return;
        }

        if (Input.GetMouseButton(0) && Time.time >= nextFireTime)
        {
            if (currentAmmo > 0)
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
        if (currentAmmo >= maxAmmo) yield break;

        isReloading = true;
        UIManager.Instance?.ShowReloadCircle();   // ✅ 켜기

        yield return new WaitForSeconds(reloadTime);

        currentAmmo = maxAmmo;
        UIManager.Instance?.UpdateAmmoText(currentAmmo, maxAmmo);

        UIManager.Instance?.HideReloadCircle();   // ✅ 끄기
        isReloading = false;
    }

    void Fire()
    {
        currentAmmo--;
        UIManager.Instance?.UpdateAmmoText(currentAmmo, maxAmmo);

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

    // UIManager가 읽어갈 때 씀
    public int GetCurrentAmmo() => currentAmmo;
}
