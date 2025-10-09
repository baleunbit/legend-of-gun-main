using System.Linq;
using UnityEngine;

[DisallowMultipleComponent]
public class Room : MonoBehaviour
{
    [SerializeField] public int roomID;
    Collider2D[] triggerInners;
    Collider2D[] solidColliders;
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

        Bounds? b = null;
        foreach (var c in all) { if (c == null) continue; b = b == null ? c.bounds : Enc(b.Value, c.bounds); }
        if (b == null)
        {
            var rends = GetComponentsInChildren<Renderer>(true);
            foreach (var r in rends) { if (r == null) continue; b = b == null ? r.bounds : Enc(b.Value, r.bounds); }
        }
        aabb = b ?? new Bounds(transform.position, Vector3.one);

        spawnPoints = GetComponentsInChildren<SpawnPoint>(true).Select(s => s.transform).ToArray();
    }

    Bounds Enc(Bounds a, Bounds add) { a.Encapsulate(add); return a; }

    public Bounds AABB => aabb;
    public Transform[] SpawnPoints => spawnPoints;

    public bool ContainsForSpawnPoints(Vector2 wp)
    {
        if (triggerInners != null && triggerInners.Length > 0)
            return triggerInners.Any(t => t.OverlapPoint(wp));
        return true;
    }

    public bool ContainsForRandom(Vector2 wp)
    {
        if (triggerInners != null && triggerInners.Length > 0)
            return triggerInners.Any(t => t.OverlapPoint(wp));
        return false;
    }

    public Vector2 SnapInside(Vector2 wp)
    {
        var cols = (triggerInners != null && triggerInners.Length > 0) ? triggerInners : solidColliders;
        if (cols == null || cols.Length == 0) return wp;

        float best = float.MaxValue;
        Vector2 bestPt = wp;
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
