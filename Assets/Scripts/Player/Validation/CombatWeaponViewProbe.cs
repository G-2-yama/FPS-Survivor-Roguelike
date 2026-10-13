#if UNITY_EDITOR
/// <summary>攻撃方式の検証から表示部品への依存を取り除く専用ビュー</summary>
public sealed class CombatWeaponViewProbe : WeaponView
{
    /// <summary>検証では武器モデルの生成を行わない</summary>
    protected override void Start() { }

    /// <summary>検証では表示部品を更新しない</summary>
    public override void RefreshView(Weapon weapon) { }

    /// <summary>検証ではリロード表示を更新しない</summary>
    public override void SetReloadProgress(float progress) { }
}
#endif
