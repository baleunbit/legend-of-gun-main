// SharedAmmo.cs
using UnityEngine;

public class SharedAmmo : MonoBehaviour
{
    [Header("���� ź�� ����")]
    public int maxAmmo = 6;
    public int currentAmmo = 6;

    public void Clamp()
    {
        currentAmmo = Mathf.Clamp(currentAmmo, 0, maxAmmo);
    }

    public bool TryConsume(int amount)
    {
        if (currentAmmo < amount) return false;
        currentAmmo -= amount;
        return true;
    }

    public void Refill()
    {
        currentAmmo = maxAmmo;
    }
}