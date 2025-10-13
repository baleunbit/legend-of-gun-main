// ProjectileDamageAuto.cs
// - "�߻�ü/���� ��Ʈ�ڽ�"�� ���̴� ���� ������ ���� ��ũ��Ʈ
// - �����տ� ���� Ÿ���� �ھƵ��� �ʴ´�. ���� ���� �÷��̾��� PlayerLoadout�� ��ȸ��
//   "���� ��� �ִ� ����"�� �������� �ڵ����� ä���Ѵ�.
// - ��, ���������� �ٲ�ų� �ֹ��Ⱑ �ٲ� �߾� ��Ģ�� �ٲٸ� �˾Ƽ� ���󰣴�.
//
// ���:
//   1) ź/��Ʈ�ڽ� �����տ� �� ��ũ��Ʈ�� ���δ�.
//   2) ��/�� ��ũ��Ʈ���� �߻�(Ȱ��ȭ)�� �� ���� ���� ���� ��� ����.
//   3) OnTriggerEnter2D/OnCollisionEnter2D���� ������ �������� �ִ´�.
//      (IDamageable, EnemyHealth.TakeDamage(float) �� �� �ϳ��� ������ ��)

using UnityEngine;

[DisallowMultipleComponent]
public class ProjectileDamageAuto : MonoBehaviour
{
    [Header("�ʱ�ȭ �ɼ�")]
    public bool sampleDamageOnEnable = true; // �߻�/Ȱ��ȭ ���� �ڵ� ���ø�

    [Header("����� �����")]
    [SerializeField] private float sampledDamage = 0f;

    private bool damageSampled = false;

    void OnEnable()
    {
        if (sampleDamageOnEnable)
            SampleDamageFromPlayer();
    }

    // �ʿ�� ��/�� ��ũ��Ʈ���� ���� ȣ�� ����
    public void SampleDamageFromPlayer()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        if (!player) return;

        var loadout = player.GetComponent<PlayerLoadout>();
        if (!loadout) return;

        // "���� ��� �ִ� ����" ���� ������ ���
        sampledDamage = loadout.GetCurrentDamage();
        damageSampled = true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        TryApplyDamage(other.gameObject);
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        TryApplyDamage(col.collider.gameObject);
    }

    private void TryApplyDamage(GameObject target)
    {
        if (!damageSampled) SampleDamageFromPlayer();

        // 1) IDamageable �������̽� �켱
        var idmg = target.GetComponent<IDamageable>();
        if (idmg != null)
        {
            idmg.TakeDamage(sampledDamage);
            return;
        }

        // 2) EnemyHealth.TakeDamage(float) �޼��尡 ������ ȣ��
        target.SendMessage("TakeDamage", sampledDamage, SendMessageOptions.DontRequireReceiver);
    }
}

// ������ �̹� �� �������̽��� ���� �ִٸ� �״�� �����.
// ���ٸ� ���� ����.
public interface IDamageable
{
    void TakeDamage(float amount);
}
