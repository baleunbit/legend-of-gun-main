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
        Vector3 mousePos = mainCam.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0f;
        transform.position = mousePos;
    }
}
