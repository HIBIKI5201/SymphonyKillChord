# HealthEntity

- id: 2b37c2c6-cc02-8077-92bb-e724080ddd5f
- path: Symphony Kill Chord / システム概要 / 音楽同期戦闘モック / キャラクター共通 / HealthEntity
- last_edited: 2025-11-27T04:22:01.107Z


### クラス概要
キャラクターの体力計算と死亡判定を行う。

## API

## OnDeath
死亡イベント。

### OnHealthChanged<float,float>
ヘルスの変化イベント。


### TakeDamage(float amount)
ダメージを受ける。
HP計算  + 死亡判定を行う。

### Die()
死亡処理。
死亡イベントの発火。