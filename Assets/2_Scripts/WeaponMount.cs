using UnityEngine;

public class WeaponMount : MonoBehaviour
{
    [Tooltip("���� ���� ���� ������(�ȼ� ��Ʈ�� 1����=1px�� ���缭)")]
    public Vector2 localOffset = Vector2.zero;

    [Tooltip("���� ���� �߰� ȸ��(��)")]
    public float localZRotation = 0f;
}
