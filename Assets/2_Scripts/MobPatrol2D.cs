using UnityEngine;

/// 순찰 포인트 없이 자동 경로만 사용(사각형/십자).
/// 발견 전엔 순찰, 발견하면 Mob이 추격. Mob 없으면 그냥 순찰만.
[RequireComponent(typeof(Rigidbody2D))]
public class MobPatrolAuto2D : MonoBehaviour
{
    public enum AutoPattern { Rectangle, Cross }

    [Header("자동 경로")]
    public AutoPattern pattern = AutoPattern.Rectangle;
    public Vector2 halfSize = new(1.5f, 1.0f); // Rectangle 반폭/반높이
    public float crossRadius = 1.5f;           // Cross 반경

    [Header("이동/대기")]
    public float patrolSpeed = 3f;
    public float arriveDist = 0.05f;
    public float waitAtPoint = 0.4f;
    public bool pingPong = true;               // 왕복(true) / 루프(false)

    // 내부
    Rigidbody2D rb;
    SpriteRenderer sr;
    Mob mob;                 // 있을 수도, 없을 수도
    Vector2[] waypoints;     // 자동 생성 경로
    int idx = 0, dir = +1;
    float waitTimer = 0f;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponentInChildren<SpriteRenderer>(true);
        mob = GetComponent<Mob>(); // null 가능

        BuildAutoPath();

        // 디싱크(동일 움직임 방지)
        if (waypoints != null && waypoints.Length > 1)
            idx = Random.Range(0, waypoints.Length);
        dir = Random.value < 0.5f ? +1 : -1;
        waitTimer = Random.Range(0f, waitAtPoint);
        patrolSpeed *= Random.Range(0.9f, 1.1f);
    }
    void BuildAutoPath()
    {
        Vector2 c = transform.position;

        if (pattern == AutoPattern.Rectangle)
        {
            // 시계 방향 4점(좌→상→우→하)
            waypoints = new Vector2[]
            {
                c + new Vector2(-halfSize.x,  0f),
                c + new Vector2( 0f,          +halfSize.y),
                c + new Vector2(+halfSize.x,  0f),
                c + new Vector2( 0f,          -halfSize.y),
            };
        }
        else // Cross
        {
            waypoints = new Vector2[]
            {
                c + Vector2.left  * crossRadius,
                c + Vector2.up    * crossRadius,
                c + Vector2.right * crossRadius,
                c + Vector2.down  * crossRadius,
            };
        }
    }
    void FixedUpdate()
    {
        // Mob이 있고 플레이어를 발견했다면 순찰 중지
        if (mob != null && mob.IsAlerted) { rb.linearVelocity = Vector2.zero; return; }

        if (waypoints == null || waypoints.Length == 0) return;

        if (waitTimer > 0f)
        {
            waitTimer -= Time.fixedDeltaTime;
            rb.linearVelocity = Vector2.zero;
            return;
        }

        Vector2 cur = rb.position;
        Vector2 target = waypoints[Mathf.Clamp(idx, 0, waypoints.Length - 1)];
        Vector2 to = target - cur;
        float dist = to.magnitude;

        if (dist <= arriveDist)
        {
            AdvanceIndex();
            waitTimer = waitAtPoint;
            rb.linearVelocity = Vector2.zero;
        }
        else
        {
            Vector2 step = to.normalized * patrolSpeed * Time.fixedDeltaTime;
            rb.MovePosition(cur + step);
            rb.linearVelocity = Vector2.zero;

            // 좌우 이동할 때만 시선 전환(상하 이동 중엔 유지)
            if (sr && Mathf.Abs(step.x) > 0.001f)
                sr.flipX = step.x < 0f;
        }
    }
    void AdvanceIndex()
    {
        int len = waypoints.Length;
        if (len <= 1) return;

        if (pingPong)
        {
            idx += dir;
            if (idx >= len - 1) { idx = len - 1; dir = -1; }
            else if (idx <= 0) { idx = 0; dir = +1; }
        }
        else
        {
            idx = (idx + 1) % len;
        }
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        // 자동 경로 미리보기(선택)
        Gizmos.color = new Color(0, 1, 1, 0.25f);
        Vector2 c = transform.position;

        if (pattern == AutoPattern.Rectangle)
        {
            Vector2 L = c + new Vector2(-halfSize.x, 0);
            Vector2 R = c + new Vector2(+halfSize.x, 0);
            Vector2 U = c + new Vector2(0, +halfSize.y);
            Vector2 D = c + new Vector2(0, -halfSize.y);
            Gizmos.DrawSphere(L, 0.06f); Gizmos.DrawSphere(U, 0.06f);
            Gizmos.DrawSphere(R, 0.06f); Gizmos.DrawSphere(D, 0.06f);
            Gizmos.DrawLine(L, U); Gizmos.DrawLine(U, R);
            Gizmos.DrawLine(R, D); Gizmos.DrawLine(D, L);
        }
        else
        {
            Vector2 L = c + Vector2.left * crossRadius;
            Vector2 U = c + Vector2.up * crossRadius;
            Vector2 R = c + Vector2.right * crossRadius;
            Vector2 D = c + Vector2.down * crossRadius;
            Gizmos.DrawSphere(L, 0.06f); Gizmos.DrawSphere(U, 0.06f);
            Gizmos.DrawSphere(R, 0.06f); Gizmos.DrawSphere(D, 0.06f);
            Gizmos.DrawLine(L, R); Gizmos.DrawLine(U, D);
        }
    }
#endif
}