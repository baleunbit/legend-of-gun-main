// DamageFromLoadout.cs
// - "�Ѿ� ������" �Ǵ� "���� ��Ʈ�ڽ�"�� ���̱�
// - OnEnable �� �÷��̾��� PlayerLoadout�� �о�, �ش� ���� Ÿ�Կ� �´� ��������
//   �� ������Ʈ�� 'damage' �迭 �ʵ�/������Ƽ/�޼���(SetDamage)�� �ڵ� ����(reflection)
// - ���� ��ũ��Ʈ��� �ִ��� ȣȯ�ǵ��� ����
// - ���� Ÿ���� �����ո��� ����(��ũ/������/������)

using System;
using System.Reflection;
using UnityEngine;

public class DamageFromLoadout : MonoBehaviour
{
    public PlayerLoadout.WeaponType weaponType = PlayerLoadout.WeaponType.Fork;
    public bool alsoSendMessage = true; // SetDamage, SetPower �� �޽����� �õ�

    void OnEnable()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        if (!player) return;

        var loadout = player.GetComponent<PlayerLoadout>();
        if (!loadout) return;

        float dmg = loadout.GetDamageFor(weaponType);

        // 1) ���� ���� ������Ʈ�鿡�� damage �迭 �ʵ�/������Ƽ�� ã�� �ִ´�
        var comps = GetComponents<MonoBehaviour>();
        foreach (var c in comps)
        {
            if (!c) continue;

            // �ʵ�
            foreach (var f in c.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                if (!f.FieldType.IsAssignableFrom(typeof(float))) continue;
                string n = f.Name.ToLower();
                if (n is "damage" or "_damage" or "basedamage" or "dmg" or "power")
                {
                    try { f.SetValue(c, dmg); } catch { }
                }
            }

            // ������Ƽ
            foreach (var p in c.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                if (!p.CanWrite || p.PropertyType != typeof(float)) continue;
                string n = p.Name.ToLower();
                if (n is "damage" or "basedamage" or "dmg" or "power")
                {
                    try { p.SetValue(c, dmg, null); } catch { }
                }
            }

            // �޼���(SetDamage/SetPower)
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

        // 2) ���� ����: ��ε�ĳ��Ʈ �޽���
        if (alsoSendMessage)
        {
            SendMessage("SetDamage", dmg, SendMessageOptions.DontRequireReceiver);
            SendMessage("SetPower", dmg, SendMessageOptions.DontRequireReceiver);
        }
    }
}
