using UnityEngine;

public class Door : MonoBehaviour
{
    [SerializeField] RoomGenerator generator;
    [SerializeField] Transform player;
    [SerializeField] float activateDelay = 0.75f;
    [SerializeField] float reenterCooldown = 0.4f;
    [SerializeField] Vector2 exitOffset = new(0f, 0.75f);

    float startTime;
    bool requireExit;

    // 🔸 모든 문이 공유하는 전역 쿨다운
    static float nextGlobalAllowedTime = 0f;

    void Awake()
    {
        startTime = Time.time;
        if (!player) { var p = GameObject.FindGameObjectWithTag("Player"); if (p) player = p.transform; }
        if (!generator) generator = FindFirstObjectByType<RoomGenerator>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (Time.time - startTime < activateDelay) return;
        if (!player || !generator) return;

        // 🔒 전역 쿨다운 + 이 문에서 나갔다가 들어오기 전까지 1회만
        if (Time.time < nextGlobalAllowedTime) return;
        if (requireExit) return;

        // “바로 위 방”만 선택
        var next = generator.GetNextRoomPositionByY(player.position);
        if (Mathf.Approximately(next.y, player.position.y)) return;

        player.position = new Vector3(next.x + exitOffset.x, next.y + exitOffset.y, player.position.z);
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
}
