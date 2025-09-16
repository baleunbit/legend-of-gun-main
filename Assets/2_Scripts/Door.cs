using UnityEngine;

public class Door : MonoBehaviour
{
    [Header("참조")]
    [SerializeField] RoomGenerator generator;
    [SerializeField] Transform player;

    [Header("문 동작")]
    [SerializeField] float activateDelay = 0.75f;
    [SerializeField] float reenterCooldown = 0.4f;
    [SerializeField] Vector2 exitOffset = new(0f, 0.75f);

    [Header("조건")]
    [SerializeField] bool requireClearRoom = true;                 // 현재 방 전멸 필요 여부
    [SerializeField] string blockedMessage = "아직 적이 남아 있어!";   // 안내 로그(옵션)

    float startTime;
    bool requireExit;

    // 모든 문이 공유하는 전역 쿨다운
    static float nextGlobalAllowedTime = 0f;

    void Awake()
    {
        startTime = Time.time;

        if (!player)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p) player = p.transform;
        }

        if (!generator)
            generator = FindFirstObjectByType<RoomGenerator>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (Time.time - startTime < activateDelay) return;
        if (!player || !generator) return;

        // 전역 쿨다운 + 이 문에서 나갔다가 다시 들어오기 전까지 1회만
        if (Time.time < nextGlobalAllowedTime) return;
        if (requireExit) return;

        // 현재 방 전멸 체크 (Room에 아무 메서드도 추가 안 함)
        if (requireClearRoom)
        {
            var currentRoom = FindRoomByPosition(player.position);
            if (currentRoom != null && HasAliveMobs(currentRoom))
            {
                Debug.Log(blockedMessage);
                return;
            }
        }

        // “바로 위 방”만 선택
        var next = generator.GetNextRoomPositionByY(player.position);
        if (Mathf.Approximately(next.y, player.position.y)) return;

        player.position = new Vector3(
            next.x + exitOffset.x,
            next.y + exitOffset.y,
            player.position.z
        );
        requireExit = true;

        // 전역 쿨다운 갱신 → 다른 문도 당분간 발동 안 함
        nextGlobalAllowedTime = Time.time + reenterCooldown;

        var rb = player.GetComponent<Rigidbody2D>();
        if (rb) rb.linearVelocity = Vector2.zero;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        requireExit = false;
    }

    // === 현재 위치에 가장 맞는 Room 찾기 ===
    // 1) 방의 콜라이더(자식 포함) 중 OverlapPoint 되는 게 있으면 그 방 리턴
    // 2) 없으면 예외적으로 가장 가까운 방 리턴
    Room FindRoomByPosition(Vector2 pos)
    {
        var rooms = FindObjectsByType<Room>(FindObjectsSortMode.None);
        Room bestByDistance = null;
        float bestSqr = float.MaxValue;

        foreach (var r in rooms)
        {
            if (r == null) continue;

            // 콜라이더로 실제 포함 여부 확인 (Room에 메서드 추가 필요 없음)
            var cols = r.GetComponentsInChildren<Collider2D>(true);
            foreach (var c in cols)
            {
                if (c != null && c.OverlapPoint(pos))
                    return r; // 실제 그 방 안에 있음
            }

            // 못 찾으면 가장 가까운 방 후보 기억
            float d = ((Vector2)r.transform.position - pos).sqrMagnitude;
            if (d < bestSqr)
            {
                bestSqr = d;
                bestByDistance = r;
            }
        }

        return bestByDistance; // 그래도 없으면 null
    }

    // === 방 안에 살아있는 몹이 있는지 ===
    bool HasAliveMobs(Room room)
    {
        // 스포너가 적을 room의 자식으로 넣고 있으므로 정확히 걸린다.
        var mobs = room.GetComponentsInChildren<Mob>(true);
        for (int i = 0; i < mobs.Length; i++)
        {
            var m = mobs[i];
            if (m == null) continue;

            // 일반적으로 몹 사망 시 Destroy(gameObject) 하므로
            // 컴포넌트가 남아 있고 활성 상태면 "살아있다"고 본다.
            // (네 Mob에 IsAlive 프로퍼티가 있다면 그걸 먼저 체크해도 됨)
            // 예: if (m.IsAlive) return true;
            if (m.gameObject.activeInHierarchy) return true;
        }
        return false;
    }
}
