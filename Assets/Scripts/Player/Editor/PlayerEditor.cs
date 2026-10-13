using UnityEditor;

/// <summary>プレイヤーの初期参照と実行中の状態を区別して表示する</summary>
[CustomEditor(typeof(Player))]
public sealed class PlayerEditor : Editor
{
    /// <summary>共有設定と強化後の値を混同しないよう実行時情報を読み取り専用で描画する</summary>
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        var player = (Player)target;
        if (player.Stats == null) return;
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Runtime Stats", EditorStyles.boldLabel);
        using (new EditorGUI.DisabledScope(true))
        {
            EditorGUILayout.TextField("Current HP / Max HP", $"{player.Health.CurrentHP} / {player.Health.MaxHP}");
            EditorGUILayout.FloatField("Walk Speed", player.Stats.WalkSpeed);
            EditorGUILayout.FloatField("Run Speed", player.Stats.RunSpeed);
            EditorGUILayout.FloatField("Jump Force", player.Stats.JumpForce);
            EditorGUILayout.FloatField("Dash Distance", player.Stats.DashDistance);
            EditorGUILayout.FloatField("Dash Duration", player.Stats.DashDuration);
            EditorGUILayout.FloatField("Dash Cooldown", player.Stats.DashCooldown);
            EditorGUILayout.FloatField("Damage Multiplier", player.Stats.DamageMultiplier);
            EditorGUILayout.FloatField("Knockback Force Multiplier", player.Stats.KnockbackForceMultiplier);
            EditorGUILayout.IntField("Level", player.Level);
            EditorGUILayout.FloatField("Experience", player.Exp);
            EditorGUILayout.FloatField("Required Experience", player.LevelUpRequiredExp);
            EditorGUILayout.IntField("Pending Level Ups", player.PendingLevelUps);
        }
        if (EditorApplication.isPlaying) Repaint();
    }
}
