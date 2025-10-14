// StageDirector.cs
// - �������� ���� �� ��Ģ/����� ����(�ڵ� ��Ʈ��Ʈ�� ����)
// - Door���� ApplyStage�� ȣ���ϴ� ���� �״�� ��� ����
// - ���� ���� Ÿ�� ������ ���� �ʴ´�. PlayerLoadout�� �����ϸ�
//   �߻�ü�� ProjectileDamageAuto�� "���� ���� ������"�� �ڵ� ���ø��Ѵ�.

using UnityEngine;

[DefaultExecutionOrder(-200)]
public class StageDirector : MonoBehaviour
{
    private static StageDirector _inst;
    public static StageDirector Instance
    {
        get
        {
            if (_inst) return _inst;
            _inst = FindFirstObjectByType<StageDirector>(FindObjectsInactive.Include);
            if (_inst) return _inst;
            var go = new GameObject("StageDirector");
            _inst = go.AddComponent<StageDirector>();
            return _inst;
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetDomain() { _inst = null; }

    [Header("2���� ����(���ϰ� ü��)")]
    [Range(0.1f, 1f)] public float stage2_TimeScale = 0.6f;
    public float stage2_RigidbodyExtraDrag = 8f;

    [Header("3���� �̲���")]
    public float stage3_PlayerDrag = 0.05f;
    public float stage3_PlayerAngularDrag = 0.05f;

    [Header("4���� ȭ��")]
    public float stage4_BurnDps = 10f;

    public int CurrentStage { get; private set; } = 1;

    void Awake()
    {
        if (_inst && _inst != this) { Destroy(gameObject); return; }
        _inst = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        // ���� �� ��Ģ �ڵ� ����
        var player = GameObject.FindGameObjectWithTag("Player");
        if (!player) return;
        var room = FindRoomByPosition(player.transform.position);
        if (!room) return;
        ApplyStage(ParseStageFromName(room.gameObject.name), room.gameObject, player);
    }

    public void ApplyStage(int stage, GameObject roomGO, GameObject playerGO)
    {
        if (stage <= 0) stage = 1;
        CurrentStage = stage;

        // ����/Ÿ�� ��Ģ
        var wm = playerGO.GetComponent<WeaponManager>();
        if (wm) wm.ApplyStageRules(stage);

        // ���� ����
        Time.timeScale = 1f;
        var status = playerGO.GetComponent<PlayerStatusEffects>();
        if (!status) status = playerGO.AddComponent<PlayerStatusEffects>();
        status.ClearAll();
        DisableIfExists<RoomSlowAll>(roomGO);
        DisableIfExists<BurnDamageOverTime>(roomGO);

        // �������� ���
        switch (stage)
        {
            case 1: break;

            case 2:
                Time.timeScale = stage2_TimeScale;
                var slow = roomGO.GetComponent<RoomSlowAll>() ?? roomGO.AddComponent<RoomSlowAll>();
                slow.Init(stage2_RigidbodyExtraDrag);
                slow.enabled = true;
                break;

            case 3:
                status.SetSlippery(true, stage3_PlayerDrag, stage3_PlayerAngularDrag);
                break;

            case 4:
                var burn = roomGO.GetComponent<BurnDamageOverTime>() ?? roomGO.AddComponent<BurnDamageOverTime>();
                burn.Init(playerGO.transform, stage4_BurnDps);
                burn.enabled = true;
                break;
        }
    }

    public static int ParseStageFromName(string roomName)
    {
        if (string.IsNullOrEmpty(roomName)) return 1;
        int us = roomName.IndexOf('_');
        string head = us > 0 ? roomName[..us] : roomName;
        return int.TryParse(head, out int s) ? s : 1;
    }

    private void DisableIfExists<T>(GameObject host) where T : Behaviour
    {
        var c = host.GetComponent<T>();
        if (c) c.enabled = false;
    }

    private Room FindRoomByPosition(Vector2 pos)
    {
        var rooms = FindObjectsByType<Room>(FindObjectsSortMode.None);
        Room best = null;
        float bestDist = float.MaxValue;

        foreach (var r in rooms)
        {
            if (!r) continue;
            var cols = r.GetComponentsInChildren<Collider2D>(true);
            foreach (var c in cols)
                if (c && c.OverlapPoint(pos)) return r;

            float d = ((Vector2)r.transform.position - pos).sqrMagnitude;
            if (d < bestDist) { bestDist = d; best = r; }
        }
        return best;
    }
}
