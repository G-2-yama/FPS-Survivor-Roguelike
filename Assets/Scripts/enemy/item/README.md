# ドロップアイテムの取得効果

設定SOは `Assets/Datas/PickupEffects` にあります。

| SO | 効果 | 持続時間 |
| --- | --- | --- |
| Heal | HP回復150 | 即時 |
| ExperienceSmall / Medium / Large | 経験値10 / 50 / 500 | 即時 |
| SpeedUp | 歩行・走行速度、ジャンプ力を各3.5加算 | 2.5秒、再取得で時間加算 |

ProjectのCreate > Items > Pickup Effect Definitionから追加できます。
各アイテムのEffect DefinitionにSOを割り当ててください。効果量はSOだけで編集します。
SOのIconを差し替えると、表示されるバフ画像も変わります。
回復・経験値を時間付きにするとInspectorにエラーが表示され、取得時にも適用されません。

## 担当

- PickupEffectDefinition: 効果の種類・効果量・方式・持続時間・再取得方式・アイコン。
- PickupEffectApplier: 即時効果の実行と時間付き効果の登録。
- PickupTimedBuff / TimedBuffManager: プレイヤー別の残り時間・適用・解除。SOには残り時間を保存しません。
- BuffIconPresenter / BuffIconView: アイコンの一覧、配置、点滅。能力値は変更しません。

PlayerHealthTextViewがStart時にBuffIconPresenterを接続し、HPバー上へ一覧を生成します。
点滅は標準で残り1秒から、0.5秒周期です。再取得で残り時間が閾値を超えると通常表示に戻ります。
設定を変えるには、PlayerHealthTextViewと同じGameObjectにBuffIconPresenterを事前に追加し、
Blink Before Seconds / Blink Period / Icon Size / Spacing / Offset Above Barを調整してください。
即時効果は継続アイコンを生成しません。
重複方式はExtend（時間加算）、Refresh（時間リセット）、Stack（別効果として重複）の3種類です。
Stackでは効果ごとに別のアイコンを表示します。

## 移行について

既存の5つのドロップPrefabはSO参照へ移行済みです。
速度・回復・経験値の既存コンポーネントと取得音は維持しています。
バランス解析のPrefabReaderもSO由来の値を読みます。
SpeedUpBuffは旧コードとの互換用で、現在の速度アイテムはPickupTimedBuffを使用します。