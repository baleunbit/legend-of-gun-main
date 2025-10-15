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
    public float detectRadius = 4f;
    public float viewDistance = 6f;
    [Range(0, 180)] public float fovAngle = 80f;

    [Header("참조")]
    public Rigidbody2D target;
    [SerializeField] Animator anim;

    [Header("표식")]
    public GameObject questionMark;
    public GameObject exclamationMark;

    [Header("체력")]
    public int maxHP = 30;

    [Header("피격/사망 SFX")]
    public AudioClip hitSfx;
    [Range(0f, 1f)] public float hitSfxVolume = 0.8f;
    public AudioClip deathSfx;
    [Range(0f, 1f)] public float deathSfxVolume = 1f;

    public bool IsAlerted => hasSpotted;
    public bool IsAlive => isLive;

    int currentHP;
    bool isLive = true;
    bool hasSpotted = false;
    float nextAttackTime = 0f;
    bool dealtThisFixed = false;

    Rigidbody2D rb;
    SpriteRenderer sr;

    int hashIsWalk, Attack;
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

        if (!hasSpotted)
        {
            float sqr = (target.position - rb.position).sqrMagnitude;
            bool inProximity = sqr <= detectRadius * detectRadius;
            bool inFov = InFovAndVisible();

            if (inFov) SetAlerted();
            else { ShowQuestion(inProximity); ShowAlert(false); }

            if (!hasSpotted)
            {
                rb.linearVelocity = Vector2.zero;
                if (anim) anim.SetBool(hashIsWalk, false);
                prevPos = rb.position;
                return;
            }
        }

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

    bool InFovAndVisible()
    {
        Vector2 myPos = rb.position;
        Vector2 to = target.position - myPos;
        float dist = to.magnitude;

        if (dist > viewDistance) return false;

        Vector2 forward = (sr && sr.flipX) ? Vector2.left : Vector2.right;
        float ang = Vector2.Angle(forward, to.normalized);
        if (ang > (fovAngle * 0.5f)) return false;

        // 간단화: 라인캐스트 생략(맵 가림 처리 필요하면 추가)
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

        // 피격 사운드만 재생 (히트 플래시 제거)
        if (hitSfx)
            AudioSource.PlayClipAtPoint(hitSfx, transform.position, hitSfxVolume);

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

        if (deathSfx)
            AudioSource.PlayClipAtPoint(deathSfx, transform.position, deathSfxVolume);

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
    }
#endif
}
