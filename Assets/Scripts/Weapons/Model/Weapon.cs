using UnityEngine;
/// <summary>所有者の武器設定と弾数と攻撃状態を管理する</summary>

public class Weapon : MonoBehaviour
{
    public static event System.Action OnAnyFired;

    [SerializeField] private Player weaponOwner;
    public Player WeaponOwner => weaponOwner;
    [SerializeField] private WeaponData weaponData;
    public WeaponData WeaponData => weaponData;

    [SerializeField] private Transform muzzle;
    public Transform Muzzle => muzzle;

    [SerializeField] private Sounder sounder;
    public Sounder Sounder => sounder;
    [SerializeField] private WeaponView weaponView;
    public WeaponView WeaponView => weaponView;

    [SerializeField, Tooltip("照準方向と弾の生成位置に使うプレイヤーカメラのTransform")]
    private Transform aimTransform;
    public Transform AimTransform => aimTransform;

    private int currentAmmo = 0;
    public int CurrentAmmo => currentAmmo;
    
    private WeaponStateMachine stateMachine;
    public WeaponStateMachine StateMachine => stateMachine;

    public bool HasWeapon => !weaponData.IsEmpty;
    /// <summary>初回生成時に必要な参照と状態を初期化する</summary>

    private void Awake()
    {
        // 未設定のシーンでもカメラの補完はゲーム開始時だけに限定する
        if (aimTransform == null)
        {
            Camera camera = Camera.main;
            if (camera != null) aimTransform = camera.transform;
        }
        if (aimTransform == null) Debug.LogError("照準用Transformが設定されていません", this);
        stateMachine = new WeaponStateMachine(this);
        Equip(weaponData);
    }

    /// <summary>
    /// 新しい武器を装備するメソッド
    /// </summary>
    /// <param name="newData">新しい武器データ</param>
    /// <param name="ammo">新しい残弾数</param>
    public void Equip(WeaponData newData, int ammo = -1)
    {
        if (newData == null || newData.IsEmpty)
        {
            weaponData = EmptyWeaponData.Instance;
            currentAmmo = 0;
        }
        else
        {
            weaponData = newData;
            // 反動の状態は複製していない武器データのアセット上にあり、前回の再生の値（NaN を含む）が残っているため0から始める
            weaponData.Recoil.Reset();

            // ammoが-1ならフルリロード、それ以外なら指定された弾数をセット
            currentAmmo = (ammo < 0) ? weaponData.MagazineSize : Mathf.Clamp(ammo, 0, weaponData.MagazineSize);
            sounder.SetSoundDB(newData.SoundDB);    
        }

        weaponView.RefreshView(this);
        stateMachine.ChangeState<WeaponIdleState>();
    }

    /// <summary>
    /// 武器レベルを上げるメソッド
    /// </summary>
    public void LevelUp()
    {
        if (!HasWeapon)
        {
            return;
        }

        weaponData = weaponData.NextLevelData;
        sounder.SetSoundDB(weaponData.SoundDB);
        weaponView.RefreshView(this);
    }

    /// <summary>
    /// 攻撃処理を実装するメソッド
    /// </summary>
    /// <returns>true: 攻撃できた / false: 攻撃できなかった</returns>
    public bool Fire()
    {
        if (!HasWeapon || currentAmmo <= 0 || aimTransform == null)
        {
            return false;
        }

        currentAmmo--;
        weaponView.UpdateAmmo(currentAmmo, weaponData.MagazineSize);
        weaponData.FireModeData.Fire(this, weaponOwner);
        OnAnyFired?.Invoke();

        return true;
    }

    /// <summary>
    /// リロード処理を実装するメソッド
    /// </summary>
    public void Reload()
    {
        if (!HasWeapon)
        {
            return;
        }

        currentAmmo = weaponData.MagazineSize;
        weaponView.UpdateAmmo(currentAmmo, weaponData.MagazineSize);
    }
}
