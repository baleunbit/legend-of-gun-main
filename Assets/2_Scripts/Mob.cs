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

    [Header("탐지-설정")]
    public bool useSight = true;                 // 시야(부채꼴)로도 발각할지
    public bool useProximityDwell = true;        // 파란원 안 머무르면 발각할지
    public float detectRadius = 4f;              // 파란 원 반경(근접)
    public float ringAlertTime = 0.6f;           // 근접 링 안 '머무른 시간' 후 발각
    public float viewDistance = 6f;              // 시야 거리
    [Range(0, 180)] public float fovAngle = 80f;  // 시야 각
    public float loseSightDelay = 0.8f;          // 시야 놓친 뒤 Idle로 돌아갈 버퍼(원하면 사용)

    [Header("시야 차단(선택)")]
    public Transform eye;                        // 레이 발사 원점(없으면 본체)
    public LayerMask obstacleMask;               // 차단 레이어(비워두면 태그 "GameObject" 폴백)

    [Header("참조")]
    public Rigidbody2D target;
    [SerializeField] Animator anim;

    [Header("표식")]
    public GameObject questionMark;
    public GameObject exclamationMark;

    [Header("체력")]
    public int maxHP = 30;

    public bool IsAlerted => hasSpotted;
    public bool IsAlive => isLive;

    // ─────────────────────────────────────────────

    int currentHP;
    bool isLive = true;
    bool hasSpotted = false;

    float nextAttackTime = 0f;
    bool dealtThisFixed = false;

    Rigidbody2D rb;
    SpriteRenderer sr;

    int hashIsWalk, Attack;
    Vector2 prevPos;

    // 근접 머무름 타이머 + 시야 끊김 버퍼
    float proximityTimer = 0f;
    float lastSeenTime = -999f;

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
        if (anim)
        {
            hashIsWalk = Animator.StringToHash("isWalk");
            Attack = Animator.StringToHash("doAttack");
        }

        SetupMarker(questionMark);
        SetupMarker(exclamationMark);

        ShowQuestion(false);
        ShowAlert(false);
        prevPos = rb.position;
    }

    void FixedUpdate()
    {
        dealtThisFixed = false;

        if (!isLive || !target)
        {
            rb.linearVelocity = Vector2.zero;
            if (anim) anim.SetBool(hashIsWalk, false);
            prevPos = rb.position;
            return;
        }

        // ── 발각 전 단계: 감지 로직
        if (!hasSpotted)
        {
            bool becameAlert = false;

            // 1) 근접 링 안 머무름 판정
            if (useProximityDwell)
            {
                float sqr = (target.position - rb.position).sqrMagnitude;
                bool inProximity = sqr <= detectRadius * detectRadius;
                if (inProximity) proximityTimer += Time.fixedDeltaTime;
                else proximityTimer = 0f;

                if (proximityTimer >= ringAlertTime)
                    becameAlert = true;

                // 표식 업데이트
                ShowQuestion(inProximity && !becameAlert);
                ShowAlert(false);
            }

            // 2) 시야 판정(보이는 즉시 발각)
            if (!becameAlert && useSight)
            {
                if (InFovAndVisible())
                {
                    becameAlert = true;
                    lastSeenTime = Time.time;
                }
            }

            if (becameAlert)
            {
                SetAlerted();
            }
            else
            {
                // 아직 발각 전이면 정지
                rb.linearVelocity = Vector2.zero;
                if (anim) anim.SetBool(hashIsWalk, false);
                prevPos = rb.position;
                return;
            }
        }
        else
        {
            // 선택: 시야를 완전히 잃고 loseSightDelay가 지나면 Idle로 복귀 (원치 않으면 주석)
            if (useSight && loseSightDelay > 0f)
            {
                if (InFovAndVisible()) lastSeenTime = Time.time;
                else if (Time.time - lastSeenTime > loseSightDelay)
                {
                    hasSpotted = false;
                    ShowAlert(false);
                    ShowQuestion(false);
                    rb.linearVelocity = Vector2.zero;
                    if (anim) anim.SetBool(hashIsWalk, false);
                    return;
                }
            }
        }

        // ── 추격 이동
        Vector2 cur = rb.position;
        Vector2 dir = ((Vector2)target.position - cur).normalized;
        rb.MovePosition(cur + dir * Speed * Time.fixedDeltaTime);

        sr.flipX = target.position.x < rb.position.x;
        if (anim) anim.SetBool(hashIsWalk, true);

        rb.linearVelocity = Vector2.zero;
        prevPos = rb.position;
    }

    void OnCollisionEnter2D(Collision2D c) { TryAttack(c.collider); }
    void OnCollisionStay2D(Collision2D c) { TryAttack(c.collider); }
    void OnTriggerEnter2D(Collider2D c) { TryAttack(c); }
    void OnTriggerStay2D(Collider2D c) { TryAttack(c); }

    void TryAttack(Collider2D col)
    {
        if (!isLive || !hasSpotted) return;
        if (!col || !col.CompareTag("Player")) return;
        if (Time.time < nextAttackTime) return;
        if (dealtThisFixed) return;

        var player = col.GetComponentInParent<Player>();
        if (!player) return;

        if (anim) anim.SetTrigger(Attack);

        int dmg = Random.Range(minDamage, maxDamage + 1);
        player.TakeDamage(dmg);

        nextAttackTime = Time.time + attackCooldown;
        dealtThisFixed = true;
    }

    // ─────────────────────────────────────────────

    bool InFovAndVisible()
    {
        Vector2 origin = eye ? (Vector2)eye.position : rb.position;
        Vector2 to = target.position - origin;
        float dist = to.magnitude;

        if (dist > viewDistance) return false;

        // 전방 벡터(스프라이트 기준)
        Vector2 forward = (sr && sr.flipX) ? Vector2.left : Vector2.right;
        float ang = Vector2.Angle(forward, to.normalized);
        if (ang > (fovAngle * 0.5f)) return false;

        // 가시선 차단
        if (obstacleMask.value != 0)
        {
            var hit = Physics2D.Raycast(origin, to.normalized, dist, obstacleMask);
            if (hit.collider != null) return false;
        }
        else
        {
            // 폴백: 태그로 차단
            var hit = Physics2D.Linecast(origin, target.position);
            if (hit.collider != null && hit.collider.CompareTag("GameObject")) return false;
        }

        return true;
    }

    void SetAlerted()
    {
        hasSpotted = true;
        ShowQuestion(false);
        ShowAlert(true);
    }

    public void TakeDamage(int damage)
    {
        if (!isLive) return;
        currentHP -= Mathf.Max(1, damage);
        SetAlerted();
        if (currentHP <= 0) Die();
    }

    public void KillSilently()
    {
        if (!isLive) return;
        isLive = false;
        foreach (var c in GetComponentsInChildren<Collider2D>(true)) if (c) c.enabled = false;
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

    void ShowQuestion(bool on)
    {
        if (questionMark && questionMark.activeSelf != on) questionMark.SetActive(on);
    }
    void ShowAlert(bool on)
    {
        if (exclamationMark && exclamationMark.activeSelf != on) exclamationMark.SetActive(on);
    }

    void SetupMarker(GameObject go)
    {
        if (!go) return;
        if (go.transform.parent != transform)
            go.transform.SetParent(transform, true);
        go.transform.localPosition = new Vector3(0f, 0.8f, 0f);
        var mSr = go.GetComponent<SpriteRenderer>();
        if (mSr && sr)
        {
            mSr.sortingLayerID = sr.sortingLayerID;
            mSr.sortingOrder = sr.sortingOrder + 1;
        }
    }

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
