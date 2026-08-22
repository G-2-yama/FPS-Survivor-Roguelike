using UnityEngine;
using System.Collections.Generic;
using System;

public class WeaponControllerManager : MonoBehaviour
{
    [SerializeField] private Player player;

    private readonly List<WeaponController> controllers = new();

    public void Register(WeaponController controller)
    {
        if (controller == null)
            return;

        if (!controllers.Contains(controller))
        {
            controllers.Add(controller);
        }
    }

    public void Unregister(WeaponController controller)
    {
        controllers.Remove(controller);
    }

    public void OnWeaponFired(WeaponController source)
    {
        Sync(source, controller => controller.FireSync());
    }

    public void OnWeaponReleased(WeaponController source)
    {
        Sync(source, controller => controller.ReleaseSync());
    }

    public void OnWeaponReloaded(WeaponController source)
    {
        Sync(source, controller => controller.ReloadSync());
    }

    /// <summary>
    /// 同期処理を行う
    /// </summary>
    /// <param name="source">発火元のWeaponController</param>
    /// <param name="action">実行するアクション</param>
    private void Sync(WeaponController source, Action<WeaponController> action)
    {
        // プレイヤーが武器同期を許可していない場合は処理を中断
        if (!player.IsWeaponSync)
            return;

        foreach (var controller in controllers)
        {
            if (controller == null || controller == source)
                continue;

            action(controller);
        }
    }

    public Vector2 GetTotalRecoil(float deltaTime)
    {
        var total = Vector2.zero;

        foreach (var controller in controllers)
        {
            if (controller == null)
                continue;

            var recoil = controller.Weapon?.WeaponData?.Recoil;

            if (recoil == null)
                continue;

            recoil.Tick(deltaTime);
            total += recoil.RecoilOffset;
        }

        return total;
    }
}