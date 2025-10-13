using UnityEngine;

[DisallowMultipleComponent]
public class Door : MonoBehaviour
{
    [Header("참조")]
    [SerializeField] private RoomGenerator generator;
    [SerializeField] private Transform player;

    [Header("문 동작")]
    [SerializeField] private float activateDelay = 0.75f;   // 생성 직후 오동작 방지
    [SerializeField] private float reenterCooldown = 0.4f;  // 연속 텔레포트 방지
    [SerializeField] private Vector2 exitOffset = new(0f, 0.75f);

    [Header("조건")]
    [SerializeField] private bool requireClearRoom = true;
    [SerializeField] private string blockedMessage = "아직 적이 남아 있어!";

    private float startTime;
    private bool requireExit;
    private static float nextGlobalAllowedTime = 0f;

    // 이 문이 속한 "최상위" Room (문 자신에 Room이 붙어있어도 부모 중 최상위로 고정)
    private Room ownerRoom;

    // ─────────────────────────────────────────────────────────────

    private void Awake()
    {
        startTime = Time.time;

        if (!player)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p) player = p.transform;
        }
        if (!generator) generator = FindFirstObjectByType<RoomGenerator>();

        ownerRoom = FindTopmostParentRoom(transform);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (!player || !generator) return;

        // 초기 지연/쿨다운/Exit 대기
        if (Time.time - startTime < activateDelay) return;
        if (Time.time < nextGlobalAllowedTime) return;
        if (requireExit) return;

        // 현재 방 결정: 최상위 부모 Room 우선, 없으면 플레이어 위치로 탐색
        Room currentRoom = ownerRoom != null ? ownerRoom : FindRoomByPosition(player.position);
        if (!currentRoom) return;

        // 방 전멸 조건
        if (requireClearRoom && HasAliveMobs(currentRoom))
        {
            Debug.Log(blockedMessage);
            return;
        }

        // 체인에서 현재 방 인덱스 찾기
        int curIndex = generator.FindChainIndexByRoom(currentRoom);
        if (curIndex < 0)
        {
            Debug.LogWarning("[Door] 현재 방 인덱스를 찾지 못함");
            return;
        }

        // 다음 방
        GameObject nextRoomGO = generator.GetChainedRoom(curIndex + 1);
        if (!nextRoomGO)
        {
            Debug.Log("[Door] 다음 방이 없습니다 (마지막 방)");
            return;
        }

        Room nextRoom = nextRoomGO.GetComponent<Room>();
        if (nextRoom == currentRoom)
        {
            // 자기 자신을 다음 방으로 잘못 인식했을 때 보호
            Debug.Log("[Door] next == current (이동 취소)");
            return;
        }

        // === 이동 처리 ===
        Vector3 target = nextRoomGO.transform.position + (Vector3)exitOffset;
        player.position = target;

        var rb = player.GetComponent<Rigidbody2D>();
        if (rb) rb.linearVelocity = Vector2.zero;

        // 재진입 방지 쿨다운
        requireExit = true;
        nextGlobalAllowedTime = Time.time + reenterCooldown;

        // 문을 삭제/비활성화하지 않음 (요청대로)
        // gameObject.SetActive(false);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        requireExit = false;
    }

    // ───────────────── 유틸 ─────────────────

    // 부모 계층에서 "최상위" Room을 찾는다 (문 자신에 Room이 붙어있어도 루트 Room으로 고정)
    private Room FindTopmostParentRoom(Transform t)
    {
        Room found = null;
        Transform cur = t;
        while (cur != null)
        {
            var r = cur.GetComponent<Room>();
            if (r) found = r;       // 더 위로 올라가며 갱신 → 최상위가 남음
            cur = cur.parent;
        }
        return found;
    }

    // 플레이어 위치 기준 방 탐색 (콜라이더 포함 확인 → 없으면 가장 가까운 방)
    private Room FindRoomByPosition(Vector2 pos)
    {
        var rooms = FindObjectsByType<Room>(FindObjectsSortMode.None);
        Room best = null;
        float bestDist = float.MaxValue;

        foreach (var r in rooms)
        {
            if (!r) continue;

            var cols = r.GetComponentsInChildren<Collider2D>(true);
            foreach (var c in cols)
            {
                if (c && c.OverlapPoint(pos))
                    return r;
            }

            float d = ((Vector2)r.transform.position - pos).sqrMagnitude;
            if (d < bestDist) { bestDist = d; best = r; }
        }
        return best;
    }

    // 방 내에 살아있는 몹이 남았는지 체크
    private bool HasAliveMobs(Room room)
    {
        var mobs = room.GetComponentsInChildren<Mob>(true);
        foreach (var m in mobs)
        {
            if (!m) continue;
            if (m.IsAlive) return true;
            if (m.gameObject.activeInHierarchy) return true;
        }
        return false;
    }
}
