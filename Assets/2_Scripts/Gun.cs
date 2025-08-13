using System.Collections;
using UnityEngine;

public class Gun : MonoBehaviour
{
    [Header("�� ����")]
    public Transform Crosshair; // �ѱ� ��ġ

    [Header("źâ ����")]
    public int maxAmmo = 30;          // �ִ� źȯ ��
    public float reloadTime = 2f;    // ������ �ð�
    private int currentAmmo;
    private bool isReloading = false;

    private Camera mainCam;

    private void Start()
    {
        mainCam = Camera.main;
        Cursor.visible = false;
        currentAmmo = maxAmmo;
        UIManager.Instance.UpdateAmmoText(currentAmmo, maxAmmo);
    }

    private void Update()
    {
        if (isReloading)
            return;

        // R Ű�� ������
        if (Input.GetKeyDown(KeyCode.R))
        {
            StartCoroutine(Reload());
            return;
        }

        // ��Ŭ�� �߻� - ź���� ���� ����
        if (Input.GetMouseButtonDown(0))
        {
            if (currentAmmo > 0)
            {
                Fire();
            }
            else
            {
                Debug.Log("źȯ ����! ������ �ʿ� (R Ű)");
            }
        }
    }

    private IEnumerator Reload()
    {
        isReloading = true;
        Debug.Log("������ ��...");
        yield return new WaitForSeconds(reloadTime);
        currentAmmo = maxAmmo;
        UIManager.Instance.UpdateAmmoText(currentAmmo, maxAmmo);
        isReloading = false;
        Debug.Log("������ �Ϸ�!");
    }

    private void Fire()
    {
        currentAmmo--;
        UIManager.Instance.UpdateAmmoText(currentAmmo, maxAmmo);

        Vector3 mouseWorldPos = mainCam.ScreenToWorldPoint(Input.mousePosition);
        Vector2 direction = (mouseWorldPos - Crosshair.position).normalized;

        // ����ĳ��Ʈ (ù ��° �浹ü�� Ȯ��)
        RaycastHit2D hit = Physics2D.Raycast(Crosshair.position, direction, 100f);
        if (hit.collider != null && hit.collider.gameObject != gameObject)
        {
            Debug.Log("Hit: " + hit.collider.name);
        }

        // �Ѿ� ���� ǥ��
        GameObject trailObj = new GameObject("BulletTrail");
        LineRenderer lr = trailObj.AddComponent<LineRenderer>();

        lr.positionCount = 2;
        lr.SetPosition(0, Crosshair.position);
        lr.SetPosition(1, Crosshair.position + (Vector3)(direction * 100f));

        lr.startWidth = 0.05f;
        lr.endWidth = 0.05f;

        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startColor = Color.gray;
        lr.endColor = Color.gray;
        lr.sortingOrder = 10;

        Destroy(trailObj, 0.05f);
    }
}
