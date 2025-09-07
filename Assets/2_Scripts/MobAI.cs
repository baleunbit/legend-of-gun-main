using UnityEngine;

public enum EnemyState { Idle, Alert, Chase }

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyAI : MonoBehaviour
{
    [Header("Refs")]
    public Transform eye;                 // 왜: 정확한 시야 발사 원점 제어
    public LayerMask obstacleMask;        // 왜: 시야 차단 레이어 분리

    [Header("Vision")]
    [Range(0f, 20f)] public float visionRange = 6f;
    [Range(0f, 360f)] public float visionAngle = 90f;
    public float loseSightDelay = 1.0f;   // 왜: 순간 이탈에 바로 풀리지 않게 버퍼

    [Header("Behaviour")]
    public float moveSpeed = 2.2f;
    public float stoppingDistance = 0.9f;

    public EnemyState state { get; private set; } = EnemyState.Idle;
    public bool IsAlerted => state != EnemyState.Idle;

    Transform _player;
    Rigidbody2D _rb;
    float _lastSeenTime = -999f;

    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        var p = GameObject.FindGameObjectWithTag("Player");
        if (p) _player = p.transform;
    }

    void Update()
    {
        if (!_player) return;

        bool canSee = CanSeePlayer();
        if (canSee)
        {
            _lastSeenTime = Time.time;
            state = state == EnemyState.Idle ? EnemyState.Alert : EnemyState.Chase;
        }
        else if (Time.time - _lastSeenTime > loseSightDelay)
        {
            state = EnemyState.Idle;
        }

        if (state == EnemyState.Chase)
        {
            Vector2 dir = (_player.position - transform.position);
            float dist = dir.magnitude;
            if (dist > stoppingDistance)
            {
                Vector2 step = dir.normalized * moveSpeed * Time.deltaTime;
                _rb.MovePosition(_rb.position + step);
            }
        }
    }

    bool CanSeePlayer()
    {
        Vector2 toPlayer = (Vector2)(_player.position - (eye ? eye.position : transform.position));
        float dist = toPlayer.magnitude;
        if (dist > visionRange) return false;

        // 각도 체크
        float angle = Vector2.Angle((transform.up), toPlayer); // 본체 up을 전방으로 가정
        if (angle > visionAngle * 0.5f) return false;

        // 가시선 차단 체크
        Vector2 origin = eye ? (Vector2)eye.position : (Vector2)transform.position;
        RaycastHit2D hit = Physics2D.Raycast(origin, toPlayer.normalized, dist, obstacleMask);
        if (hit.collider != null) return false; // 차단

        return true;
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Vector3 o = eye ? eye.position : transform.position;
        Gizmos.color = IsAlerted ? Color.red : Color.yellow;
        UnityEditor.Handles.color = Gizmos.color;
        UnityEditor.Handles.DrawWireArc(o, Vector3.forward, Quaternion.Euler(0, 0, -visionAngle * 0.5f) * transform.up, visionAngle, visionRange);
        Gizmos.DrawLine(o, o + (Quaternion.Euler(0, 0, -visionAngle * 0.5f) * transform.up) * visionRange);
        Gizmos.DrawLine(o, o + (Quaternion.Euler(0, 0, visionAngle * 0.5f) * transform.up) * visionRange);
    }
#endif
}