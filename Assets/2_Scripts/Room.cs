using System.Linq;
using UnityEngine;

[DisallowMultipleComponent]
public class Room : MonoBehaviour
{
    [SerializeField] public int roomID; // 접근 문제 해결도 겸해 공개
    Collider2D[] triggerInners;   // 내부(실내)로 간주할 Trigger 콜라이더들
    Collider2D[] solidColliders;  // 참고용(스냅시 사용)
    Bounds aabb;
    Transform[] spawnPoints;

    void Awake() => Init();
#if UNITY_EDITOR
    void OnValidate() { if (!Application.isPlaying) Init(); }
#endif

    void Init()
    {
        var all = GetComponentsInChildren<Collider2D>(true);
        triggerInners = all.Where(c => c && c.isTrigger).ToArray();
        solidColliders = all.Where(c => c && !c.isTrigger).ToArray();

        // AABB: 콜라이더 우선, 없으면 렌더러 합성, 그래도 없으면 1x1
        Bounds? b = null;
        foreach (var c in all) { if (c == null) continue; b = b == null ? c.bounds : Enc(b.Value, c.bounds); }
        if (b == null)
        {
            var rends = GetComponentsInChildren<Renderer>(true);
            foreach (var r in rends) { if (r == null) continue; b = b == null ? r.bounds : Enc(b.Value, r.bounds); }
        }
        aabb = b ?? new Bounds(transform.position, Vector3.one);

        // 스폰포인트
        spawnPoints = GetComponentsInChildren<SpawnPoint>(true).Select(s => s.transform).ToArray();
    }

    Bounds Enc(Bounds a, Bounds add) { a.Encapsulate(add); return a; }

    public Bounds AABB => aabb;
    public Transform[] SpawnPoints => spawnPoints;

    // ★ 내부 판정 규칙:
    //  - Trigger 콜라이더가 1개 이상 있으면, 반드시 그 트리거 중 하나와 OverlapPoint여야 "실내"
    //  - 트리거가 없으면: 랜덤 스폰은 허용하지 않도록 ContainsRandom용에서 false를 돌려 안전하게.
    public bool ContainsForSpawnPoints(Vector2 wp)
    {
        // 스폰포인트는 '조정' 용으로만 체크: 트리거가 있으면 내부 보정, 없으면 그냥 사용(스폰포인트는 디자이너가 안쪽에 둔다고 가정)
        if (triggerInners != null && triggerInners.Length > 0)
            return triggerInners.Any(t => t.OverlapPoint(wp));
        return true;
    }

    public bool ContainsForRandom(Vector2 wp)
    {
        if (triggerInners != null && triggerInners.Length > 0)
            return triggerInners.Any(t => t.OverlapPoint(wp));
        // 트리거가 없으면 랜덤 내부 스폰 금지
        return false;
    }

    public Vector2 SnapInside(Vector2 wp)
    {
        // 내부 트리거가 있으면 그쪽으로 스냅, 없으면 솔리드 기준으로 근접점
        var cols = (triggerInners != null && triggerInners.Length > 0) ? triggerInners : solidColliders;
        if (cols == null || cols.Length == 0) return wp;

        float best = float.MaxValue; Vector2 bestPt = wp;
        foreach (var c in cols)
        {
            if (c == null) continue;
            var cp = (Vector2)c.ClosestPoint(wp);
            float d = (cp - wp).sqrMagnitude;
            if (d < best) { best = d; bestPt = cp; }
        }
        return bestPt;
    }
}
