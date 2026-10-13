using UnityEngine;
/// <summary>接触対象へダメージを適用して攻撃個体を返却する</summary>

public class DamageObject : DamageBase
{
    /// <summary>侵入したColliderとの接触処理を開始する</summary>
    private void OnTriggerEnter(Collider other)
    {
        if (isReleased)
            return;

        if (TryDamage(other, out _))
        {
            Release();
        }
        else if (!other.isTrigger && !DamageTargetRegistry.TryResolve(other, out _))
        {
            Release();
        }
    }
    /// <summary>物理衝突したColliderへの命中を処理する</summary>

    private void OnCollisionEnter(Collision collision)
    {
        if (isReleased)
            return;

        if (TryDamage(collision.collider, out _))
        {
            Release();
        }
        else
        {
            // 壁などでも消えるなら
            Release();
        }
    }
}