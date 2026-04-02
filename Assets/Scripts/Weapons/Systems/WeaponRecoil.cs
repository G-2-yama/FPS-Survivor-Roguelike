using UnityEngine;

public class WeaponRecoil
{
    private Vector2 currentRecoil;
    private Vector2 targetRecoil;

    private WeaponData weaponData;

    public WeaponRecoil(WeaponData data)
    {
        weaponData = data;
    }

    /// <summary>
    /// 発射時に反動を加える
    /// </summary>
    public void AddRecoil()
    {
        float recoilX = weaponData.RecoilX;
        float recoilY = weaponData.RecoilY;

        targetRecoil += new Vector2(
            Random.Range(-recoilY, recoilY), // 横ブレ
            recoilX // 縦ブレ
        );
    }

    /// <summary>
    /// 毎フレーム更新（回復処理）
    /// </summary>
    public Vector2 Update(float deltaTime)
    {
        // 現在値をターゲットに近づける
        currentRecoil = Vector2.Lerp(currentRecoil, targetRecoil, deltaTime * 10f);

        // ターゲットを0に戻す（回復）
        targetRecoil = Vector2.Lerp(targetRecoil, Vector2.zero, deltaTime * weaponData.RecoilRecoverySpeed);

        return currentRecoil;
    }
}
