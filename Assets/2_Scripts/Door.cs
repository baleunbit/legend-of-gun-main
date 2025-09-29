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
        if (!generator) generator = FindFirstObjectByType<RoomGenerator>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (Time.time - startTime < activateDelay) return;
        if (!player || !generator) return;

        // 전역 쿨다운 + 이 문에서 나갔다가 다시 들어오기 전까지 1회만
        if (Time.time < nextGlobalAllowedTime) return;
        if (requireExit) return;

        // “바로 위 방”만 선택
        var next = generator.GetNextRoomPositionByY(player.position);
        if (Mathf.Approximately(next.y, player.position.y)) return;

        // 이동
        player.position = new Vector3(next.x + exitOffset.x, next.y + exitOffset.y, player.position.z);
        requireExit = true;

        // 전역 쿨다운 갱신
        nextGlobalAllowedTime = Time.time + reenterCooldown;

        // 속도 정지
        var rb = player.GetComponent<Rigidbody2D>();
        if (rb) rb.linearVelocity = Vector2.zero;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        requireExit = false;
    }
}
