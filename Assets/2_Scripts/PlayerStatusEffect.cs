// PlayerStatusEffects.cs
// - �÷��̾� ���� ����ȿ�� ����(�̲���/��Ÿ)
// - �̲���: rb.drag�� ���缭 ���� ũ��, ���� �� ���� drag ����

using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerStatusEffects : MonoBehaviour
{
    private Rigidbody2D rb;
    private float baseDrag;
    private bool baseDragSaved = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (!baseDragSaved) { baseDrag = rb.linearDamping; baseDragSaved = true; }
    }

    public void SetSlippery(bool enable, float slipperyDrag = 0.2f)
    {
        if (!rb) return;

        if (enable)
        {
            if (!baseDragSaved) { baseDrag = rb.linearDamping; baseDragSaved = true; }
            rb.linearDamping = Mathf.Max(0f, slipperyDrag);
        }
        else
        {
            if (baseDragSaved) rb.linearDamping = baseDrag;
        }
    }
}
