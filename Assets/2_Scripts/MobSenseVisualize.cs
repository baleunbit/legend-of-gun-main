using UnityEngine;

[RequireComponent(typeof(Mob))]
public class MobSenseVisualizerClear : MonoBehaviour
{
    [Range(12, 256)] public int segments = 64;
    public float lineWidth = 0.035f;
    [Range(0f, 1f)] public float alpha = 0.35f;

    public Color ringColor = new(0.2f, 0.7f, 1f, 0.6f); // 근접(원)
    public Color fovColor = new(1f, 0.9f, 0.1f, 0.6f); // 시야(부채꼴)

    Mob mob;
    LineRenderer ring;    // 근접 원
    LineRenderer fan;     // 시야 부채꼴 (중심 포함)

    void Awake()
    {
        mob = GetComponent<Mob>();
        ring = MakeLR("SenseRing");
        fan = MakeLR("SenseFOV");
    }

    LineRenderer MakeLR(string n)
    {
        var go = new GameObject(n);
        go.transform.SetParent(transform, false);
        var lr = go.AddComponent<LineRenderer>();
        lr.useWorldSpace = true;
        lr.widthMultiplier = lineWidth;
        lr.numCapVertices = 4;
        lr.numCornerVertices = 2;
        lr.material = new Material(Shader.Find("Sprites/Default"));
        return lr;
    }

    void Update()
    {
        DrawRing();
        DrawFan();
    }

    void DrawRing()
    {
        float r = Mathf.Max(0.01f, mob.detectRadius);
        int N = Mathf.Max(12, segments);
        ring.positionCount = N + 1;
        var c = ringColor; c.a = alpha;
        ring.startColor = ring.endColor = c;

        Vector3 center = transform.position;
        for (int i = 0; i <= N; i++)
        {
            float t = (float)i / N * Mathf.PI * 2f;
            Vector3 p = new Vector3(Mathf.Cos(t), Mathf.Sin(t), 0f) * r + center;
            ring.SetPosition(i, p);
        }
    }

    void DrawFan()
    {
        float dist = Mathf.Max(0.01f, mob.viewDistance);
        float half = Mathf.Clamp(mob.fovAngle * 0.5f, 0f, 180f);
        int N = Mathf.Max(12, segments / 2);

        // Mob 내부에서 관리하는 정면을 얻기 위해 작은 헬퍼 제공했다고 가정 (없어도 sprite flip으로 근사)
        Vector2 forward;
        var sr = GetComponentInChildren<SpriteRenderer>();
        forward = (sr != null && sr.flipX) ? Vector2.left : Vector2.right;

        // 부채꼴: [중심] + [호를 따라 N+1점] + [다시 중심]
        fan.positionCount = N + 3;
        var c = fovColor; c.a = alpha;
        fan.startColor = fan.endColor = c;

        Vector3 center = transform.position;
        fan.SetPosition(0, center);

        float start = -half;
        for (int i = 0; i <= N; i++)
        {
            float a = start + (half * 2f) * (i / (float)N);
            Vector2 dir = Quaternion.Euler(0, 0, a) * forward;
            Vector3 p = (Vector3)(dir.normalized * dist) + center;
            fan.SetPosition(1 + i, p);
        }

        fan.SetPosition(N + 2, center);
    }
}
