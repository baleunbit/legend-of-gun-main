// BurnDamageOverTime.cs
// - �� ���ο� �ִ� ���� �÷��̾�� �ʴ� dps ��ŭ ȭ�� ����
// - Room�� �ݶ��̴� ���� �ȿ� �÷��̾ ���� ���� ����

using UnityEngine;

public class BurnDamageOverTime : MonoBehaviour
{
    [SerializeField] private float dps = 4f;
    private Transform player;
    private Room room;

    public void Init(Transform playerTr, float dpsValue)
    {
        player = playerTr;
        dps = dpsValue;
    }

    void Awake()
    {
        room = GetComponent<Room>();
        if (!room) room = GetComponentInParent<Room>();
    }

    void Update()
    {
        if (!enabled || !player || dps <= 0f || room == null) return;

        // �÷��̾ �� �������� ����
        if (!IsInsideRoom(player.position)) return;

        // �÷��̾� ü�� ó��(������� Health �ý��ۿ� �°� �Ʒ��� ����)
        var dmgRecv = player.GetComponent<IPlayerDamageReceiver>();
        if (dmgRecv != null)
        {
            dmgRecv.ApplyDamage(dps * Time.deltaTime);
        }
        else
        {
            // ����: "PlayerHealth" ������Ʈ�� TakeDamage(float) �޼��尡 �ִٸ�
            var any = player.GetComponent("PlayerHealth");
            if (any != null)
            {
                any.SendMessage("TakeDamage", dps * Time.deltaTime, SendMessageOptions.DontRequireReceiver);
            }
        }
    }

    private bool IsInsideRoom(Vector2 wp)
    {
        var cols = room.GetComponentsInChildren<Collider2D>(true);
        foreach (var c in cols) if (c && c.OverlapPoint(wp)) return true;
        return false;
    }
}

// ����: ������ �÷��̾� ������ �ý����� �� �������̽��� �����ϸ� ����ϰ� �۵�
public interface IPlayerDamageReceiver
{
    void ApplyDamage(float amount);
}
