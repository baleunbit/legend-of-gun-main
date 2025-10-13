// SharedAmmo.cs
using UnityEngine;

public class SharedAmmo : MonoBehaviour
{
    [Header("공용 탄약 설정")]
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