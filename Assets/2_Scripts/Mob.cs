using UnityEngine;

public class Mob : MonoBehaviour
{
    [Header("이동")]
    public float Speed = 7.5f;

    [Header("공격")]
    public int minDamage = 3;
    public int maxDamage = 5;
    public float attackCooldown = 1f;

    [Header("탐지 (발견 전까지만 사용)")]
    public float detectRadius = 4f;           // 근접 감지(청각/주변)
    public float viewDistance = 6f;           // 시야 거리
    [Range(0, 180)] public float fovAngle = 80f; // 좌우 합친 시야각
    public LayerMask obstacleMask;            // 벽/지형 레이어(가림 체크)

    [Header("참조")]
    public Rigidbody2D target;                // Player의 Rigidbody2D (인스펙터에 연결)

    private bool isLive = true;
    private bool hasSpotted = false;          // ★ 한 번 발견하면 계속 true
    private float nextAttackTime = 0f;        // 절대시간 기반 쿨다운
    private bool dealtThisFixed = false;      // 한 고정프레임 1회 가드

    private Rigidbody2D rigid;
    private SpriteRenderer spriter;

    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        spriter = GetComponent<SpriteRenderer>();
        isLive = true;
        hasSpotted = false;
    }

    void FixedUpdate()
    {
        dealtThisFixed = false;

        if (!isLive || target == null)
        {
            rigid.linearVelocity = Vector2.zero;
            return;
        }

        // 아직 안 봤다면 탐지 시도
        if (!hasSpotted && CanDetectPlayer())
        {
            hasSpotted = true; // ★ 이제부터 영구 추적
        }

        // 봤다면 계속 추적
        if (hasSpotted)
        {
            Vector2 dir = target.position - rigid.position;
            Vector2 step = dir.normalized * Speed * Time.fixedDeltaTime;
            rigid.MovePosition(rigid.position + step);
        }

        // 추적형이라 관성 제거
        rigid.linearVelocity = Vector2.zero;
    }

    void LateUpdate()
    {
        if (!isLive || target == null) return;
        // 좌우 방향만 사용 (원하면 이동 벡터로 교체 가능)
        spriter.flipX = target.position.x < rigid.position.x;
    }

    // 붙자마자 1회
    void OnCollisionEnter2D(Collision2D collision)
    {
        TryDealDamage(collision);
    }

    // 이후 쿨다운마다
    void OnCollisionStay2D(Collision2D collision)
    {
        TryDealDamage(collision);
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            // 다시 붙으면 즉시 타격 허용
            nextAttackTime = 0f;
        }
    }

    void TryDealDamage(Collision2D collision)
    {
        if (!isLive) return;
        if (!collision.collider.CompareTag("Player")) return;
        if (dealtThisFixed) return; // 같은 Fixed 프레임 중복 방지

        // 발견 이전엔 공격하지 않음(잠입 유지)
        if (!hasSpotted) return;

        if (Time.time < nextAttackTime) return;

        var player = collision.collider.GetComponentInParent<Player>();
        if (player == null) return;

        int dmg = Random.Range(minDamage, maxDamage + 1); // 3~5
        player.TakeDamage(dmg);

        nextAttackTime = Time.time + attackCooldown;
        dealtThisFixed = true;
    }

    // === 발견 로직(발견 전까지만 호출) ===
    bool CanDetectPlayer()
    {
        Vector2 myPos = rigid.position;
        Vector2 toPlayer = target.position - myPos;
        float dist = toPlayer.magnitude;

        // 1) 근접 반경: 가림만 안 되면 즉시 발견
        if (dist <= detectRadius)
            return !Blocked(myPos, target.position);

        // 2) 시야각 + 시야거리
        if (dist > viewDistance) return false;

        Vector2 forward = spriter.flipX ? Vector2.left : Vector2.right; // 간단한 정면
        float angle = Vector2.Angle(forward, toPlayer.normalized);
        if (angle > (fovAngle * 0.5f)) return false;

        // 3) 라인캐스트로 가림 체크
        return !Blocked(myPos, target.position);
    }

    bool Blocked(Vector2 from, Vector2 to)
    {
        var hit = Physics2D.Linecast(from, to, obstacleMask);
        return hit.collider != null;
    }

    // 디버그용
    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1, 0.6f, 0, 0.25f);
        Gizmos.DrawWireSphere(transform.position, detectRadius);

        Gizmos.color = new Color(0, 1, 0, 0.25f);
        Gizmos.DrawWireSphere(transform.position, viewDistance);

        // 대략적인 FOV 표시
        Vector2 forward = (spriter != null && spriter.flipX) ? Vector2.left : Vector2.right;
        float half = fovAngle * 0.5f;
        Vector2 left = Quaternion.Euler(0, 0, +half) * forward;
        Vector2 right = Quaternion.Euler(0, 0, -half) * forward;
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, (Vector2)transform.position + left * viewDistance);
        Gizmos.DrawLine(transform.position, (Vector2)transform.position + right * viewDistance);
    }

    public void OnDamage(float damage)
    {
        Debug.Log($"{name}이(가) {damage} 데미지");
        // TODO: 체력/사망 시 hasSpotted = false; isLive = false; 등 상태 전환
    }
}
