// DamageFromLoadout.cs
// - "총알 프리팹" 또는 "근접 히트박스"에 붙이기
// - OnEnable 시 플레이어의 PlayerLoadout을 읽어, 해당 무기 타입에 맞는 데미지를
//   이 오브젝트의 'damage' 계열 필드/프로퍼티/메서드(SetDamage)로 자동 주입(reflection)
// - 흔한 스크립트들과 최대한 호환되도록 구현
// - 무기 타입은 프리팹마다 지정(포크/숟가락/젓가락)

using System;
using System.Reflection;
using UnityEngine;

public class DamageFromLoadout : MonoBehaviour
{
    public PlayerLoadout.WeaponType weaponType = PlayerLoadout.WeaponType.Fork;
    public bool alsoSendMessage = true; // SetDamage, SetPower 등 메시지도 시도

    void OnEnable()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        if (!player) return;

        var loadout = player.GetComponent<PlayerLoadout>();
        if (!loadout) return;

        float dmg = loadout.GetDamageFor(weaponType);

        // 1) 내가 가진 컴포넌트들에서 damage 계열 필드/프로퍼티를 찾아 넣는다
        var comps = GetComponents<MonoBehaviour>();
        foreach (var c in comps)
        {
            if (!c) continue;

            // 필드
            foreach (var f in c.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                if (!f.FieldType.IsAssignableFrom(typeof(float))) continue;
                string n = f.Name.ToLower();
                if (n is "damage" or "_damage" or "basedamage" or "dmg" or "power")
                {
                    try { f.SetValue(c, dmg); } catch { }
                }
            }

            // 프로퍼티
            foreach (var p in c.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                if (!p.CanWrite || p.PropertyType != typeof(float)) continue;
                string n = p.Name.ToLower();
                if (n is "damage" or "basedamage" or "dmg" or "power")
                {
                    try { p.SetValue(c, dmg, null); } catch { }
                }
            }

            // 메서드(SetDamage/SetPower)
            if (alsoSendMessage)
            {
                var m = c.GetType().GetMethod("SetDamage", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (m != null && m.GetParameters().Length == 1 && m.GetParameters()[0].ParameterType == typeof(float))
                {
                    try { m.Invoke(c, new object[] { dmg }); } catch { }
                }
                var m2 = c.GetType().GetMethod("SetPower", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (m2 != null && m2.GetParameters().Length == 1 && m2.GetParameters()[0].ParameterType == typeof(float))
                {
                    try { m2.Invoke(c, new object[] { dmg }); } catch { }
                }
            }
        }

        // 2) 최후 수단: 브로드캐스트 메시지
        if (alsoSendMessage)
        {
            SendMessage("SetDamage", dmg, SendMessageOptions.DontRequireReceiver);
            SendMessage("SetPower", dmg, SendMessageOptions.DontRequireReceiver);
        }
    }
}
