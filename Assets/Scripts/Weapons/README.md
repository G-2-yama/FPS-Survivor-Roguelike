# Weapons

武器機能は「攻撃ロジックを共通化し、差分はデータで吸収する」前提で構成します。

## 推奨フォルダ構成

- `Model/`
  - 武器パラメータ定義（ScriptableObject）
  - 例: `WeaponData.cs`
- `Controller/`
  - 武器切り替え、入力受け取り、状態遷移の起点
- `Systems/`
  - 発射・リロード・命中判定などの共通処理
- `States/`
  - Idle/Firing/Reloading などの状態クラス

## 運用ルール

- 変更頻度の高い値は `Model` に集約する
- `Systems` は `WeaponData` を受け取り、武器ごとに分岐しない実装を基本とする
- 武器固有挙動が必要な場合も、まずはデータ（フラグ、係数、モード）で表現できるかを優先する
