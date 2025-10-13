// ProjectileDamageAuto.cs
// - "발사체/근접 히트박스"에 붙이는 범용 데미지 제공 스크립트
// - 프리팹에 무기 타입을 박아두지 않는다. 스폰 순간 플레이어의 PlayerLoadout을 조회해
//   "현재 들고 있는 무기"의 데미지를 자동으로 채택한다.
// - 즉, 스테이지가 바뀌거나 주무기가 바뀌어도 중앙 규칙만 바꾸면 알아서 따라간다.
//
// 사용:
//   1) 탄/히트박스 프리팹에 이 스크립트만 붙인다.
//   2) 총/검 스크립트에서 발사(활성화)할 때 별도 세팅 없이 사용 가능.
//   3) OnTriggerEnter2D/OnCollisionEnter2D에서 적에게 데미지를 넣는다.
//      (IDamageable, EnemyHealth.TakeDamage(float) 둘 중 하나만 있으면 됨)

using UnityEngine;

[DisallowMultipleComponent]
public class ProjectileDamageAuto : MonoBehaviour
{
    [Header("초기화 옵션")]
    public bool sampleDamageOnEnable = true; // 발사/활성화 순간 자동 샘플링

    [Header("디버그 보기용")]
    [SerializeField] private float sampledDamage = 0f;

    private bool damageSampled = false;

    void OnEnable()
    {
        if (sampleDamageOnEnable)
            SampleDamageFromPlayer();
    }

    // 필요시 총/검 스크립트에서 직접 호출 가능
    public void SampleDamageFromPlayer()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        if (!player) return;

        var loadout = player.GetComponent<PlayerLoadout>();
        if (!loadout) return;

        // "현재 들고 있는 무기" 기준 데미지 사용
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

        // 1) IDamageable 인터페이스 우선
        var idmg = target.GetComponent<IDamageable>();
        if (idmg != null)
        {
            idmg.TakeDamage(sampledDamage);
            return;
        }

        // 2) EnemyHealth.TakeDamage(float) 메서드가 있으면 호출
        target.SendMessage("TakeDamage", sampledDamage, SendMessageOptions.DontRequireReceiver);
    }
}

// 게임이 이미 이 인터페이스를 쓰고 있다면 그대로 연결됨.
// 없다면 선택 사항.
public interface IDamageable
{
    void TakeDamage(float amount);
}
