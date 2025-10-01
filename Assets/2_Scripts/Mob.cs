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
    public float detectRadius = 4f;    // 외부 감지(원) → ? 표시 전용
    public float viewDistance = 6f;    // 시야 거리(부채꼴)
    [Range(0, 180)] public float fovAngle = 80f;   // 부채꼴 각도

    [Header("참조")]
    public Rigidbody2D target;               // 플레이어 Rigidbody2D
    [SerializeField] Animator anim;          // (선택) 애니메이터 사용 시 연결

    [Header("표식(월드 오브젝트)")]
    public GameObject questionMark;          // ?
    public GameObject exclamationMark;       // !

    [Header("체력")]
    public int maxHP = 30;

    // 외부에서 읽는 상태
    public bool IsAlerted => hasSpotted;
    public bool IsAlive => isLive;

    // 내부 상태
    int currentHP;
    bool isLive = true;
    bool hasSpotted = false;
    float nextAttackTime = 0f;
    bool dealtThisFixed = false;

    Rigidbody2D rb;
    SpriteRenderer sr;

    // (선택) 애니 파라미터
    int hashIsWalk, hashDoAttack;
    Vector2 prevPos;

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
            hashDoAttack = Animator.StringToHash("doAttack");
        }

        // 마커가 가려지지 않게 정렬/위치 보정
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

        // ───────── 발각 전: ?/! 결정 ─────────
        if (!hasSpotted)
        {
            float sqr = (target.position - rb.position).sqrMagnitude;
            bool inProximity = sqr <= detectRadius * detectRadius;  // 원
            bool inFov = InFovAndVisible();                         // 부채꼴 + 가림 없음

            if (inFov)
            {
                // 부채꼴에 들어오면 즉시 발각 → !
                SetAlerted();
            }
            else
            {
                // 시야엔 없음: 원 안이면 ? 표시, 아니면 둘 다 off
                ShowQuestion(inProximity);
                ShowAlert(false);
            }

            // 발각 전엔 이동하지 않음
            if (!hasSpotted)
            {
                rb.linearVelocity = Vector2.zero;
                if (anim) anim.SetBool(hashIsWalk, false);
                prevPos = rb.position;
                return;
            }
        }

        // ───────── 발각 후: 추격 이동 ─────────
        Vector2 cur = rb.position;
        Vector2 dir = ((Vector2)target.position - cur).normalized;
        rb.MovePosition(cur + dir * Speed * Time.fixedDeltaTime);

        // 좌우 바라보기
        sr.flipX = target.position.x < rb.position.x;

        // (선택) 애니: 걷기 on
        if (anim) anim.SetBool(hashIsWalk, true);

        rb.linearVelocity = Vector2.zero;
        prevPos = rb.position;
    }

    // 충돌 공격 (발각 후에만)
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

        if (anim) anim.SetTrigger(hashDoAttack);

        int dmg = Random.Range(minDamage, maxDamage + 1);
        player.TakeDamage(dmg);

        nextAttackTime = Time.time + attackCooldown;
        dealtThisFixed = true;
    }

    // ───────── 시야(FOV) + 가림 체크 ─────────
    bool InFovAndVisible()
    {
        Vector2 myPos = rb.position;
        Vector2 to = target.position - myPos;
        float dist = to.magnitude;

        if (dist > viewDistance) return false;

        // 좌/우만 본다고 가정(스프라이트가 보는 방향)
        Vector2 forward = (sr && sr.flipX) ? Vector2.left : Vector2.right;
        float ang = Vector2.Angle(forward, to.normalized);
        if (ang > (fovAngle * 0.5f)) return false;

        // 라인 오브 사이트: "GameObject" 태그에 맞으면 가려진 것
        var hit = Physics2D.Linecast(myPos, target.position);
        if (hit.collider != null && hit.collider.CompareTag("GameObject"))
            return false;

        return true;
    }

    void SetAlerted()
    {
        hasSpotted = true;
        ShowQuestion(false);
        ShowAlert(true);
    }

    // ───────── 피해/사망 ─────────
    public void TakeDamage(int damage)
    {
        if (!isLive) return;
        currentHP -= Mathf.Max(1, damage);

        // 총 맞으면 즉시 발각
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

    // ───────── 마커 on/off & 셋업 ─────────
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

        // 본체 자식으로 두고 머리 위로
        if (go.transform.parent != transform)
            go.transform.SetParent(transform, true);
        go.transform.localPosition = new Vector3(0f, 0.8f, 0f);

        // 스프라이트 정렬 1단계 위
        var mSr = go.GetComponent<SpriteRenderer>();
        if (mSr && sr)
        {
            mSr.sortingLayerID = sr.sortingLayerID;
            mSr.sortingOrder = sr.sortingOrder + 1;
        }
    }

#if UNITY_EDITOR
    // 디버그 기즈모
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
