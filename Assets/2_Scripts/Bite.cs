using UnityEngine;

public class PlayerBite : MonoBehaviour
{
    [Header("Bite Settings")]
    public KeyCode biteKey = KeyCode.E;
    public float biteRange = 2f;
    public LayerMask enemyMask;
    public int fullnessOnBite = 20;

    [Header("VFX/SFX (optional)")]
    public GameObject biteVfx;

    Transform _tr;
    PlayerHungry _hungry;

    void Awake()
    {
        _tr = transform;
        _hungry = GetComponent<PlayerHungry>();
    }

    void Update()
    {
        if (Input.GetKeyDown(biteKey)) TryBite();
    }

    void TryBite()
    {
        // 주변 적 탐색
        Collider2D[] hits = Physics2D.OverlapCircleAll(_tr.position, biteRange, enemyMask);
        Mob best = null;
        float bestDist = float.MaxValue;

        foreach (var h in hits)
        {
            var mob = h.GetComponentInParent<Mob>() ?? h.GetComponent<Mob>();
            if (mob == null) continue;
            if (mob.IsAlerted) continue; // 이미 들켰으면 은밀 처치 불가

            float d = ((Vector2)mob.transform.position - (Vector2)_tr.position).sqrMagnitude;
            if (d < bestDist)
            {
                bestDist = d;
                best = mob;
            }
        }

        if (best != null)
        {
            // VFX
            if (biteVfx) Instantiate(biteVfx, best.transform.position, Quaternion.identity);

            // ✅ 사망 처리
            best.KillSilently();

            // 포만감 시스템 연동
            if (_hungry) _hungry.Add(fullnessOnBite);
        }
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, biteRange);
    }
#endif
}
