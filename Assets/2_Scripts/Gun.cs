using System.Collections;
using UnityEngine;

public class Gun : MonoBehaviour
{
    [Header("크로스헤어")]
    public Transform Crosshair;

    [Header("탄창")]
    public int maxAmmo = 20;
    public float reloadTime = 2f;
    private int currentAmmo;
    private bool isReloading = false;

    [Header("발사 속도")]
    public float fireRate = 0.2f;

    [Header("데미지/관통")]
    [SerializeField] float Damage = 5f;   // ← 인스펙터에서 5로 설정
    [SerializeField] int Pierce = 1;      // 1 = 관통 없음, 2 이상 = 그 수만큼 맞고 진행

    private float nextFireTime = 0.2f;
    private Camera mainCam;
    [SerializeField] private GameObject bulletPrefab;

    void Start()
    {
        mainCam = Camera.main;
        Cursor.visible = false;
        currentAmmo = maxAmmo;
        UIManager.Instance.UpdateAmmoText(currentAmmo, maxAmmo);
    }

    void Update()
    {
        if (isReloading) return;

        if (Input.GetKeyDown(KeyCode.R)) { StartCoroutine(Reload()); return; }

        if (Input.GetMouseButton(0) && Time.time >= nextFireTime)
        {
            if (currentAmmo > 0)
            {
                Fire();
                nextFireTime = Time.time + fireRate;
            }
            else
            {
                Debug.Log("탄약 부족! 재장전 필요 (R 키)");
                StartCoroutine(Reload());
            }
        }
    }

    IEnumerator Reload()
    {
        isReloading = true;
        yield return new WaitForSeconds(reloadTime);
        currentAmmo = maxAmmo;
        UIManager.Instance.UpdateAmmoText(currentAmmo, maxAmmo);
        isReloading = false;
    }

    void Fire()
    {
        currentAmmo--;
        UIManager.Instance.UpdateAmmoText(currentAmmo, maxAmmo);

        Vector3 playerPos = transform.position;
        Vector3 crosshairPos = Crosshair.position;
        Vector2 direction = (crosshairPos - playerPos).normalized;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion bulletRotation = Quaternion.AngleAxis(angle + 270f, Vector3.forward);

        GameObject bulletObj = Instantiate(bulletPrefab, playerPos, bulletRotation);
        var b = bulletObj.GetComponent<Bullet>();

        // 🔴 핵심: 데미지/관통 값 주입
        b.Init(Damage, Pierce, direction);
        b.Setup(direction);

        // Destroy(bulletObj, 1f);  // ← Bullet.Setup에서 이미 수명 처리하므로 중복 제거 권장
    }
}
