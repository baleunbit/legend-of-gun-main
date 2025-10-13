// Door.cs
// - 다음 방 이동 + 스테이지 적용 호출(강제 보장)

using UnityEngine;

[DisallowMultipleComponent]
public class Door : MonoBehaviour
{
    [Header("참조")]
    [SerializeField] private RoomGenerator generator;
    [SerializeField] private Transform player;

    [Header("문 동작")]
    [SerializeField] private float activateDelay = 0.75f;
    [SerializeField] private float reenterCooldown = 0.4f;
    [SerializeField] private Vector2 exitOffset = new(0f, 0.75f);

    [Header("조건")]
    [SerializeField] private bool requireClearRoom = true;
    [SerializeField] private string blockedMessage = "아직 적이 남아 있어!";

    private float startTime;
    private bool requireExit;
    private static float nextGlobalAllowedTime = 0f;

    private Room ownerRoom;

    void Awake()
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

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (!player || !generator) return;

        if (Time.time - startTime < activateDelay) return;
        if (Time.time < nextGlobalAllowedTime) return;
        if (requireExit) return;

        Room currentRoom = ownerRoom ? ownerRoom : FindRoomByPosition(player.position);
        if (!currentRoom) return;

        if (requireClearRoom && HasAliveMobs(currentRoom))
        {
            Debug.Log(blockedMessage);
            return;
        }

        int curIndex = generator.FindChainIndexByRoom(currentRoom);
        if (curIndex < 0) return;

        GameObject nextRoomGO = generator.GetChainedRoom(curIndex + 1);
        if (!nextRoomGO) return;

        Room nextRoom = nextRoomGO.GetComponent<Room>();
        if (nextRoom == currentRoom) return;

        player.position = nextRoomGO.transform.position + (Vector3)exitOffset;
        var rb = player.GetComponent<Rigidbody2D>();
        if (rb) rb.linearVelocity = Vector2.zero;

        int stage = StageDirector.ParseStageFromName(nextRoomGO.name);
        StageDirector.Instance.ApplyStage(stage, nextRoomGO, player.gameObject);

        requireExit = true;
        nextGlobalAllowedTime = Time.time + reenterCooldown;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        requireExit = false;
    }

    private Room FindTopmostParentRoom(Transform t)
    {
        Room found = null;
        for (var cur = t; cur != null; cur = cur.parent)
        {
            var r = cur.GetComponent<Room>();
            if (r) found = r;
        }
        return found;
    }

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
                if (c && c.OverlapPoint(pos)) return r;

            float d = ((Vector2)r.transform.position - pos).sqrMagnitude;
            if (d < bestDist) { bestDist = d; best = r; }
        }
        return best;
    }

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
