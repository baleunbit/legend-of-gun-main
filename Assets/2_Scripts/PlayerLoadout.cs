// PlayerLoadout.cs
// - "무기 타입을 프리팹에 고정"하지 않고, 플레이어가 들고 있는 현재 무기에 따라
//   데미지를 중앙에서 계산해 준다. (스테이지 바뀌면 규칙도 자동 반영)
// - 마우스 휠로 무기 변경 가능(요구 반영)
// - 1~2: 포크 주무기, 3: 숟가락, 4: 젓가락
// - 몹 기본 HP = 30 → 주무기 3타 / 비주무기 6타

using UnityEngine;
using System;

[DisallowMultipleComponent]
public class PlayerLoadout : MonoBehaviour
{
    public enum WeaponType { Fork, Spoon, Chopstick }

    [Header("Rule")]
    [SerializeField] private float enemyBaseHP = 30f;

    [Header("Runtime (read only)")]
    public int currentStage = 1;
    public WeaponType mainWeapon = WeaponType.Fork;   // 스테이지가 정하는 "주무기"
    public WeaponType currentWeapon = WeaponType.Fork; // 현재 플레이어가 들고있는 무기(휠로 변경)

    public event Action<WeaponType> OnWeaponChanged;

    void Update()
    {
        // 마우스 휠로 무기 변경 (요청사항)
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.01f)
        {
            int dir = scroll > 0 ? 1 : -1;
            int next = ((int)currentWeapon + dir) % 3;
            if (next < 0) next += 3;
            SetCurrentWeapon((WeaponType)next);
        }
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

        // 스테이지가 바뀌면 기본적으로 주무기를 손에 쥐게 함(원하면 주석 처리)
        SetCurrentWeapon(mainWeapon);
    }

    public void SetCurrentWeapon(WeaponType w)
    {
        if (currentWeapon == w) return;
        currentWeapon = w;
        OnWeaponChanged?.Invoke(currentWeapon);
    }

    // 현재 들고 있는 무기 기준 데미지(주무기 3타, 비주무기 6타)
    public float GetCurrentDamage()
    {
        return (currentWeapon == mainWeapon) ? (enemyBaseHP / 3f) : (enemyBaseHP / 6f);
    }

    // 특정 무기로 공격할 때의 데미지(발사체가 "나는 지금 포크 탄환이야"라고 안 알려도 됨;
    // 스테이지 규칙 + 현재 장착 무기로 중앙에서 계산)
    public float GetDamageFor(WeaponType weaponToUse)
    {
        bool isMain = (weaponToUse == mainWeapon);
        return isMain ? (enemyBaseHP / 3f) : (enemyBaseHP / 6f);
    }
}
