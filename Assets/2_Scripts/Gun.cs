using System.Collections;
using UnityEngine;

public class Gun : MonoBehaviour
{
    [Header("크로스헤어")]
    public Transform Crosshair;

    [Header("탄창")]
    public int maxAmmo = 6;
    public float reloadTime = 2f;

    [Header("연사 속도")]
    public float fireRate = 0.2f;

    [Header("데미지/관통")]
    [SerializeField] float Damage = 5f;
    [SerializeField] int Pierce = 1;  // 1=관통없음

    [SerializeField] GameObject bulletPrefab;

    int currentAmmo;
    bool isReloading;
    float nextFireTime;
    Player player;

    void Start()
    {
        var pObj = GameObject.FindGameObjectWithTag("Player");
        if (pObj) player = pObj.GetComponent<Player>();

        currentAmmo = maxAmmo;
        nextFireTime = 0f;

        UIManager.Instance.UpdateAmmoText(currentAmmo, maxAmmo);
    }

    void Update()
    {
        // ✅ Player.cs 수정 없이 health로만 체크
        if (!player || player.health <= 0) return;

        if (isReloading) return;

        if (Input.GetKeyDown(KeyCode.R))
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
        isReloading = true;
        yield return new WaitForSeconds(reloadTime);
        currentAmmo = maxAmmo;
        isReloading = false;
        UIManager.Instance.UpdateAmmoText(currentAmmo, maxAmmo);
    }

    void Fire()
    {
        currentAmmo--;

        UIManager.Instance?.UpdateAmmoText(currentAmmo, maxAmmo);

        Vector3 origin = transform.position;
        Vector3 aim = Crosshair ? Crosshair.position : origin + transform.right;
        Vector2 dir = (aim - origin).normalized;

        float spawnOffset = GetPlayerRadius() + 0.1f;
        Vector3 spawnPos = origin + (Vector3)dir * spawnOffset;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        Quaternion rot = Quaternion.AngleAxis(angle + 270f, Vector3.forward);

        var go = Instantiate(bulletPrefab, spawnPos, rot);
        var b = go.GetComponent<Bullet>();
        b.Init(Damage, Pierce, dir);
        b.Setup(dir);

        // 플레이어와 충돌 무시
        var bulletCol = go.GetComponent<Collider2D>();
        var ownerCols = player.GetComponentsInChildren<Collider2D>(true);
        foreach (var c in ownerCols)
            if (c && bulletCol) Physics2D.IgnoreCollision(bulletCol, c, true);
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
