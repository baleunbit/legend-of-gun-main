// StageDirector.cs
// - 스테이지 진입 시 주무기/데미지 규칙 적용
// - 스테이지 2: 방 전체 느려짐(플레이어+몹)  ─ RoomSlowAll
// - 스테이지 3: 얼음 미끄럼(플레이어만)     ─ PlayerStatusEffects(slippery)
// - 스테이지 4: 화상 도트(플레이어)        ─ BurnDamageOverTime
// - 프리팹 이름 "1_*", "2_*"… 접두사로 스테이지 판별

using UnityEngine;

public class StageDirector : MonoBehaviour
{
    public static StageDirector Instance { get; private set; }

    [Header("기믹 기본값")]
    [SerializeField, Range(0.1f, 1f)] private float stage2SpeedScale = 0.6f; // 1=정상, 0.6=40%감속
    [SerializeField] private float stage3PlayerDrag = 0.2f;                  // 미끄럼 시 플레이어 rb.drag
    [SerializeField] private float stage4BurnDps = 4f;                        // 초당 화상 데미지

    public int CurrentStage { get; private set; } = 1;

    void Awake()
    {
        if (Instance && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void ApplyStage(int stage, GameObject enteredRoom, GameObject playerGO)
    {
        if (stage <= 0) stage = 1;
        CurrentStage = stage;

        // 1) 무기/데미지 규칙
        var loadout = playerGO.GetComponent<PlayerLoadout>();
        if (loadout) loadout.ApplyStageRules(stage);

        // 플레이어 상태 컴포넌트 확보
        var status = playerGO.GetComponent<PlayerStatusEffects>();
        if (!status) status = playerGO.AddComponent<PlayerStatusEffects>();

        // 방 기믹 리셋
        var slow = enteredRoom.GetComponent<RoomSlowAll>();
        var burn = enteredRoom.GetComponent<BurnDamageOverTime>();
        if (slow) slow.enabled = false;
        if (burn) burn.enabled = false;
        status.SetSlippery(false, 0f);

        // 2) 스테이지별 기믹
        switch (stage)
        {
            case 1:
                // 기믹 없음
                break;

            case 2: // 전체 이속 감소(플레이어+몹)
                slow = Ensure<RoomSlowAll>(enteredRoom);
                slow.Init(stage2SpeedScale);
                slow.enabled = true;
                break;

            case 3: // 얼음: 플레이어 미끄럼
                status.SetSlippery(true, stage3PlayerDrag);
                break;

            case 4: // 화상: 플레이어 DoT
                burn = Ensure<BurnDamageOverTime>(enteredRoom);
                burn.Init(playerGO.transform, stage4BurnDps);
                burn.enabled = true;
                break;
        }
    }

    public static int ParseStageFromName(string roomName)
    {
        if (string.IsNullOrEmpty(roomName)) return 1;
        int us = roomName.IndexOf('_');
        string head = us > 0 ? roomName.Substring(0, us) : roomName;
        return int.TryParse(head, out int s) ? s : 1;
    }

    private T Ensure<T>(GameObject host) where T : Component
    {
        var c = host.GetComponent<T>();
        return c ? c : host.AddComponent<T>();
    }
}
