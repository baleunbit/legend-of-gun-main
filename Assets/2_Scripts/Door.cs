using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Door : MonoBehaviour
{
    [Header("참조")]
    [SerializeField] private RoomGenerator generator;
    [SerializeField] private Transform player;

    [Header("동작")]
    [SerializeField] private float activateDelay = 0.75f;
    [SerializeField] private float reenterCooldown = 0.4f;
    [SerializeField] private Vector2 exitOffset = new(0f, 0.75f);

    [Header("조건")]
    [SerializeField] private bool requireClearRoom = true;
    [SerializeField] private string blockedMessage = "아직 적이 남아 있어!";

    [Header("애니메이션")]
    [SerializeField] private Animator doorAnimator;   // Bool "Open"
    [SerializeField] private float clearCheckInterval = 0.25f;

    float startTime; bool requireExit; static float nextGlobalAllowedTime = 0f;
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
        ownerRoom = FindTopmostParentRoom(transform);

        if (!doorAnimator) doorAnimator = GetComponent<Animator>();
        InvokeRepeating(nameof(UpdateOpenAnimation), 0.1f, clearCheckInterval);
    }

    void UpdateOpenAnimation()
    {
        if (!doorAnimator) return;
        bool isClear = IsRoomCleared(ownerRoom);
        doorAnimator.SetBool("Open", isClear);   // 전멸 시 열림
    }

    bool IsRoomCleared(Room room)
    {
        if (!room) return false;
        var mobs = room.GetComponentsInChildren<Mob>(true);
        foreach (var m in mobs) if (m && m.IsAlive) return false;
        return true;
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

        if (requireClearRoom && !IsRoomCleared(currentRoom))
        {
            Debug.Log(blockedMessage);
            return;
        }

        int curIndex = generator.FindChainIndexByRoom(currentRoom);
        if (curIndex < 0) { Debug.LogWarning("[Door] 현재 방 인덱스 못 찾음"); return; }

        var nextRoomGO = generator.GetChainedRoom(curIndex + 1);
        if (!nextRoomGO) { Debug.Log("[Door] 다음 방 없음"); return; }

        player.position = nextRoomGO.transform.position + (Vector3)exitOffset;

        var rb = player.GetComponent<Rigidbody2D>();
        if (rb) rb.linearVelocity = Vector2.zero;

        requireExit = true;
        nextGlobalAllowedTime = Time.time + reenterCooldown;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        requireExit = false;
    }

    // ── 유틸 ──
    Room FindTopmostParentRoom(Transform t)
    {
        Room found = null; Transform cur = t;
        while (cur) { var r = cur.GetComponent<Room>(); if (r) found = r; cur = cur.parent; }
        return found;
    }
    Room FindRoomByPosition(Vector2 pos)
    {
        var rooms = FindObjectsByType<Room>(FindObjectsSortMode.None);
        Room best = null; float bestDist = float.MaxValue;
        foreach (var r in rooms)
        {
            if (!r) continue;
            var cols = r.GetComponentsInChildren<Collider2D>(true);
            foreach (var c in cols) { if (c && c.OverlapPoint(pos)) return r; }
            float d = ((Vector2)r.transform.position - pos).sqrMagnitude;
            if (d < bestDist) { bestDist = d; best = r; }
        }
        return best;
    }
}
