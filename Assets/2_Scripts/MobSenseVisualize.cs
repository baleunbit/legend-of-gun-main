using UnityEngine;

/// 몹 탐지 가시화(항상 표시): 파란 원=근접/소리, 노란 부채꼴=시야, 흰 화살표=정면
[RequireComponent(typeof(Mob))]
public class MobSenseVisualizerClear : MonoBehaviour
{
    [Header("스타일")]
    [Range(8, 256)] public int circleSegments = 64;
    public float lineWidth = 0.035f;
    [Range(0f, 1f)] public float alpha = 0.35f;

    [Header("색상")]
    public Color proximityColor = new(0.2f, 0.7f, 1f, 0.6f); // 근접 원(파랑)
    public Color fovColor = new(1f, 0.9f, 0.1f, 0.6f); // 시야 부채꼴(노랑)
    public Color forwardColor = new(1f, 1f, 1f, 0.9f);     // 정면 화살표(흰색)

    Mob mob;
    LineRenderer ringLR;    // 근접 원
    LineRenderer fovLR;     // 시야 호
    Camera cam;

    void Awake()
    {
        mob = GetComponent<Mob>();
        cam = Camera.main;

        ringLR = CreateLR("SenseRing");
        fovLR = CreateLR("SenseFOV");
    }

    LineRenderer CreateLR(string name)
    {
        var go = new GameObject(name);
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
        DrawProximityRing();
        DrawFOVArc();
    }

    void DrawProximityRing()
    {
        float r = Mathf.Max(0.01f, mob.detectRadius);
        int N = Mathf.Max(8, circleSegments);

        ringLR.positionCount = N + 1;
        var c = proximityColor; c.a = alpha;
        ringLR.startColor = ringLR.endColor = c;

        Vector3 center = transform.position;
        for (int i = 0; i <= N; i++)
        {
            float t = (float)i / N * Mathf.PI * 2f;
            Vector3 p = new Vector3(Mathf.Cos(t), Mathf.Sin(t), 0f) * r + center;
            ringLR.SetPosition(i, p);
        }
    }

    void DrawFOVArc()
    {
        float dist = Mathf.Max(0.01f, mob.viewDistance);
        float half = Mathf.Clamp(mob.fovAngle * 0.5f, 0f, 180f);
        int N = Mathf.Max(8, circleSegments / 2);

        var sr = GetComponentInChildren<SpriteRenderer>();
        Vector2 forward = (sr != null && sr.flipX) ? Vector2.left : Vector2.right;

        fovLR.positionCount = N + 1;
        var c = fovColor; c.a = alpha;
        fovLR.startColor = fovLR.endColor = c;

        Vector3 center = transform.position;
        float start = -half;
        for (int i = 0; i <= N; i++)
        {
            float a = start + (half * 2f) * (i / (float)N);
            Vector2 dir = Quaternion.Euler(0, 0, a) * forward;
            Vector3 p = (Vector3)(dir.normalized * dist) + center;
            fovLR.SetPosition(i, p);
        }
    }
}
