# 敵のサイクルPhase

`Assets > Create > EnemyGenerator > Enemy Cycle Phase` で敵専用の `EnemyCyclePhase` を作成する。
既存の `EnemyGenerator` の `phases` 欄に、そのまま設定できる。旧 `PhaseData` との混在も可能。
既存のScene・Prefab・Phaseアセットは自動変換しない。アイテム生成は引き続き旧Phaseを使う。

## 設定

- 共通の `Enemies` / `Min Spawn Count` / `Spawn Interval` が通常区間の雑魚設定。
- `Normal Duration` 秒が経過すると `Boss Spawn Count` 体の中ボスを生成する。Phaseごとに0以上を指定でき、既定値は1。0なら中ボスなしでも区間の時間は変わらない。
- `Boss Prefab` のルートには `Enemy` が必要。`Boss Candidates` を設定した場合は、単体Prefabの代わりに各個体を候補の重みから選ぶ。再生成時もその個体に選ばれた候補を使用する。
- 生成数はそのPhaseの合計数で、撃破した個体は補充しない。生成位置やGeneratorの上限で出せなかった分は1秒ごとに再試行する。
- 中ボスHPは基礎HPに、今回の通常区間の実測DPSから求めた倍率を掛ける。旧 `isExPhase` と同じ `clamp(sqrt(DPS / ReferenceDps), MinHpMultiplier, MaxHpMultiplier)` を使用し、既定では1〜1.3倍。
- DPSは通常区間の実ダメージ合計÷通常区間の秒数。中ボス戦・大群のダメージは含めない。倍率は通常区間終了時に固定し、再生成でも変えない。雑魚のHPは補正しない。
- 中ボス戦中も同じ雑魚を生成する。`Boss Spawn Rate Multiplier` と `Boss Minimum Multiplier` で出現頻度・最低維持数を別々に調整する。両方1なら通常どおり、既定の0.4なら緩やかになる。出現頻度0なら補充も停止する。
- 通常終了から `Boss Interval` 秒（既定10秒）を中ボス区間とし、その後に大群へ進む。早期撃破でも未撃破でも切替時刻は変えない。
- 大群は `Rush Duration` 秒。出現頻度と最低維持数はそれぞれ `Rush` の倍率を使う。
- `Rush Enemies` は空なら共通構成を継続し、設定すると大群だけ専用の構成に切り替わる。
- 大群終了後は次のPhaseへ進む。最後の新Phaseが完了した場合は新規出現を停止し、生存中の雑魚は残す。

1Phaseの長さは `Normal Duration + Boss Interval + Rush Duration`。`Start Time` からの絶対時刻で各区間を進め、フレームの遅延を次Phaseへ積み上げない。
次Phaseの `Start Time` を前Phaseの終了時刻に合わせる。既定は通常30秒＋インターバル10秒＋大群25秒＝65秒なので、0・65・130秒…と設定する。
旧PhaseのStart Timeは絶対時刻のまま。新Phaseの後で期限切れの旧Phaseが続く場合、従来と同じく経過済みの旧Phaseを飛ばす。

## 実行時の扱い

新Phaseの設定と状態はGeneratorごとに複製する。同じSOを複数のGeneratorで共有しても、中ボスや進行状態は共有しない。
Play中に元の新Phaseを編集した場合、実行中のサイクルには反映せず次の入場時に読み直す。
雑魚の入れ替えで中ボスを消さず、中ボス自身は雑魚の最低維持数に数えない。未撃破の中ボスは次Phaseにも残し、同じ個体のHP倍率も維持する。
生成に失敗した中ボスは1秒ごとに再試行する。破棄・プール返却された中ボスも撃破扱いにせず再生成する。
`DisableAutoSpawn` は新Phaseの雑魚と中ボスの両方を止める。

Generator内で必要になった接続変更には理由のコメントを付けている。旧Phaseだけなら従来の時刻切替と生成処理を使う。
Phaseの開始時刻を終了時刻に揃えることで、`PhaseWallColor` のようなStart Timeを見る演出とも同じ時刻で切り替わる。

現時点のBalanceSimは中ボスと大群を含む新Phaseに未対応。誤って通常雑魚だけを計算しないよう、新Phaseを検出した場合は明示的に停止する。

## GameSceneへの移行

`Phase_00`〜`Phase_09` を使用し、開始時刻を0〜585秒の65秒刻みに設定した。保持したPhaseのGUID・通常の雑魚構成・中ボス候補は維持する。
大群の `Rush Enemies` はBit・Ball中心とし、進行に応じてMissileBox・Gatlingを少量混ぜる。硬いHandやEX個体は大群に入れない。
600秒以降に始まる `Phase_10`〜`Phase_26` はScene・演出の参照を整理して削除した。最後のPhaseへ演出の終端の赤さを引き継ぐ。
旧EXPhase_01〜26はGeneratorの参照から外して削除済み。アイテム用・TestPhase・作業用の空Phaseは旧形式のまま残す。
ゲームの600秒制限は維持する。最後のPhaseは585秒に始まり、600秒で終了するため途中までの進行となる。
