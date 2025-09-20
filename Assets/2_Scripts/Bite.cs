using UnityEngine;

public class PlayerBite : MonoBehaviour
{
    [Header("Bite")]
    public KeyCode biteKey = KeyCode.E;
    public float biteRange = 1.4f;        // 필요하면 2.0까지 테스트
    public string enemyTag = "Mob";       // 몹 태그

    [Header("조건")]
    public bool requireStealth = true;    // 들키면 못 먹기
    public bool requireBackAngle = false; // 뒤에서만 허용할지
    [Range(0, 180)] public float backAngle = 120f; // 뒤에서 ±각도 허용

    [Header("VFX/SFX (옵션)")]
    public GameObject biteVfx;

    [Header("디버그")]
    public bool debugLog = false;

    Transform _tr;

    void Awake()
    {
        _tr = transform;
    }

    void Update()
    {
        if (Input.GetKeyDown(biteKey))
            TryBite();
    }

    void TryBite()
    {
        // ⛔ 레이어마스크 안 씀. 반경 내 모든 콜라이더 검색 후 "태그"로만 거름
        Collider2D[] hits = Physics2D.OverlapCircleAll(_tr.position, biteRange);

        Mob best = null;
        float bestDist = float.MaxValue;

        foreach (var h in hits)
        {
            // 태그로 필터링 (자식 콜라이더 대비해서 부모/본인 모두 검사)
            if (!(h.CompareTag(enemyTag) || (h.transform.parent && h.transform.parent.CompareTag(enemyTag))))
                continue;

            var mob = h.GetComponentInParent<Mob>() ?? h.GetComponent<Mob>();
            if (mob == null) continue;

            // 스텔스 요구 시, 발각된 몹은 제외
            if (requireStealth && mob.IsAlerted)
            {
                if (debugLog) Debug.Log("[Bite] 제외: 발각됨");
                continue;
            }

            // 뒤에서만 허용 옵션
            if (requireBackAngle && !IsBehindTarget(mob.transform))
            {
                if (debugLog) Debug.Log("[Bite] 제외: 후방 각도 아님");
                continue;
            }

            float d = ((Vector2)mob.transform.position - (Vector2)_tr.position).sqrMagnitude;
            if (d < bestDist)
            {
                bestDist = d;
                best = mob;
            }
        }

        if (best != null)
        {
            if (biteVfx) Instantiate(biteVfx, best.transform.position, Quaternion.identity);
            best.KillSilently(); // 바로 사망 처리
            if (debugLog) Debug.Log("[Bite] 성공!");
        }
        else
        {
            if (debugLog) Debug.Log("[Bite] 대상 없음 (범위/발각/각도/태그 확인)");
        }
    }

    bool IsBehindTarget(Transform target)
    {
        // 몹의 좌/우를 스프라이트 기준으로 간단히 계산
        // flipX가 true면 왼쪽을 바라보는 것으로 가정
        var sr = target.GetComponentInChildren<SpriteRenderer>();
        Vector2 forward = (sr != null && sr.flipX) ? Vector2.left : Vector2.right;

        Vector2 toPlayer = ((Vector2)_tr.position - (Vector2)target.position).normalized;
        float ang = Vector2.Angle(forward, toPlayer); // 타겟이 보는 방향에서 플레이어까지 각도
        // 뒤쪽이면 ang가 큰 값(≈180). backAngle만큼의 원뿔 뒤쪽에 있는지 판단
        return ang >= (180f - backAngle * 0.5f);
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, biteRange);
    }
#endif
}
