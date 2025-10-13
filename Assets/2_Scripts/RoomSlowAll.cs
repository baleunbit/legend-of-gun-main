// RoomSlowAll.cs
// - �� ������ ��� Rigidbody2D�� "�߰� �巡��"�� �ο��Ͽ� �̵��� ��ȭ
// - ���� ������(���� ������ �̵�) �� �濡�� �� ������Ʈ�� ���ų� �ٸ� ������ ����
// - ���� drag�� ������ �ξ��ٰ� Disable �� ����

using System.Collections.Generic;
using UnityEngine;

public class RoomSlowAll : MonoBehaviour
{
    [Range(0.1f, 1f)] public float speedScale = 0.6f; // 1=����, 0.6=40%����
    [SerializeField] private float extraDragAtScale06 = 6f; // scale 0.6�� �� ������ drag ��(Ʃ�׿�)

    private readonly List<Rigidbody2D> rbs = new();
    private readonly Dictionary<Rigidbody2D, float> originalDrag = new();

    public void Init(float scale) => speedScale = Mathf.Clamp(scale, 0.1f, 1f);

    void OnEnable()
    {
        CacheRigidbodies();
        ApplySlow();
    }

    void OnDisable()
    {
        Restore();
    }

    private void CacheRigidbodies()
    {
        rbs.Clear();
        originalDrag.Clear();

        var bodies = GetComponentsInChildren<Rigidbody2D>(true);
        foreach (var rb in bodies)
        {
            if (!rb) continue;
            rbs.Add(rb);
            if (!originalDrag.ContainsKey(rb)) originalDrag[rb] = rb.linearDamping;
        }
    }

    private void ApplySlow()
    {
        float t = Mathf.InverseLerp(1f, 0.6f, speedScale); // 1��0, 0.6��1
        float extra = Mathf.Lerp(0f, extraDragAtScale06, t);

        foreach (var rb in rbs)
        {
            if (!rb) continue;
            if (!originalDrag.TryGetValue(rb, out float baseDrag)) baseDrag = rb.linearDamping;
            rb.linearDamping = baseDrag + extra;
        }
    }

    private void Restore()
    {
        foreach (var kv in originalDrag)
        {
            if (kv.Key) kv.Key.linearDamping = kv.Value;
        }
        originalDrag.Clear();
        rbs.Clear();
    }
}
