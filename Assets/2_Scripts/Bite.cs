using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Animator))]
public class Bite : MonoBehaviour
{
    [Header("Bite 키/범위/태그")]
    public KeyCode biteKey = KeyCode.E;
    public float biteRange = 1.4f;
    public string enemyTag = "Mob";

    [Header("조건")]
    public bool requireStealth = true;      // 발각된 몹은 못 먹음
    public bool requireBackAngle = false;   // 뒤에서만 먹게 할지
    [Range(0, 180)] public float backAngle = 120f;

    [Header("VFX (옵션)")]
    public GameObject biteVfx;

    [Header("애니메이션")]
    public float biteCooldown = 0.35f;      // 연타 방지
    public string biteStateName = "Bite";   // 애니 이름
    public string standStateName = "Stand 0";

    [Header("디버그")]
    public bool debugLog = false;

    Animator _anim;
    Transform _tr;

    static readonly int HashBiteTrigger = Animator.StringToHash("Bite");
    bool _canBite = true;          // 쿨다운
    bool _pendingBite = false;     // 이번 Bite 애니 동안 1회만 판정
    Mob _pendingTarget = null;     // 애니 이벤트 시 처리할 대상

    void Awake()
    {
        _tr = transform;
        _anim = GetComponent<Animator>();
        if (_anim.runtimeAnimatorController == null)
            Debug.LogError("[Bite] Animator Controller가 비어있음");
    }

    void Update()
    {
        if (Input.GetKeyDown(biteKey) && _canBite)
        {
            var target = FindBestTarget();
            if (target != null)
            {
                StartBite(target);
                StartCoroutine(CoCooldown());
            }
            else if (debugLog) Debug.Log("[Bite] 대상 없음: 범위/스텔스/각도/태그 확인");
        }
    }

    // ─────────────────────────────────────────────────────────
    // 대상 탐색 (성공 시 Bite 시작, 처치는 애니 이벤트에서)
    Mob FindBestTarget()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(_tr.position, biteRange);
        Mob best = null;
        float bestDist = float.MaxValue;

        foreach (var h in hits)
        {
            if (!(h.CompareTag(enemyTag) || (h.transform.parent && h.transform.parent.CompareTag(enemyTag))))
                continue;

            var mob = h.GetComponentInParent<Mob>() ?? h.GetComponent<Mob>();
            if (!mob) continue;

            if (requireStealth && mob.IsAlerted) continue;            // 발각되면 제외
            if (requireBackAngle && !IsBehindTarget(mob.transform)) continue;

            float d = ((Vector2)mob.transform.position - (Vector2)_tr.position).sqrMagnitude;
            if (d < bestDist) { bestDist = d; best = mob; }
        }
        return best;
    }

    void StartBite(Mob target)
    {
        _pendingTarget = target;
        _pendingBite = true;

        _anim.ResetTrigger(HashBiteTrigger);
        _anim.SetTrigger(HashBiteTrigger);
        _anim.CrossFadeInFixedTime(biteStateName, 0.05f, 0, 0f);

        float len = GetStateLength(biteStateName);
        if (len > 0f) StartCoroutine(ForceBackToStand(len + 0.05f));

        if (debugLog) Debug.Log("[Bite] 시작 → 대상: " + target.name);
    }

    IEnumerator ForceBackToStand(float delay)
    {
        yield return new WaitForSeconds(delay);
        _anim.CrossFadeInFixedTime(standStateName, 0.05f, 0, 0f);
    }

    IEnumerator CoCooldown()
    {
        _canBite = false;
        yield return new WaitForSeconds(biteCooldown);
        _canBite = true;
    }

    // ===== Animation Event 수신 (클립에서 BiteEvent/BiteHitEvent 호출) =====
    public void BiteEvent() { OnBiteHit(); }
    public void BiteHitEvent() { OnBiteHit(); }

    void OnBiteHit()
    {
        if (!_pendingBite) return;
        _pendingBite = false;

        if (_pendingTarget != null)
        {
            if (biteVfx) Instantiate(biteVfx, _pendingTarget.transform.position, Quaternion.identity);
            _pendingTarget.KillSilently();   // ✅ Mob의 메서드 호출
            EatBar.Instance?.AddFromEat(10); // <- 10은 회복량 (원하면 20~30으로 테스트)
            if (debugLog) Debug.Log("[Bite] 성공 처리 완료");
        }
        _pendingTarget = null;
    }

    // =====================================================================

    bool IsBehindTarget(Transform target)
    {
        var sr = target.GetComponentInChildren<SpriteRenderer>();
        Vector2 forward = (sr != null && sr.flipX) ? Vector2.left : Vector2.right;

        Vector2 toPlayer = ((Vector2)_tr.position - (Vector2)target.position).normalized;
        float ang = Vector2.Angle(forward, toPlayer);
        return ang >= (180f - backAngle * 0.5f);
    }

    float GetStateLength(string stateName)
    {
        var clips = _anim.runtimeAnimatorController.animationClips;
        foreach (var c in clips) if (c.name == stateName) return c.length;
        return 0f;
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, biteRange);
    }
#endif
}
