// StageDirector.cs
// - �������� ���� �� �ֹ���/������ ��Ģ ����
// - �������� 2: �� ��ü ������(�÷��̾�+��)  �� RoomSlowAll
// - �������� 3: ���� �̲���(�÷��̾)     �� PlayerStatusEffects(slippery)
// - �������� 4: ȭ�� ��Ʈ(�÷��̾�)        �� BurnDamageOverTime
// - ������ �̸� "1_*", "2_*"�� ���λ�� �������� �Ǻ�

using UnityEngine;

public class StageDirector : MonoBehaviour
{
    public static StageDirector Instance { get; private set; }

    [Header("��� �⺻��")]
    [SerializeField, Range(0.1f, 1f)] private float stage2SpeedScale = 0.6f; // 1=����, 0.6=40%����
    [SerializeField] private float stage3PlayerDrag = 0.2f;                  // �̲��� �� �÷��̾� rb.drag
    [SerializeField] private float stage4BurnDps = 4f;                        // �ʴ� ȭ�� ������

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

        // 1) ����/������ ��Ģ
        var loadout = playerGO.GetComponent<PlayerLoadout>();
        if (loadout) loadout.ApplyStageRules(stage);

        // �÷��̾� ���� ������Ʈ Ȯ��
        var status = playerGO.GetComponent<PlayerStatusEffects>();
        if (!status) status = playerGO.AddComponent<PlayerStatusEffects>();

        // �� ��� ����
        var slow = enteredRoom.GetComponent<RoomSlowAll>();
        var burn = enteredRoom.GetComponent<BurnDamageOverTime>();
        if (slow) slow.enabled = false;
        if (burn) burn.enabled = false;
        status.SetSlippery(false, 0f);

        // 2) ���������� ���
        switch (stage)
        {
            case 1:
                // ��� ����
                break;

            case 2: // ��ü �̼� ����(�÷��̾�+��)
                slow = Ensure<RoomSlowAll>(enteredRoom);
                slow.Init(stage2SpeedScale);
                slow.enabled = true;
                break;

            case 3: // ����: �÷��̾� �̲���
                status.SetSlippery(true, stage3PlayerDrag);
                break;

            case 4: // ȭ��: �÷��̾� DoT
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
