using UnityEngine;

public class WeaponMount : MonoBehaviour
{
    [Tooltip("소켓 기준 로컬 오프셋(픽셀 아트면 1유닛=1px에 맞춰서)")]
    public Vector2 localOffset = Vector2.zero;

    [Tooltip("소켓 기준 추가 회전(도)")]
    public float localZRotation = 0f;
}
