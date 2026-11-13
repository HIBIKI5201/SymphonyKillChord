# チュートリアルの敵撃破ステップで進めなくなる不具合の調査

- 調査日: 2026-10-27
- 発生環境: エディタのPlayMode（2026-10-27 4:00 開始のセッション）
- 症状: 最後の「敵を撃破しよう」ステップで、出てくる敵が1体足りず、条件を満たせないまま止まる

## 結論

キルコードの手順（スキルを2回使うステップ）では、チュートリアル用に敵を無敵にしている。ところが、プレイヤーの攻撃の確定ダメージ（基礎ダメージ 50）がこの無敵を通り抜け、敵が1体倒されていた。

この撃破は、次の撃破ステップが始まる前に数えられる。撃破ステップは開始時の撃破数を基準にして「赤い敵を5体倒す」と判定するため、Waveに残った4体を全部倒しても条件に届かない。

## ログの根拠（`%LOCALAPPDATA%\Unity\Editor\Editor.log`）

- キルコードの手順に入ったあと、出現した敵3体に `InvincibleBuff` が付いている（`EnemyLifeCycle.Activate` → `EnemyInvincibilityRegistry.Register`）。
- その間も、1体のHPが攻撃1回ごとに50ずつ減っている。`[Attack] ... Damage:0`（補正後のダメージは0）なのに、`[EnemyHealthHudPresenter] 敵HP更新：0 / 500` まで下がっている。
- 撃破ステップに入ってから倒れた敵は4体。その直後に `[EnemyWaveSpawnerState] Wave Cleared.` が出て、`[EnemyWaveTimerView] Wave Timeout.` で止まっている。

## コード上の原因

- `InvincibleBuff`（`Assets/Scripts/Runtime/2.Application/InGame/Buff/InvincibleBuff.cs`）は、被ダメージの補正で `FinalDamage` を0にするだけ。
- `DamageExecutor.ExecuteInternal`（`Assets/Scripts/Runtime/2.Application/InGame/Battle/DamageExecutor.cs`）は、被ダメージの補正とバリアの吸収が済んだあとに確定ダメージを足す。足すかどうかは `defender.CanTakeDamage` で判断している。
- `CharacterEntity.CanTakeDamage` が見ているのは Entity 自身の無敵フラグ（`_isInvincible`）だけ。状態効果の無敵は反映されない。
- 確定ダメージを加算する変更（12f4be1f4「確定ダメージを全ての計算のあとに加算する」）が入ってから、この貫通が起きるようになった。無敵バフ（35181d40f、#2481）は、この変更より前の前提で作られていた。

## 対処

`EnemyInvincibilityRegistry` で敵を無敵にするとき、`InvincibleBuff` に加えて `CharacterEntity.SetInvincible` も切り替える。これで `CanTakeDamage` が false になり、確定ダメージもバリアへの吸収も通らなくなる。敵の `SetInvincible` を呼んでいる箇所は、ほかにない（プレイヤーだけが使っている）。
