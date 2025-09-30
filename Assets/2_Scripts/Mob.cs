using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(SpriteRenderer))]
public class Mob : MonoBehaviour
{
    [Header("이동")]
    public float Speed = 7f;

    [Header("공격")]
    public int minDamage = 3;
    public int maxDamage = 5;
    public float attackCooldown = 1f;

    [Header("탐지(발견 전까지)")]
    public float detectRadius = 4f;                  // ✅ 근접(소리) 반경: 의심만, 발각 NO
    public float viewDistance = 6f;                  // 시야 거리
    [Range(0, 180)] public float fovAngle = 80f;     // 시야각
    public LayerMask obstacleMask;                   // 가림 체크용(시야)

    [Header("발각 규칙 (사용 안 함)")]
    public float proximityAlertTime = 5f;            // (과거 로직: 근접 유지로 발각) — 지금은 미사용

    [Header("참조")]
    public Rigidbody2D target;                       // Player Rigidbody2D

    [Header("표식(선택)")]
    public GameObject questionMark;                  // 머리 위 ? (근접 의심)
    public GameObject exclamationMark;               // 머리 위 ! (발각)

    [Header("체력")]
    public int maxHP = 30;

    // 외부에서 읽기
    public bool IsAlerted => hasSpotted;
    public bool IsAlive => isLive;

    // 내부 상태
    int currentHP;
    bool isLive = true;
    bool hasSpotted = false;         // true 되면 계속 추격
    float nextAttackTime = 0f;
    bool dealtThisFixed = false;

    // (과거) 근접 유지 타이머 — 현재는 발각에 사용하지 않음
    float proximityTimer = 0f;

    Rigidbody2D rb;
    SpriteRenderer sr;

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

        ShowQuestion(false);
        ShowAlert(false);
    }

    void FixedUpdate()
    {
        dealtThisFixed = false;

        if (!isLive || !target)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        // 1) 발각 전: 근접 의심 표시만, 시야에 걸리면 발각
        if (!hasSpotted)
        {
            HandleProximitySuspicion();     // ❗ 의심만 — 발각하지 않음

            if (!hasSpotted && CanDetectPlayer())
            {
                SetAlerted(); // ! + 추격 시작 (Speed 사용은 Mob이 담당)
            }
        }

        // 2) 발각 후: 추격
        if (hasSpotted)
        {
            Vector2 dir = (target.position - rb.position).normalized;
            rb.MovePosition(rb.position + dir * Speed * Time.fixedDeltaTime);
            sr.flipX = target.position.x < rb.position.x; // 시선
        }

        rb.linearVelocity = Vector2.zero;
    }

    void LateUpdate()
    {
        if (!isLive || !target) return;
    }

    // ───────────────────────────────────
    // 근접 유지 감시 (이제는 의심만, 발각 NO)
    void HandleProximitySuspicion()
    {
    bool inRange = Vector2.SqrMagnitude(target.position - rb.position) <= detectRadius * detectRadius;

    if (inRange)
    {
        ShowQuestion(true); // ? 만 켜기
        // 절대 SetAlerted() 부르지 않음
    }
    else
    {
        ShowQuestion(false);
    }
    }

    void SetAlerted()
    {
        hasSpotted = true;
        proximityTimer = 0f;
        ShowQuestion(false);
        ShowAlert(true);
    }

    // ───────────────────────────────────
    // 충돌 공격
    void OnCollisionEnter2D(Collision2D c) { TryDealDamage(c); }
    void OnCollisionStay2D(Collision2D c) { TryDealDamage(c); }
    void OnCollisionExit2D(Collision2D c)
    {
        if (c.collider.CompareTag("Player")) nextAttackTime = 0f;
    }

    void TryDealDamage(Collision2D c)
    {
        if (!isLive) return;
        if (!c.collider.CompareTag("Player")) return;
        if (!hasSpotted) return;                 // 발각 전 공격 금지
        if (dealtThisFixed) return;
        if (Time.time < nextAttackTime) return;

        var player = c.collider.GetComponentInParent<Player>();
        if (!player) return;

        int dmg = Random.Range(minDamage, maxDamage + 1);
        player.TakeDamage(dmg);

        nextAttackTime = Time.time + attackCooldown;
        dealtThisFixed = true;
    }

    // ───────────────────────────────────
    // 시야 감지 (발각 전만)
    bool CanDetectPlayer()
    {
        Vector2 myPos = rb.position;
        Vector2 to = target.position - myPos;
        float dist = to.magnitude;

        if (dist > viewDistance) return false;

        Vector2 forward = (sr && sr.flipX) ? Vector2.left : Vector2.right;
        float angle = Vector2.Angle(forward, to.normalized);
        if (angle > (fovAngle * 0.5f)) return false;

        // 가림 체크
        return !Blocked(myPos, target.position);
    }

    bool Blocked(Vector2 from, Vector2 to)
    {
        var hit = Physics2D.Linecast(from, to, obstacleMask);
        return hit.collider != null;
    }

    // ───────────────────────────────────
    // 피해/사망
    public void TakeDamage(int damage)
    {
        if (!isLive) return;

        currentHP -= Mathf.Max(1, damage);

        // 총/공격을 맞으면 즉시 발각 (스텔스 실패)
        SetAlerted();

        if (currentHP <= 0) Die();
    }

    public void KillSilently()
    {
        if (!isLive) return;
        isLive = false;

        var cols = GetComponentsInChildren<Collider2D>(true);
        foreach (var c in cols) if (c) c.enabled = false;
        if (rb) rb.simulated = false;

        Destroy(gameObject);
    }

    public void OnDamage(float damage) => TakeDamage(Mathf.RoundToInt(damage));

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

    // ───────────────────────────────────
    // 머리표시 on/off
    void ShowQuestion(bool on)
    {
        if (questionMark && questionMark.activeSelf != on) questionMark.SetActive(on);
    }
    void ShowAlert(bool on)
    {
        if (exclamationMark && exclamationMark.activeSelf != on) exclamationMark.SetActive(on);
    }

    // 디버그
#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.2f, 0.7f, 1f, 0.25f);
        Gizmos.DrawWireSphere(transform.position, detectRadius);

        Gizmos.color = new Color(1f, 0.9f, 0.1f, 0.25f);
        Vector2 forward = Vector2.right;
        var sr0 = GetComponentInChildren<SpriteRenderer>();
        if (sr0 && sr0.flipX) forward = Vector2.left;
        float half = fovAngle * 0.5f;
        Vector2 L = Quaternion.Euler(0, 0, +half) * forward * viewDistance;
        Vector2 R = Quaternion.Euler(0, 0, -half) * forward * viewDistance;
        Gizmos.DrawLine(transform.position, (Vector2)transform.position + L);
        Gizmos.DrawLine(transform.position, (Vector2)transform.position + R);
    }
#endif
}
