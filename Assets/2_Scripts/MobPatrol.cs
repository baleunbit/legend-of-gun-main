using UnityEngine;

/// 발견 전까지 2D 순찰(좌우+상하). 발견하면 Mob.cs가 추격.
/// - points 지정 시: 그 순서대로 이동(왕복/루프 선택)
/// - points 미지정 시: 자동 사각형(또는 십자) 패턴
[RequireComponent(typeof(Mob)), RequireComponent(typeof(Rigidbody2D))]
public class MobPatrol2D : MonoBehaviour
{
    [Header("순찰 포인트(선택)")]
    public Transform[] points;
    public bool pingPong = true;

    // ✅ enum 위에서는 Header 제거
    public enum AutoPattern { Rectangle, Cross }

    [Header("자동 경로(포인트가 없을 때)")]
    public AutoPattern pattern = AutoPattern.Rectangle;
    public Vector2 halfSize = new Vector2(1.5f, 1.0f);
    public float crossRadius = 1.5f;
    
    [Header("이동/대기")]
    public float patrolSpeed = 3f;
    public float arriveDist = 0.05f;
    public float waitAtPoint = 0.4f;

    Mob mob;
    Rigidbody2D rb;
    SpriteRenderer sr;

    // 내부 상태
    int idx = 0;        // 현재 타겟 인덱스
    int dir = +1;       // 진행 방향(핑퐁용)
    float waitTimer = 0f;

    // 자동 경로
    Vector2[] autoPath;
    bool useAuto = false;

    void Awake()
    {
        mob = GetComponent<Mob>();
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponentInChildren<SpriteRenderer>();

        if (points == null || points.Length < 2)
        {
            useAuto = true;
            BuildAutoPath();
        }
    }

    void BuildAutoPath()
    {
        Vector2 c = transform.position;

        if (pattern == AutoPattern.Rectangle)
        {
            // 시계 방향 사각형 4점
            autoPath = new Vector2[4];
            autoPath[0] = c + new Vector2(-halfSize.x, 0f);             // 좌
            autoPath[1] = c + new Vector2(0f, +halfSize.y);    // 상
            autoPath[2] = c + new Vector2(+halfSize.x, 0f);             // 우
            autoPath[3] = c + new Vector2(0f, -halfSize.y);    // 하
        }
        else // Cross
        {
            // 십자 4점
            autoPath = new Vector2[4];
            autoPath[0] = c + Vector2.left * crossRadius;
            autoPath[1] = c + Vector2.up * crossRadius;
            autoPath[2] = c + Vector2.right * crossRadius;
            autoPath[3] = c + Vector2.down * crossRadius;
        }
    }

    void FixedUpdate()
    {
        // 들키면 순찰 중지 → Mob.cs 가 추격 담당
        if (mob.IsAlerted) { rb.linearVelocity = Vector2.zero; return; }

        // 대기
        if (waitTimer > 0f)
        {
            waitTimer -= Time.fixedDeltaTime;
            rb.linearVelocity = Vector2.zero;
            return;
        }

        Vector2 targetPos = GetCurrentTarget();
        Vector2 cur = rb.position;
        Vector2 to = targetPos - cur;
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

            // 좌우 시선 전환 (위/아래는 flipX 유지)
            if (sr && Mathf.Abs(step.x) > 0.001f)
                sr.flipX = step.x < 0f;
        }
    }

    Vector2 GetCurrentTarget()
    {
        if (useAuto)
            return autoPath[Mathf.Clamp(idx, 0, autoPath.Length - 1)];
        else
            return points[Mathf.Clamp(idx, 0, points.Length - 1)].position;
    }

    void AdvanceIndex()
    {
        if (useAuto)
        {
            StepIndex(autoPath.Length);
            return;
        }
        StepIndex(points.Length);
    }

    void StepIndex(int length)
    {
        if (length <= 1) return;
        if (pingPong)
        {
            idx += dir;
            if (idx >= length - 1) { idx = length - 1; dir = -1; }
            else if (idx <= 0) { idx = 0; dir = +1; }
        }
        else
        {
            idx = (idx + 1) % length;
        }
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0, 1, 1, 0.25f);

        if (points != null && points.Length > 0)
        {
            for (int i = 0; i < points.Length; i++)
            {
                if (!points[i]) continue;
                Gizmos.DrawSphere(points[i].position, 0.07f);
                if (i < points.Length - 1 && points[i + 1])
                    Gizmos.DrawLine(points[i].position, points[i + 1].position);
            }
            if (!pingPong && points.Length > 2 && points[0] && points[^1])
                Gizmos.DrawLine(points[^1].position, points[0].position);
        }
        else
        {
            // 자동 경로 미리보기
            Vector2 c = Application.isPlaying ? (Vector2)transform.position : (Vector2)transform.position;
            if (pattern == AutoPattern.Rectangle)
            {
                Vector2 L = c + new Vector2(-halfSize.x, 0);
                Vector2 R = c + new Vector2(+halfSize.x, 0);
                Vector2 U = c + new Vector2(0, +halfSize.y);
                Vector2 D = c + new Vector2(0, -halfSize.y);
                Gizmos.DrawSphere(L, 0.07f); Gizmos.DrawSphere(U, 0.07f);
                Gizmos.DrawSphere(R, 0.07f); Gizmos.DrawSphere(D, 0.07f);
                Gizmos.DrawLine(L, U); Gizmos.DrawLine(U, R);
                Gizmos.DrawLine(R, D); Gizmos.DrawLine(D, L);
            }
            else
            {
                Vector2 L = c + Vector2.left * crossRadius;
                Vector2 U = c + Vector2.up * crossRadius;
                Vector2 R = c + Vector2.right * crossRadius;
                Vector2 D = c + Vector2.down * crossRadius;
                Gizmos.DrawSphere(L, 0.07f); Gizmos.DrawSphere(U, 0.07f);
                Gizmos.DrawSphere(R, 0.07f); Gizmos.DrawSphere(D, 0.07f);
                Gizmos.DrawLine(L, R); Gizmos.DrawLine(U, D);
            }
        }
    }
#endif
}