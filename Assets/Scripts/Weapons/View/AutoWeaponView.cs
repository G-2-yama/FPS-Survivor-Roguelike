using UnityEngine;
using UnityEngine.UI;

public class AutoWeaponView : WeaponView
{
    [SerializeField] private Image reloadingBackground;

    public override void SetReloadProgress(float progress)
    {
        base.SetReloadProgress(progress);
        
        // リロード中のみ背景を表示する
        bool isReloading = progress > 0f && progress < 1f;
        reloadingBackground.gameObject.SetActive(isReloading);
    }
}