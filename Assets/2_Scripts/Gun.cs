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

    // Gun.cs 상단 필드들 사이에 추가
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

        // UI 매니저가 아직 초기화 전일 수 있으니 널가드
        UIManager.Instance?.UpdateAmmoText(currentAmmo, maxAmmo);
    }

    void Update()
    {
        // ✅ Player.cs 수정 없이 health로만 체크
        if (!player)
            return;

        // ✅ 체력 0 이하가 되는 '순간'에만 한 번 처리
        if (player.health <= 0)
        {
            if (!deathHandled)
            {
                deathHandled = true;

                // 사망 패널 표시 + 일시정지
                UIManager.Instance?.ShowDiedPanel();

                // 입력/사운드 등은 UI 쪽에서 멈추므로 여기선 추가로 할 것 없음
                // 필요하면 총 자체를 비활성화해서 발사 로직이 더는 돌지 않게 할 수도 있음:
                // enabled = false;
            }
            return;
        }

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
        if (isReloading) yield break;
        isReloading = true;

        // ⏱ 리로드 시작 → UI 켜기
        if (reloadCircleObj) reloadCircleObj.SetActive(true);

        // 그냥 시간만 기다리면 됨 (애니메이션은 자동 재생)S
        yield return new WaitForSeconds(reloadTime);

        // 리로드 완료 → 탄창 회복
        currentAmmo = maxAmmo;
        isReloading = false;
        UIManager.Instance?.UpdateAmmoText(currentAmmo, maxAmmo);

        // ⏱ 리로드 끝 → UI 끄기
        if (reloadCircleObj) reloadCircleObj.SetActive(false);
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
