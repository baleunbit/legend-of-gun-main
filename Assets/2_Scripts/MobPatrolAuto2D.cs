using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class MobPatrolAuto2D : MonoBehaviour
{
    public enum AutoPattern { Rectangle, Cross }

    [Header("자동 경로")]
    public AutoPattern pattern = AutoPattern.Rectangle;
    public Vector2 halfSize = new(1.5f, 1.0f);
    public float crossRadius = 1.5f;

    [Header("이동/대기")]
    public float patrolSpeed = 3f;       // 순찰 속도
    public float arriveDist = 0.05f;
    public float waitAtPoint = 0.4f;
    public bool pingPong = true;

    [Header("의심 접근(발각 전)")]
    public bool approachOnProximity = true; // 외부 감지에서 한 스텝 다가갈지
    public float suspicionSpeed = 2.0f;     // 🔸 의심 속도(발각 전) — 여기만 조절하면 됨

    Rigidbody2D rb;
    SpriteRenderer sr;
    Mob mob;
    Vector2[] waypoints;
    int idx = 0, dir = +1;
    float waitTimer = 0f;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponentInChildren<SpriteRenderer>(true);
        mob = GetComponent<Mob>();

        BuildAutoPath();

        if (waypoints != null && waypoints.Length > 1)
        {
            idx = Random.Range(0, waypoints.Length);
            dir = Random.value < 0.5f ? +1 : -1;
            waitTimer = Random.Range(0f, waitAtPoint);
            patrolSpeed *= Random.Range(0.9f, 1.1f);
        }
    }

    void BuildAutoPath()
    {
        Vector2 c = transform.position;
        if (pattern == AutoPattern.Rectangle)
        {
            waypoints = new Vector2[]
            {
                c + new Vector2(-halfSize.x,  0f),
                c + new Vector2( 0f,          +halfSize.y),
                c + new Vector2(+halfSize.x,  0f),
                c + new Vector2( 0f,          -halfSize.y),
            };
        }
        else
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
        // ✅ 발각되면 순찰 중지 (추격은 Mob에서 Speed로)
        if (mob != null && mob.IsAlerted)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        // ✅ 발각 전 + 외부 감지 범위 → "의심 속도"로 한 스텝만 접근
        if (approachOnProximity && mob != null && mob.target != null)
        {
            float dist = Vector2.Distance(rb.position, mob.target.position);
            if (dist <= mob.detectRadius)
            {
                Vector2 dirToPlayer = (mob.target.position - rb.position);
                if (dirToPlayer.sqrMagnitude > 0.0001f)
                {
                    // 🔸 여기서 suspicionSpeed 사용! (절대 Speed 사용 금지)
                    Vector2 step = dirToPlayer.normalized * suspicionSpeed * Time.fixedDeltaTime;
                    rb.MovePosition(rb.position + step);
                    rb.linearVelocity = Vector2.zero;

                    if (sr && Mathf.Abs(step.x) > 0.001f)
                        sr.flipX = step.x < 0f;

                    return; // 이번 프레임은 순찰 스킵
                }
            }
        }

        // 기본 순찰
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
        float distToWp = to.magnitude;

        if (distToWp <= arriveDist)
        {
            AdvanceIndex();
            waitTimer = waitAtPoint;
            rb.linearVelocity = Vector2.zero;
        }
        else
        {
            Vector2 step = to.normalized * patrolSpeed * Time.fixedDeltaTime; // 순찰 속도
            rb.MovePosition(cur + step);
            rb.linearVelocity = Vector2.zero;

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
}
