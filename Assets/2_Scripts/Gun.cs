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

    // 리로드 게이지 오브젝트 (월드/캔버스 상관 X, 활성/비활성만 컨트롤)
    [SerializeField] private GameObject reloadCircleObj;

    [SerializeField] GameObject bulletPrefab;

    int currentAmmo;
    bool isReloading;
    float nextFireTime;
    Player player;

    // ✅ 사망 처리 1회 호출 보장용
    bool deathHandled = false;

    void Start()
    {
        var pObj = GameObject.FindGameObjectWithTag("Player");
        if (pObj) player = pObj.GetComponent<Player>();

        currentAmmo = maxAmmo;
        nextFireTime = 0f;
        if (reloadCircleObj) reloadCircleObj.SetActive(false);

        UIManager.Instance?.UpdateAmmoText(currentAmmo, maxAmmo);
    }

    void Update()
    {
        if (!player) return;

        // 사망 처리
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

        // ⛔ 탄창이 가득하면 R 눌러도 리로드 시작 안 함 (게이지도 안 켜짐)
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
                // 0발이면 자연스럽게 리로드 (여기도 코루틴 가드가 있으니 중복 시작 안 됨)
                StartCoroutine(Reload());
            }
        }
    }

    IEnumerator Reload()
    {
        if (isReloading) yield break;
        // ⛔ 가득 차 있으면 코루틴 자체도 시작하지 않음 (게이지 안 켜짐)
        if (currentAmmo >= maxAmmo) yield break;

        isReloading = true;

        // 리로드 시작 → 게이지 켜기
        if (reloadCircleObj) reloadCircleObj.SetActive(true);

        // 시간 대기(애니/이펙트는 에셋이 알아서 돌 것)
        yield return new WaitForSeconds(reloadTime);

        // 완료
        currentAmmo = maxAmmo;
        UIManager.Instance?.UpdateAmmoText(currentAmmo, maxAmmo);

        // 리로드 끝 → 게이지 끄기
        if (reloadCircleObj) reloadCircleObj.SetActive(false);

        isReloading = false;
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
