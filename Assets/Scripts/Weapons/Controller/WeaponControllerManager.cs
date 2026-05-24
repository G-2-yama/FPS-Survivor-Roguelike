// WeaponControllerManager.cs
using UnityEngine;
using System.Collections.Generic;

public class WeaponControllerManager : MonoBehaviour
{
    [SerializeField] private Player player;
    private readonly List<WeaponController> controllers = new();

    public void Register(WeaponController controller)
    {
        if (!controllers.Contains(controller))
            controllers.Add(controller);
    }

    public void Unregister(WeaponController controller)
    {
        controllers.Remove(controller);
    }

    /// <summary>
    /// あるコントローラーの発火イベントを受け取ったとき、他のコントローラーに同期イベントを送る
    /// </summary>
    /// <param name="source"></param>
    public void OnWeaponFired(WeaponController source)
    {
        if (!player.IsWeaponSync) return;
        foreach (var c in controllers)
        {
            if (c == source) continue;
            c.FireSync();
        }
    }

    public void OnWeaponReleased(WeaponController source)
    {
        if (!player.IsWeaponSync) return;
        foreach (var c in controllers)
        {
            if (c == source) continue;
            c.ReleaseSync();
        }
    }

    public void OnWeaponReloaded(WeaponController source)
    {
        if (!player.IsWeaponSync) return;
        foreach (var c in controllers)
        {
            if (c == source) continue;
            c.ReloadSync();
        }
    }


    public Vector2 GetTotalRecoil(float deltaTime)
    {
        var total = Vector2.zero;
        foreach (var c in controllers)
        {
            var recoil = c?.Weapon?.WeaponStats?.Recoil;
            if (recoil == null) continue;
            recoil.Tick(deltaTime);
            total += recoil.RecoilOffset;
        }
        return total;
    }
}