using Unity.VisualScripting;
using UnityEngine;

public abstract class FireModeData : ScriptableObject
{
    /// <summary>
    /// 攻撃処理を実装するメソッド
    /// </summary>
    /// <param name="weapon">攻撃を行う武器</param>
    /// <param name="direction">攻撃方向</param>
    public abstract void Fire(Weapon weapon, Vector3 direction);
}
