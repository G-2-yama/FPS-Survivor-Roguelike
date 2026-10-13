# プレイヤー設定と衝突機構

## 設定を変更する

プレイヤーの `PlayerConfig` を選び、用途に対応するSOを展開して編集する
項目にカーソルを合わせると、用途、単位、他の設定への影響を表示する
項目名は英語、Tooltipは日本語で表示する PlayerConfigの分類名は日本語のまま使用する

| 調整内容 | 設定SO |
|---|---|
| 初期HP、移動速度、ジャンプ初速、ダッシュ能力、攻撃倍率 | PlayerStatsSettings |
| 減速、重力、ジャンプ回数、視点操作、スライド、急降下 | PlayerControlSettings |
| 本体半径、高さ、中心、段差、スライド高さ、接触、押し出し | PlayerCollisionSettings |
| スライド視点高さ、走行とスライドのFOV | PlayerCameraSettings |
| 初期レベル、必要経験値、必要経験値の増加倍率 | PlayerProgressionSettings |

SOは複数のプレイヤーで共有できる
特定のプレイヤーだけ変更する場合はSOを複製して割り当てる
Inspectorの「新規作成」は既定値から独立したSOを作り、「選択」は割り当て済みSOを選ぶ

プレイ開始時に5つのSOを複製して使用するため、プレイ中のSO編集は次の開始時に反映される
SOアセットの編集は保存されるため、実行中のHPや能力は `Player` の読み取り専用表示で確認する

## 新しいプレイヤーへ組み込む

1. プレイヤー本体に `Player` と `PlayerController` を付ける
2. 自動追加される `PlayerConfig` に5分類のSOを割り当てる
3. `PlayerController` の武器管理、カメラ支点、カメラなどの参照を設定する
4. 本体にRigidbodyがある場合は `PlayerController` の参照欄へ割り当てる
5. Input Systemのイベントを既存の `OnMove`、`OnLook`、`OnJump`、`OnSprint`、`OnCrouch` へ関連付ける

開始時に `PlayerCollisionController` が本体寸法を反映し、子の `EnemyPushZone` を作る
本体に既存の被弾用カプセルがある場合も寸法と姿勢を同期し、Triggerとして使用する
編集時にも衝突のGizmoを確認する場合は `PlayerCollisionController` を本体へ付ける
水色は本体、黄色は接触領域を示し、追加距離が0の場合は同じ境界になる

GameScene用の設定は `Assets/Datas/Player/GameScene*Settings.asset` に保存してある
初期HP1000、ダッシュ距離5.5、必要経験値50、増加倍率1.08などのシーン値を引き継いでいる
実際に参照されていなかった地上加速、空中減速、地面レイ設定は移行対象から除いている
復旧用の `Assets/_Recovery` は移行していないため、復旧シーンを使用する際は現行SOを割り当てる

## 強化値と成長状態

`PlayerStats` はプレイヤーごとに生成され、速度や攻撃倍率の強化はこのオブジェクトに適用する
最大HPは `Health` を唯一の値として参照し、変更は `PlayerStats.AddMaxHP` を使用する
ダッシュの距離と時間は開始時の強化値を使用し、実行途中で強化されても進行中のダッシュは変化しない
長押しジャンプの補正もジャンプ開始時の強化後の初速から計算する

`PlayerProgression` はレベル、残余経験値、次の必要経験値をプレイヤーごとに保持する
一度に複数レベル分を取得した場合は全レベルを確定し、選択待ち回数を加算してからUIへ通知する
経験値UIとバランスシミュレーターは新しい設定と状態を使用する

## 敵との接触と押し出し

衝突SOの敵レイヤーは、本体からの衝突除外と接触領域の検知に使用する
本体、接触領域、敵のレイヤーは重複させない
既存の敵はプール取得時に `Enemy` レイヤーを割り当てる
別のレイヤーを使用する独自の敵は、そのレイヤーを敵の対象レイヤーへ追加する

既存の `Enemy` はプール取得時に `PlayerContactTarget` を準備する
独自の敵では `PlayerContactTarget` を付け、移動するRigidbodyと接触ダメージ元を割り当てる
ダメージ元は `IContactDamageSource` を実装する
通常Colliderの外接境界から半径を推定するため、細長い形状などでは「水平半径の上書き」を設定する
半径を調整する場合は敵Prefabへあらかじめ `PlayerContactTarget` を付ける

押し出しと接触ダメージは独立して有効化できる
検知はカプセルで行い、押し出し計算は水平面の円形境界で行う
押し出しでは対象の高さを変えない
接触ダメージは対象の最初のColliderが入ったときに一度だけ適用し、全部退出した後の再侵入で再び適用する
プール返却や無効化時は対象から接触終了を通知し、古い状態を持ち越さない

## 攻撃方式を追加する

接触領域には `AttackIgnoredCollider` が付いている
補助Colliderを追加する場合は、そのColliderと同じGameObjectにマーカーを付ける
攻撃の命中処理前に `AttackCollisionFilter.ShouldIgnore(collider)` を確認する
ヒット一覧の検索では次の候補へ進み、実体弾では命中処理と解放を行わず通過させる
壁や敵の通常Colliderにはこのマーカーを付けない

## 検証

5つの設定SOのInspectorとPlayerConfigに設定不足や数値の警告を表示する
Unityのコンパイルに加え、移行値、共有SOからの状態分離、能力強化、複数レベルアップ、接触判定を検証する
