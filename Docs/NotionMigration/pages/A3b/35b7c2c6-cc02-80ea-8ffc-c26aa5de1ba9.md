# 攻撃パイプライン・キャラクタデータ 2026-05-09

- page_id: 35b7c2c6-cc02-80ea-8ffc-c26aa5de1ba9
- Notion パス: Symphony Kill Chord / システム概要 / 攻撃パイプライン・キャラクタデータ 2026-05-09
- スナップショットの最終更新: 2026-05-14T11:18:31.618Z
- 書き込み許可: 可
- 処置: アーカイブ推奨

## 判定

- 信頼性: 陳腐化（記録としては概ね正確） — 「改修」節の内容は実装と一致する。`CriticalChance` は 0〜1 の範囲外で例外（`CriticalChance.cs:22-24`）、`CriticalMultiplier` は負数で例外（`CriticalMultiplier.cs:18`）、`Damage` は負数を 0 に補正（`Damage.cs:21`）、`CharacterEntity` は `Guid Id` を持つ（`CharacterEntity.cs:59`）、`Health` は公開せず `CurrentHealth`・`MaxHealth` のみ公開（`CharacterEntity.cs:65,68`）。`DamagePresenter` は現存せず、`AttackResultView` は残っている（4.View/InGame/Battle）
- 整合性: 現行のモジュール文書 `システム概要 / キャラ&バトル`（3957c2c6-cc02-80d8-b0c3-d586b36180fa）と題材が重複する。ページ内の誤り: 「無効な BeatType 設定が静かに握りつぶされる」と「被弾イベント対象識別に GetHashCode() を使用していた」の「対応内容」が、直前の項目の文（未使用メソッド・プロパティを削除した）の貼り間違いになっている。実際の対応は各項目の「例外対応」に書かれている
- 変更点:
  - ページ冒頭（「各レイヤーの色分け」の前）: アーカイブのコールアウトと、残す設計判断の要点を追加
  - 既存本文は直さない（アーカイブのため）。貼り間違いはコールアウトで注記する
- 織り込んだ反映項目: BT-41, RL-14
- 出典: 実装（上記ファイル）、`10_インゲーム戦闘コア.md` BT-41
- 要確認: アーカイブの置き方（RL-14、要企画確認）。W-14 のキャッシュマップ確認

## 適用する本文

（ページ冒頭、「各レイヤーの色分け」の前に挿入する）

> 📦 **アーカイブ（2026-05 のリファクタリング記録）**
このページは 2026-05 に行った攻撃パイプラインとキャラクターエンティティのリファクタリングの記録である。現行の構成は「システム概要 / キャラ&バトル」を参照する。
「無効な BeatType 設定が静かに握りつぶされる」と「被弾イベント対象識別に GetHashCode() を使用していた」の「対応内容」は貼り間違いである。実際の対応は各項目の「例外対応」のとおり。
現行実装に残っている設計判断は次のとおり。
- 命中判定は `AttackExecutor.Execute()` の外（攻撃する側）で行い、命中したときだけ攻撃を実行する
- `CharacterEntity` は内部の `HealthEntity` を公開せず、`CurrentHealth`・`MaxHealth` だけを公開する
- `CriticalChance` は 0 以上 1 以下、`CriticalMultiplier` は 0 以上でなければ生成時に例外を出す
- `Damage` は負の値を例外にせず 0 に補正する
- 被弾イベントの対象は `GetHashCode()` ではなく、エンティティごとの GUID（`CharacterEntity.Id`）で識別する
- `AttackStepContext` の `IAttacker`・`IDefender` は、攻撃力・防御力・属性などの Step 拡張のために残している
