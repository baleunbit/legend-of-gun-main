// StageDirector.cs
// - 스테이지 진입 시 규칙/기믹을 적용(자동 부트스트랩 포함)
// - Door에서 ApplyStage를 호출하는 구조 그대로 사용 가능
// - 여긴 무기 타입 주입을 하지 않는다. PlayerLoadout만 갱신하면
//   발사체는 ProjectileDamageAuto가 "현재 무기 데미지"를 자동 샘플링한다.

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

    [Header("2스테 감속(강하게 체감)")]
    [Range(0.1f, 1f)] public float stage2_TimeScale = 0.6f;
    public float stage2_RigidbodyExtraDrag = 8f;

    [Header("3스테 미끄럼")]
    public float stage3_PlayerDrag = 0.05f;
    public float stage3_PlayerAngularDrag = 0.05f;

    [Header("4스테 화상")]
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
        // 시작 방 규칙 자동 적용
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

        // 무기/타수 규칙
        var wm = playerGO.GetComponent<WeaponManager>();
        if (wm) wm.ApplyStageRules(stage);

        // 공통 리셋
        Time.timeScale = 1f;
        var status = playerGO.GetComponent<PlayerStatusEffects>();
        if (!status) status = playerGO.AddComponent<PlayerStatusEffects>();
        status.ClearAll();
        DisableIfExists<RoomSlowAll>(roomGO);
        DisableIfExists<BurnDamageOverTime>(roomGO);

        // 스테이지 기믹
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
