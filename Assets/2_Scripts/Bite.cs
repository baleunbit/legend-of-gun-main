// Bite.cs
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
    public bool requireStealth = true;   // 경계 중인 몹은 못 먹기
    public bool requireBackAngle = false;
    [Range(0, 180)] public float backAngle = 120f;

    [Header("VFX/SFX (옵션)")]
    public GameObject biteVfx;
    public AudioClip biteSfx;
    [Range(0f, 1f)] public float biteSfxVolume = 0.9f;

    [Header("애니메이션")]
    public float biteCooldown = 0.35f;
    public string biteStateName = "Bite";
    public string standStateName = "Stand 0";

    [Header("디버그")]
    public bool debugLog = false;

    Animator _anim;
    Transform _tr;
    Player _player;

    static readonly int HashBiteTrigger = Animator.StringToHash("Bite");
    bool _canBite = true;
    bool _pendingBite = false;
    Mob _pendingTarget = null;

    void Awake()
    {
        _tr = transform;
        _anim = GetComponent<Animator>();
        if (_anim.runtimeAnimatorController == null)
            Debug.LogError("[Bite] Animator Controller가 비어있음");

        var pObj = GameObject.FindGameObjectWithTag("Player");
        _player = pObj ? pObj.GetComponent<Player>() : null;
        if (!_player) Debug.LogWarning("[Bite] Player를 찾지 못했습니다. (Tag=Player 확인)");
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

    Mob FindBestTarget()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(_tr.position, biteRange);
        Mob best = null; float bestDist = float.MaxValue;

        foreach (var h in hits)
        {
            if (!(h.CompareTag(enemyTag) || (h.transform.parent && h.transform.parent.CompareTag(enemyTag))))
                continue;

            var mob = h.GetComponentInParent<Mob>() ?? h.GetComponent<Mob>();
            if (!mob || !mob.IsAlive) continue;

            if (requireStealth && mob.IsAlerted) continue;
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

    // 애니메이션 이벤트에서 호출할 것: BiteEvent 또는 BiteHitEvent
    public void BiteEvent() { OnBiteHit(); }
    public void BiteHitEvent() { OnBiteHit(); }

    void OnBiteHit()
    {
        if (!_pendingBite) return;
        _pendingBite = false;

        if (_pendingTarget != null && _pendingTarget.IsAlive)
        {
            if (biteSfx) AudioSource.PlayClipAtPoint(biteSfx, _pendingTarget.transform.position, biteSfxVolume);
            if (biteVfx) Instantiate(biteVfx, _pendingTarget.transform.position, Quaternion.identity);

            // ★ Exp +1 (Bite로만)
            _player?.AddExpFromBite(1);

            // ★ 포만감 (원하면 조정)
            EatBar.Instance?.AddFromEat(10);

            _pendingTarget.KillSilently();

            if (debugLog) Debug.Log("[Bite] 성공 처리 완료 (Exp+1, EatBar+10)");
        }
        _pendingTarget = null;
    }

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
        var ctr = _anim.runtimeAnimatorController;
        if (ctr == null) return 0f;
        var clips = ctr.animationClips;
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
