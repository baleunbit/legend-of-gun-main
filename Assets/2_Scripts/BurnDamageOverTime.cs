// BurnDamageOverTime.cs
// - 방 내부에 있는 동안 플레이어에게 초당 dps 만큼 화상 피해
// - Room의 콜라이더 영역 안에 플레이어가 있을 때만 적용

using UnityEngine;

public class BurnDamageOverTime : MonoBehaviour
{
    [SerializeField] private float dps = 4f;
    private Transform player;
    private Room room;

    public void Init(Transform playerTr, float dpsValue)
    {
        player = playerTr;
        dps = dpsValue;
    }

    void Awake()
    {
        room = GetComponent<Room>();
        if (!room) room = GetComponentInParent<Room>();
    }

    void Update()
    {
        if (!enabled || !player || dps <= 0f || room == null) return;

        // 플레이어가 방 내부인지 판정
        if (!IsInsideRoom(player.position)) return;

        // 플레이어 체력 처리(사용중인 Health 시스템에 맞게 아래를 조정)
        var dmgRecv = player.GetComponent<IPlayerDamageReceiver>();
        if (dmgRecv != null)
        {
            dmgRecv.ApplyDamage(dps * Time.deltaTime);
        }
        else
        {
            // 예시: "PlayerHealth" 컴포넌트에 TakeDamage(float) 메서드가 있다면
            var any = player.GetComponent("PlayerHealth");
            if (any != null)
            {
                any.SendMessage("TakeDamage", dps * Time.deltaTime, SendMessageOptions.DontRequireReceiver);
            }
        }
    }

    private bool IsInsideRoom(Vector2 wp)
    {
        var cols = room.GetComponentsInChildren<Collider2D>(true);
        foreach (var c in cols) if (c && c.OverlapPoint(wp)) return true;
        return false;
    }
}

// 선택: 게임의 플레이어 데미지 시스템이 이 인터페이스를 구현하면 깔끔하게 작동
public interface IPlayerDamageReceiver
{
    void ApplyDamage(float amount);
}
