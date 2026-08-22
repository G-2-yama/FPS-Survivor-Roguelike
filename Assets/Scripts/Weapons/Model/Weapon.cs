using UnityEngine;

public class Weapon : MonoBehaviour
{
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

    private int currentAmmo = 0;
    public int CurrentAmmo => currentAmmo;
    
    private WeaponStateMachine stateMachine;
    public WeaponStateMachine StateMachine => stateMachine;

    public bool HasWeapon => !weaponData.IsEmpty;

    private void Awake()
    {
        stateMachine = new WeaponStateMachine(this);
        if (!HasWeapon)
        {
            weaponData = EmptyWeaponData.Instance;
            currentAmmo = 0;
            weaponView.RefreshView(this);
            return;
        }

        currentAmmo = weaponData.MagazineSize;
        sounder.SetSoundDB(weaponData.SoundDB);
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

            // ammoが-1ならフルリロード、それ以外なら指定された弾数をセット
            currentAmmo = (ammo < 0) ? weaponData.MagazineSize : Mathf.Clamp(ammo, 0, weaponData.MagazineSize);
            sounder.SetSoundDB(newData.SoundDB);    
        }


        stateMachine.ChangeState<WeaponIdleState>();
        weaponView.RefreshView(this);
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
        if (!HasWeapon || currentAmmo <= 0)
        {
            return false;
        }

        currentAmmo--;
        weaponView.RefreshView(this);
        weaponData.FireModeData.Fire(this, weaponOwner);

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
        weaponView.RefreshView(this);
    }
}
