using UnityEngine;

public class Crosshair : MonoBehaviour
{
    public Sprite CrosshairSprite; // ����� Ŀ�� �̹���
    private SpriteRenderer spriteRenderer;
    private Camera mainCam;

    void Awake()
    {
        mainCam = Camera.main;
        spriteRenderer = GetComponent<SpriteRenderer>();

        Cursor.visible = false; // �⺻ Ŀ�� �����
        spriteRenderer.sprite = CrosshairSprite;
    }

    void Update()
    {
        // ���콺 ��ġ �� ���� ��ǥ
        Vector3 mousePos = mainCam.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0f;
        transform.position = mousePos;
    }
}
