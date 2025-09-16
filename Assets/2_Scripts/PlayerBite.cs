using UnityEngine;

public class PlayerBite : MonoBehaviour
{
    [Header("Bite")]
    public KeyCode biteKey = KeyCode.E;
    public float biteRange = 1.2f;
    public LayerMask enemyMask;          // 몹 콜라이더가 있는 레이어
    public GameObject biteVfx;           // (선택) 효과 프리팹
    public int fullnessOnBite = 20;      // (선택) 배고픔 채우기

    Transform _tr;
    PlayerHungry _hungry;                // 없으면 null

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
        // 범위 내 콜라이더 전부
        var hits = Physics2D.OverlapCircleAll(_tr.position, biteRange, enemyMask);

        Mob best = null;
        float bestDist = float.MaxValue;

        foreach (var h in hits)
        {
            var mob = h.GetComponentInParent<Mob>();
            if (!mob) mob = h.GetComponent<Mob>();
            if (!mob) continue;

            // 들키지 않은 적만
            if (mob.IsAlerted) continue;

            float d = (mob.transform.position - _tr.position).sqrMagnitude;
            if (d < bestDist) { bestDist = d; best = mob; }
        }

        if (!best) return;

        if (biteVfx) Instantiate(biteVfx, best.transform.position, Quaternion.identity);
        best.KillSilently();                 // 🚩 한입 처리
        if (_hungry) _hungry.Add(fullnessOnBite);
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, biteRange);
    }
#endif
}
