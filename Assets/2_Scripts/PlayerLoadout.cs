// PlayerLoadout.cs
// - 몹 기본 HP=30 기준으로 "주무기 3타 / 비주무기 6타"가 되도록 데미지 산출
// - 1~2 스테이지: 포크가 주무기, 3: 숟가락, 4: 젓가락
// - 총알/근접 스크립트는 GetDamageFor(currentWeapon)만 호출하면 됨

using UnityEngine;

public class PlayerLoadout : MonoBehaviour
{
    public enum WeaponType { Fork, Spoon, Chopstick }

    [Header("전투 규칙")]
    [SerializeField] private float enemyBaseHP = 30f;

    [Header("현재 상태(읽기 전용)")]
    public int currentStage = 1;
    public WeaponType mainWeapon = WeaponType.Fork;

    // 외부에서 조회
    public float GetDamageFor(WeaponType weapon)
    {
        // 주무기 3타 → 데미지 = HP/3, 비주무기 6타 → HP/6
        float dmgMain = enemyBaseHP / 3f;
        float dmgSub = enemyBaseHP / 6f;
        return (weapon == mainWeapon) ? dmgMain : dmgSub;
    }

    public void ApplyStageRules(int stage)
    {
        currentStage = stage;
        mainWeapon = stage switch
        {
            1 => WeaponType.Fork,
            2 => WeaponType.Fork,
            3 => WeaponType.Spoon,
            4 => WeaponType.Chopstick,
            _ => WeaponType.Fork
        };
    }
}
