// BurnDamageOverTime.cs (4스테 화상)

using UnityEngine;

public class BurnDamageOverTime : MonoBehaviour
{
    [SerializeField] private float dps = 10f;
    private Transform player;
    private Room room;

    public void Init(Transform playerTr, float dpsValue)
    {
        player = playerTr;
        dps = dpsValue;
    }

    void Awake()
    {
        room = GetComponent<Room>() ?? GetComponentInParent<Room>();
    }

    void Update()
    {
        if (!enabled || !player || dps <= 0f || room == null) return;
        if (!IsInsideRoom(player.position)) return;

        var recv = player.GetComponent<IPlayerDamageReceiver>();
        if (recv != null) recv.ApplyDamage(dps * Time.deltaTime);
        else player.SendMessage("TakeDamage", dps * Time.deltaTime, SendMessageOptions.DontRequireReceiver);
    }

    private bool IsInsideRoom(Vector2 wp)
    {
        var cols = room.GetComponentsInChildren<Collider2D>(true);
        foreach (var c in cols) if (c && c.OverlapPoint(wp)) return true;
        return false;
    }
}

public interface IPlayerDamageReceiver { void ApplyDamage(float amount); }
