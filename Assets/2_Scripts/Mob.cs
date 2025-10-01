using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(SpriteRenderer))]
public class Mob : MonoBehaviour
{
    [Header("이동/추격")]
    public float Speed = 7f;

    [Header("공격")]
    public int minDamage = 3;
    public int maxDamage = 5;
    public float attackCooldown = 1f;

    [Header("탐지")]
    public float detectRadius = 4f;    // 근접 범위 (의심)
    public float viewDistance = 6f;    // 시야 거리 (발각)
    [Range(0, 180)] public float fovAngle = 80f;
    public LayerMask obstacleMask;

    [Header("참조")]
    public Rigidbody2D target;
    [SerializeField] Animator anim;    // 인스펙터로 연결 (있으면 자동 할당)

    [Header("표식")]
    public GameObject questionMark;
    public GameObject exclamationMark;

    [Header("체력")]
    public int maxHP = 30;

    // ---------- 외부에서 읽는 프로퍼티 ----------
    // 다른 스크립트(Bite 등)에서 사용하므로 public으로 노출
    public bool IsAlerted => hasSpotted;
    public bool IsAlive => isLive;

    // ---------- 내부 상태 ----------
    private int currentHP;
    private bool isLive = true;      // 살아있는지
    private bool hasSpotted = false; // 발각 여부 (true면 추격/공격)
    private float nextAttackTime = 0f;
    private bool dealtThisFixed = false;

    private Rigidbody2D rb;
    private SpriteRenderer sr;

    // Animator 관련
    private int hashIsWalk, hashDoAttack;
    private Vector2 prevPos;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();

        currentHP = Mathf.Max(1, maxHP);

        if (!target)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p) target = p.GetComponent<Rigidbody2D>();
        }

        if (!anim) anim = GetComponentInChildren<Animator>(true);
        hashIsWalk = Animator.StringToHash("isWalk");
        hashDoAttack = Animator.StringToHash("doAttack");

        prevPos = rb.position;

        ShowQuestion(false);
        ShowAlert(false);
    }

    void FixedUpdate()
    {
        dealtThisFixed = false;

        if (hasSpotted)
        {
            float dt = Time.fixedDeltaTime;
            Vector2 cur = rb.position;
            Vector2 toTarget = (Vector2)target.position - cur;
            Vector2 dir = toTarget.normalized;

            // 바로 앞에 뭔가 있나 짧게 캐스트 (태그로만 필터)
            RaycastHit2D hit = Physics2D.CircleCast(cur, 0.2f, dir, 0.12f);
            bool blocked =
                hit.collider != null &&
                (
                    hit.collider.CompareTag("GameObject") ||   // 벽/기둥/상자 등에 공통으로 붙일 태그
                    hit.collider.CompareTag("GameObject") ||       // 쓰는 태그가 따로면 여기에 추가
                    hit.collider.CompareTag("GameObject")
                );

            if (blocked)
            {
                // 벽에 닿았으면 벽 접선 방향으로 한 스텝 미끄러지기
                Vector2 n = hit.normal;
                Vector2 t1 = new Vector2(-n.y, n.x).normalized;
                Vector2 t2 = new Vector2(n.y, -n.x).normalized;

                Vector2 cand1 = cur + t1 * Speed * dt;
                Vector2 cand2 = cur + t2 * Speed * dt;
                float d1 = ((Vector2)target.position - cand1).sqrMagnitude;
                float d2 = ((Vector2)target.position - cand2).sqrMagnitude;

                Vector2 slide = (d1 < d2 ? t1 : t2);
                rb.MovePosition(cur + slide * Speed * dt);
            }
            else
            {
                // 막힌 게 없으면 직선 추격
                rb.MovePosition(cur + dir * Speed * dt);
            }

            bool faceLeft = target.position.x < rb.position.x;
            transform.localScale = new Vector3(faceLeft ? -1f : 1f, 1f, 1f);
        }

        // 발각 전/발각 처리(기존 로직)
        if (!hasSpotted)
        {
            HandleProximitySuspicion();
            if (!hasSpotted && CanDetectPlayer()) SetAlerted();
        }

        // 발각 후 추격 (이동은 항상 수행)
        if (hasSpotted)
        {
            rb.MovePosition(rb.position + (target.position - rb.position).normalized * Speed * Time.fixedDeltaTime);
            sr.flipX = target.position.x < rb.position.x;
        }

        // 이동 여부 판정 — 단, Attack 상태일 때는 검사/갱신하지 않음
        bool isInAttack = anim && anim.GetCurrentAnimatorStateInfo(0).IsName("Attack");
        if (!isInAttack)
        {
            Vector2 delta = rb.position - prevPos;
            bool moved = delta.sqrMagnitude > 0.000001f;
            if (anim) anim.SetBool(hashIsWalk, moved);
        }
        // Attack 중이면 isWalk 갱신을 건너뜀 -> 깜빡임 제거

        rb.linearVelocity = Vector2.zero;
        prevPos = rb.position;

        if (anim)
        {
            bool walking = hasSpotted && isLive; // 추격 중일 때만 걷기
            anim.SetBool("isWalk", walking);
        }

        if (anim)
        {
            // 현재 위치에서 목표까지의 거리
            float dist = Vector2.Distance(rb.position, target.position);

            // 거리가 일정 이상이면 걷기 = true, 아니면 Idle
            bool walking = hasSpotted && dist > 0.1f;
            anim.SetBool("isWalk", walking);
        }
    }


    // ----------------- 근접(의심) -----------------
    void HandleProximitySuspicion()
    {
        bool inRange = Vector2.SqrMagnitude(target.position - rb.position) <= detectRadius * detectRadius;
        if (inRange) ShowQuestion(true);
        else ShowQuestion(false);
    }

    void SetAlerted()
    {
        hasSpotted = true;
        ShowQuestion(false);
        ShowAlert(true);
    }

    // ----------------- 공격 (충돌/트리거 모두 대응) -----------------
    void OnCollisionEnter2D(Collision2D c) { TryAttack(c.collider); }
    void OnCollisionStay2D(Collision2D c) { TryAttack(c.collider); }
    void OnCollisionExit2D(Collision2D c)
    {
        if (c.collider.CompareTag("Player")) nextAttackTime = 0f;
    }

    void OnTriggerEnter2D(Collider2D c) { TryAttack(c); }
    void OnTriggerStay2D(Collider2D c) { TryAttack(c); }
    void OnTriggerExit2D(Collider2D c)
    {
        if (c.CompareTag("Player")) nextAttackTime = 0f;
    }

    void TryAttack(Collider2D col)
    {
        if (!isLive) return;
        if (!col || !col.CompareTag("Player")) return;
        if (!hasSpotted) return;                 // 발각 전에는 공격 안 함
        if (Time.time < nextAttackTime) return;
        if (dealtThisFixed) return;

        var player = col.GetComponentInParent<Player>();
        if (!player) return;

        // Attack 애니메이션 트리거
        if (anim) anim.SetTrigger(hashDoAttack);

        // 데미지 적용
        int dmg = Random.Range(minDamage, maxDamage + 1);
        player.TakeDamage(dmg);

        nextAttackTime = Time.time + attackCooldown;
        dealtThisFixed = true;
    }

    // ----------------- 시야(발각) 체크 -----------------
    bool CanDetectPlayer()
    {
        Vector2 myPos = rb.position;
        Vector2 to = target.position - myPos;
        float dist = to.magnitude;

        if (dist > viewDistance) return false;

        Vector2 forward = (sr && sr.flipX) ? Vector2.left : Vector2.right;
        float angle = Vector2.Angle(forward, to.normalized);
        if (angle > (fovAngle * 0.5f)) return false;

        return !Blocked(myPos, target.position);
    }

    bool Blocked(Vector2 from, Vector2 to)
    {
        var hit = Physics2D.Linecast(from, to, obstacleMask);
        return hit.collider != null;
    }

    // ----------------- 피해/사망 -----------------
    public void TakeDamage(int damage)
    {
        if (!isLive) return;
        currentHP -= Mathf.Max(1, damage);

        // 맞으면 발각
        SetAlerted();

        if (currentHP <= 0) Die();
    }

    public void KillSilently()  // Bite에서 호출하는 메서드: 외부에서 안전하게 사용
    {
        if (!isLive) return;
        isLive = false;

        var cols = GetComponentsInChildren<Collider2D>(true);
        foreach (var c in cols) if (c) c.enabled = false;
        if (rb) rb.simulated = false;

        Destroy(gameObject);
    }

    void Die()
    {
        if (!isLive) return;
        isLive = false;

        ShowQuestion(false);
        ShowAlert(false);

        foreach (var c in GetComponentsInChildren<Collider2D>(true)) if (c) c.enabled = false;
        if (rb) rb.simulated = false;
        Destroy(gameObject);
    }

    // ----------------- UI 표식 -----------------
    void ShowQuestion(bool on) { if (questionMark && questionMark.activeSelf != on) questionMark.SetActive(on); }
    void ShowAlert(bool on) { if (exclamationMark && exclamationMark.activeSelf != on) exclamationMark.SetActive(on); }
}
