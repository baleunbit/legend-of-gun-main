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
    [SerializeField] bool requireClearRoom = true;                 // 현재 방 전멸 필요
    [SerializeField] string blockedMessage = "아직 적이 남아 있어!";   // 안내 로그

    float startTime;
    bool requireExit;
    static float nextGlobalAllowedTime = 0f; // 모든 문 공유 쿨다운

    // 이 문이 “속한” 방(문을 방의 자식으로 배치하면 자동 인식)
    Room ownerRoom;

    void Awake()
    {
        startTime = Time.time;

        if (!player)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p) player = p.transform;
        }
        if (!generator) generator = FindFirstObjectByType<RoomGenerator>();

        // 문이 어느 방에 속했는지 우선적으로 파악(가장 정확)
        ownerRoom = GetComponentInParent<Room>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (!player || !generator) return;
        if (Time.time - startTime < activateDelay) return;

        // 전역 쿨다운 + 재진입 방지
        if (Time.time < nextGlobalAllowedTime) return;
        if (requireExit) return;

        // 1) 현재 방 결정: 문이 방 자식이면 그 방, 아니면 플레이어 위치 기준으로 찾기
        Room currentRoom = ownerRoom ? ownerRoom : FindRoomByPosition(player.position);

        // 2) 전멸 조건 체크
        if (requireClearRoom && currentRoom != null && HasAliveMobs(currentRoom))
        {
            Debug.Log(blockedMessage);
            return;
        }

        // 3) 바로 위 방으로만 이동
        var next = generator.GetNextRoomPositionByY(player.position);
        if (Mathf.Approximately(next.y, player.position.y)) return;

        player.position = new Vector3(
            next.x + exitOffset.x,
            next.y + exitOffset.y,
            player.position.z
        );

        requireExit = true;
        nextGlobalAllowedTime = Time.time + reenterCooldown;

        var rb = player.GetComponent<Rigidbody2D>();
        if (rb) rb.linearVelocity = Vector2.zero;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        requireExit = false;
    }

    // === 유틸 ===

    // 플레이어 위치가 들어있는 방 우선, 없으면 가장 가까운 방
    Room FindRoomByPosition(Vector2 pos)
    {
        var rooms = FindObjectsByType<Room>(FindObjectsSortMode.None);
        Room bestByDistance = null;
        float bestSqr = float.MaxValue;

        foreach (var r in rooms)
        {
            if (!r) continue;

            // 방의 콜라이더로 실제 포함 여부 확인
            var cols = r.GetComponentsInChildren<Collider2D>(true);
            foreach (var c in cols)
            {
                if (c && c.OverlapPoint(pos))
                    return r;
            }

            // 포함 방이 없으면 가장 가까운 방 후보 저장
            float d = ((Vector2)r.transform.position - pos).sqrMagnitude;
            if (d < bestSqr) { bestSqr = d; bestByDistance = r; }
        }
        return bestByDistance;
    }

    // 방 자식 중 살아있는 몹 있는지
    bool HasAliveMobs(Room room)
    {
        var mobs = room.GetComponentsInChildren<Mob>(true);
        foreach (var m in mobs)
        {
            if (!m) continue;
            // 네 Mob이 IsAlive 프로퍼티 있죠? 있으면 그걸 신뢰
            if (m.IsAlive) return true;

            // 혹시 모를 안전망: 아직 게임오브젝트가 살아있으면 적으로 간주
            if (m.gameObject.activeInHierarchy) return true;
        }
        return false;
    }
}
