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
    public float patrolSpeed = 3f;
    public float arriveDist = 0.05f;
    public float waitAtPoint = 0.4f;
    public bool pingPong = true;

    Rigidbody2D rb;
    Mob mob;
    Vector2[] waypoints;
    int idx = 0, dir = +1;
    float waitTimer = 0f;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        mob = GetComponent<Mob>(); // 있을 수도, 없을 수도

        BuildAutoPath();

        // 살짝 디싱크
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
            waypoints = new Vector2[] {
                c + new Vector2(-halfSize.x,  0f),
                c + new Vector2( 0f,          +halfSize.y),
                c + new Vector2(+halfSize.x,  0f),
                c + new Vector2( 0f,          -halfSize.y),
            };
        }
        else
        {
            waypoints = new Vector2[] {
                c + Vector2.left  * crossRadius,
                c + Vector2.up    * crossRadius,
                c + Vector2.right * crossRadius,
                c + Vector2.down  * crossRadius,
            };
        }
    }

    void FixedUpdate()
    {
        // 발각되면 순찰 중단(추격은 Mob이 함)
        if (mob != null && mob.IsAlerted) { rb.linearVelocity = Vector2.zero; return; }

        // 360° 근접(소리) 반응: 플레이어 쪽으로 "한 스텝"만
        if (mob != null && mob.IsPlayerInHearingRange())
        {
            Vector2 dirToPlayer = (mob.target.position - rb.position);
            Vector2 step = dirToPlayer.normalized * patrolSpeed * Time.fixedDeltaTime;
            rb.MovePosition(rb.position + step);
            rb.linearVelocity = Vector2.zero;
            mob.SetLook(dirToPlayer); // 시선 = 움직임 방향(8방향 스냅)
            return;
        }

        // ---- 기본 순찰 ----
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
            if (mob) mob.SetLook(to); // 시선 = 이동 방향(8방향 스냅)
        }
    }

    public void KillSilently()
    {
        if (!isLive) return;   // 이미 죽었으면 무시
        isLive = false;

        // 몹의 충돌/물리 비활성화
        var cols = GetComponentsInChildren<Collider2D>(true);
        foreach (var c in cols) if (c) c.enabled = false;
        if (rb) rb.simulated = false;

        // 바로 제거
        Destroy(gameObject);
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
