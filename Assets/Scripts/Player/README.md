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
3. `Player` の `Config`、`Inventory`、`Timed Buff Manager` を割り当て、必要な効果音の参照も設定する
4. `PlayerController` のプレイヤー、武器管理、カメラ支点、カメラ、衝突制御などの参照を設定する
5. 本体に被弾用の `CapsuleCollider` を配置し、`PlayerCollisionController` の `Config`、`Controller`、`Body Trigger` を割り当てる
6. 本体の子に `EnemyPushZone` を作り、`PlayerEnemyPushZone` と `CapsuleCollider` を付ける 子の位置と回転は0、スケールは1にする
7. 子の部品を `PlayerCollisionController` の `Push Zone` と `Push Zone Collider` に割り当てる
8. 本体にRigidbodyがある場合は `PlayerController` の参照欄へ割り当てる
9. 戦闘Sceneへ更新担当を事前配置する 詳細は「固定コンポーネントの事前配置」を参照する
10. Input Systemのイベントを既存の `OnMove`、`OnLook`、`OnJump`、`OnSprint`、`OnCrouch` へ関連付ける

開始時に `PlayerCollisionController` が、事前配置した本体と接触領域へSOの寸法と衝突設定を反映する
被弾用カプセルは必須参照で、本体の寸法と姿勢に同期してTriggerとして使用する
本体の `PlayerCollisionController` を選択すると、衝突のGizmoを確認できる
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

既存の `Enemy` はPrefabに事前配置した `PlayerContactTarget` の参照を使い、プール取得時の `OnGet()` で `BeginLife()` を呼んで登録する
独自の敵では `PlayerContactTarget` を事前配置し、活動開始時に `BeginLife(body, damageSource)` を呼ぶ
`body` は敵を移動するRigidbody、`damageSource` は `IContactDamageSource` を実装したコンポーネントを渡す
検知するColliderの `attachedRigidbody` が登録したRigidbodyと一致する構成にする
Inspectorで参照を割り当てただけでは登録されず、押し出しと接触ダメージの対象にならない
プール返却時や活動終了時には `Invalidate()` を呼ぶ 無効化でも登録は解除されるが、再有効化だけでは再登録されないため、活動再開時に `BeginLife()` を呼び直す
半径は接触通知を受けたColliderのワールド座標の境界から推定するため、細長い形状などでは `Radius Override` を設定する
`Radius Override` が0の場合、推定半径は同じ接触期間中の最大値を保持し、Colliderの縮小や退出だけでは小さくならない

押し出しと接触ダメージは独立して有効化できる
検知はカプセルで行い、押し出し計算は水平面の円形境界で行う
押し出しでは対象の高さを変えない
接触ダメージは対象の最初のColliderが入ったときに一度だけ適用し、全部退出した後の再侵入で再び適用する
プール返却や無効化時は対象から接触終了を通知し、古い状態を持ち越さない

敵は既定ですべて接触領域から押し出される
`PlayerObstacle` コンポーネントを例外オブジェクトに付けると、自身と子のColliderへ専用レイヤーを設定し、プレイヤー接触領域からの押し出し対象にも含めない
`PlayerBody` はこのレイヤーと衝突し、`Enemy` レイヤーは衝突しない
新しいレイヤーを使う場合は `TagManager` の名前と `DynamicsManager` の衝突設定を維持する

## 攻撃方式を追加する

接触領域のColliderは `AttackCollisionFilter` に登録され、攻撃判定から除外される
攻撃から除外する補助Colliderを追加する場合は、参照を保持するコンポーネントから `AttackCollisionFilter.Register(collider)` を呼ぶ
参照の設定後と再有効化時に登録し、無効化または破棄時に `AttackCollisionFilter.Unregister(collider)` を呼ぶ
既存の `PlayerEnemyPushZone` は初期化時と有効化時に登録し、無効化時に解除するため、追加の登録処理は不要
攻撃の命中処理前に `AttackCollisionFilter.ShouldIgnore(collider)` を確認する
ヒット一覧の検索では次の候補へ進み、実体弾では命中処理と解放を行わず通過させる
壁や敵の通常Colliderは攻撃除外対象として登録しない

## 検証

5つの設定SOのInspectorとPlayerConfigに設定不足や数値の警告を表示する
Unityのコンパイルに加え、移行値、共有SOからの状態分離、能力強化、複数レベルアップ、接触判定を検証する

## 実行中の参照管理

ゲーム開始時と各個体の初回生成時だけUnityの参照検索を行う
発射、命中、接触、プール再利用時は保持済みの参照を使う

- `DamageTargetBinding` は同じGameObjectのColliderと `IDamageable` を登録する
- `DamageTargetRegistry` はColliderから登録済みの対象を解決する
- `PoolManager.Rent` は初回生成時に準備した `PooledInstance` を返す
- `PooledInstance` は実際に使う部品だけを用途別のプロパティに保持する
- 攻撃生成は `RequireDamage()`、任意の移動部品は `Movement` などから取得する
- `Weapon.AimTransform` は明示的に設定した照準の基準を使う

ダメージ対象の既存基底型には登録部品の `RequireComponent` があるため、新しいPrefabにも登録部品が追加される
独自の `IDamageable` を追加する場合も `DamageTargetBinding` を同じGameObjectへ付ける
既存資産の移行は `Tools/Combat/参照を移行して検証` から実行できる
親のダメージ対象へ子のColliderを自動的に対応付ける処理は行わない

部品参照は初回生成後に変更せず、発射や再取得ではプロパティから直接読み取る
プール自体が `PooledInstance` を保持するため、取得と返却のコールバックでGameObjectから個体の登録表を引き直さない
返却処理は初回生成時にだけ関連付け、`PoolableObject.Release` から直接呼び出す
既存の用途の派生クラスは同じプロパティへ登録されるため、新しい敵や移動方式を追加する際に型別キャッシュへ登録する作業は不要
新しい用途の部品をプールから参照する場合は、`PooledInstance` の用途別プロパティと初回の型判定へ追加する

## 固定コンポーネントの事前配置

`EnemyPushZone` と専用の `CapsuleCollider` はPlayerの子オブジェクトに配置し、`PlayerCollisionController` の `Push Zone` と `Push Zone Collider` に割り当てる
`Enemy` の `Contact Target` には同じGameObjectの `PlayerContactTarget` を割り当てる
`PlayerActionTrail` の `State Bindings` には各アクション専用の `Trail Renderer` を割り当てる
これらの不足を実行時に生成して補完する処理は行わない

戦闘Sceneの `CombatRunners` に `LifetimeRunner`、`EnemyBrainRunner`、`BoostFireCullerRunner`、`DamagePopupRunner` を1つずつ配置する
更新担当はScene内で先に初期化され、Scene終了時に登録と個体の登録番号を解放する
Sceneを再読み込みした場合は新しいSceneに配置した更新担当を使う

既存資産の構成を移行する場合は `Tools/Combat/固定コンポーネントを事前配置` を使用する
`Tools/Combat/固定コンポーネントの設定を確認` で現在のSceneの未設定参照、軌跡の共有、更新担当の不足や重複を検出する
Prefabとビルド対象Sceneはビルド時にも検査する
設定済みPrefabの生成とプール再利用は引き続き実行時に行う

## 接触状態の更新と再利用

プレイヤーの接触領域と範囲攻撃は、それぞれ対象別の接触状態とCollider別の対応表を持つ
状態の生成、更新、解除は各コンポーネント内で完結し、終了した状態とCollider集合を再利用する
状態の保持数は各領域で同時に追跡した最大数に応じて増えるため、再侵入時の割り当てを減らす代わりに再利用用のメモリを保持する
領域が破棄されると再利用用の状態も解放対象になる

接触ダメージを適用した途中で対象が返却される場合があるため、活動期間の番号で終了済み状態への書き戻しを防ぐ
範囲攻撃は既存の仕様に合わせ、Colliderごとに時間を加算し、1つのColliderが退出した時点で対象の時間をリセットする
押し出しは既存の継続通知とFixedUpdateのタイミングを維持する
`PlayerContactTarget.TryResolve` はRigidbodyをキーとしたDictionaryの参照取得であり、Sceneや階層を探索しない

## 負荷比較

`CombatPerformanceComparison.RunBatch` は隔離したUnityの空Sceneで初回生成、プール再利用、24体と200体の接触、200対象の範囲攻撃を計測する
300回のウォームアップ後に600回を5走行測定し、CPU時間の中央値と95パーセンタイル、GC.Alloc、管理メモリの差分をCSVへ出力する
CPUとGCは別の走行で記録し、GC.Allocは既知の割り当てで校正する
同じ物理通知を直接呼ぶ処理単位の比較であり、描画、物理エンジン全体、ゲーム全体のフレーム時間は対象に含めない
