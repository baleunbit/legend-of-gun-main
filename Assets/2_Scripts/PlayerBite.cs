using UnityEngine;


public class PlayerBite : MonoBehaviour
{
    [Header("Bite Settings")]
    public KeyCode biteKey = KeyCode.E;
    public float biteRange = 1.2f; // 왜: 잠입 근접 전용 범위
    public LayerMask enemyMask;
    public int fullnessOnBite = 20; // 왜: 시스템 연결 값 노출


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
        // 주변 적 수색 (원 범위)
        Collider2D[] hits = Physics2D.OverlapCircleAll(_tr.position, biteRange, enemyMask);
        EnemyAI best = null;
        float bestDist = float.MaxValue;


        foreach (var h in hits)
        {
            var ai = h.GetComponentInParent<EnemyAI>();
            if (ai == null) ai = h.GetComponent<EnemyAI>();
            if (ai == null) continue;
            if (ai.IsAlerted) continue; // 왜: 발각 상태면 한입 금지


            float d = Vector2.SqrMagnitude((Vector2)(ai.transform.position - _tr.position));
            if (d < bestDist)
            {
                bestDist = d;
                best = ai;
            }
        }


        if (best != null)
        {
            if (biteVfx) Instantiate(biteVfx, best.transform.position, Quaternion.identity);
            Destroy(best.gameObject);
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