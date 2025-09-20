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

    [Header("탐지(발견 전까지만)")]
    public float detectRadius = 4f;
    public float viewDistance = 6f;
    [Range(0, 180)] public float fovAngle = 80f;
    public LayerMask obstacleMask;

    [Header("참조")]
    public Rigidbody2D target;   // Player의 Rigidbody2D(스포너/인스펙터에서 주입)

    [Header("체력")]
    public int maxHP = 30;

    public bool IsAlerted => hasSpotted;
    public bool IsAlive => isLive;

    int currentHP;
    bool isLive = true;
    bool hasSpotted = false;
    float nextAttackTime = 0f;
    bool dealtThisFixed = false;

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
    }

    void FixedUpdate()
    {
        dealtThisFixed = false;

        if (!isLive || !target)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        // 아직 못 봤으면 감지 시도
        if (!hasSpotted && CanDetectPlayer())
            hasSpotted = true;

        // 봤으면 추격
        if (hasSpotted)
        {
            Vector2 dir = (target.position - rb.position).normalized;
            rb.MovePosition(rb.position + dir * Speed * Time.fixedDeltaTime);
        }

        rb.linearVelocity = Vector2.zero;
    }

    void LateUpdate()
    {
        if (!isLive) return;
        // ✅ 순찰 스크립트와 충돌 방지: 발견했을 때만 플레이어 보게
        if (hasSpotted && target)
            sr.flipX = target.position.x < rb.position.x;
    }

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
        if (!hasSpotted) return;
        if (dealtThisFixed) return;
        if (Time.time < nextAttackTime) return;

        var player = c.collider.GetComponentInParent<Player>();
        if (!player) return;

        int dmg = Random.Range(minDamage, maxDamage + 1);
        player.TakeDamage(dmg);

        nextAttackTime = Time.time + attackCooldown;
        dealtThisFixed = true;
    }

    // ===== 감지 로직 =====
    bool CanDetectPlayer()
    {
        Vector2 my = rb.position;
        Vector2 to = target.position - my;
        float dist = to.magnitude;

        // 1) 근접
        if (dist <= detectRadius)
            return !Blocked(my, target.position);

        // 2) 시야
        if (dist > viewDistance) return false;

        Vector2 forward = sr && sr.flipX ? Vector2.left : Vector2.right;
        float angle = Vector2.Angle(forward, to.normalized);
        if (angle > fovAngle * 0.5f) return false;

        return !Blocked(my, target.position);
    }

    bool Blocked(Vector2 from, Vector2 to)
    {
        var hit = Physics2D.Linecast(from, to, obstacleMask);
        return hit.collider != null;
    }

    // ===== 피격/사망 =====
    public void TakeDamage(int damage)
    {
        if (!isLive) return;
        currentHP -= Mathf.Max(1, damage);
        hasSpotted = true; // 총 맞으면 바로 발각
        if (currentHP <= 0) Die();
    }

    public void OnDamage(float damage) => TakeDamage(Mathf.RoundToInt(damage));

    void Die()
    {
        if (!isLive) return;
        isLive = false;
        var cols = GetComponentsInChildren<Collider2D>(true);
        foreach (var c in cols) if (c) c.enabled = false;
        if (rb) rb.simulated = false;
        Destroy(gameObject);
    }

    public void KillSilently() => Die();
}
