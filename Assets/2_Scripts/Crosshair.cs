using UnityEngine;

public class Crosshair : MonoBehaviour
{
    public Sprite CrosshairSprite; // ����� Ŀ�� �̹���
    private SpriteRenderer spriteRenderer;
    private Camera mainCam;

    private void Awake()
    {
        mainCam = Camera.main;
        spriteRenderer = GetComponent<SpriteRenderer>();

        Cursor.visible = false; // �⺻ Ŀ�� �����
        spriteRenderer.sprite = CrosshairSprite;
    }

    private void Update()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = 10f;
        Vector3 worldPos = mainCam.ScreenToWorldPoint(mousePos);

        transform.position = new Vector3(worldPos.x, worldPos.y, 0f);
    }
}
