using UnityEngine;
using UnityEngine.PlayerLoop;

public class WeaponRecoil : MonoBehaviour
{
    [SerializeField] private Transform cameraPivotPoint;
    private Vector2 recoilOffset;
    public Vector2 RecoilOffset => recoilOffset;
    private Vector2 recoilVelocity;

    private RecoilProfile recoilProfile;

    public void Initialization(RecoilProfile recoilProfile)
    {
        this.recoilProfile = recoilProfile;
    }

    /// <summary>
    /// 発射時に反動を加える
    /// </summary>
    public void AddRecoil(float recoilMultiplier = 1f)
    {
        if (recoilProfile == null)
        {
            return;
        }

        float yaw = Random.Range(-recoilProfile.YawKick, recoilProfile.YawKick) * recoilProfile.YawRandomness;

        recoilOffset.y += recoilProfile.PitchKick * recoilMultiplier;
        recoilOffset.x += yaw * recoilMultiplier;
    }

    /// <summary>
    /// 毎フレーム更新（回復処理）
    /// </summary>
    public Vector2 Tick(float deltaTime)
    {
        if (recoilProfile == null)
        {
            return Vector2.zero;
        }

        float dt = deltaTime;

        Vector2 accel = (-recoilProfile.ReturnStrength * recoilOffset) - (recoilProfile.Damping * recoilVelocity);
        
        recoilVelocity += accel * dt;
        recoilOffset += recoilVelocity * dt;

        recoilOffset.y = Mathf.Clamp(recoilOffset.y, -recoilProfile.MaxPitch, recoilProfile.MaxPitch);

        return recoilOffset;
    }
}
