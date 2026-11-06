# CRI キューシート確認（2026-11-01 版）

サウンドから届いた 2026-11-01 版の `CueSheet_0.acb` を、サウンドのキュー一覧スプレッドシート（赤字 = 今回追加したボイス）と突き合わせた結果。

- 対象: `Assets/StreamingAssets/CueSheet_0.acb`（18,263,808 B、ACB Ver.1.46.0）。前の版は 16,311,680 B。
- 方法: ACB の CueName・Cue テーブルを読み、キュー名・Cue ID・長さを取り出して、前の版とスプレッドシートの A 列（キューネーム）と比べた。
- キュー数: 86 → 125（+39）。消えたキュー・Cue ID が変わったキューはない。

## スプレッドシートの赤字（39 件）はすべて入っている

| グループ | キュー名 | 件数 | Cue ID |
| --- | --- | --- | --- |
| Symphony ホーム | `VOICE_Symphony_Home2` | 1 | 91 |
| Symphony シナリオ掛け声 | `VOICE_Symphony_Scenario1_1`〜`1_3`、`2_1`〜`2_3`、`3_1`〜`3_3`、`4_1`〜`4_3`、`5`、`6_1`〜`6_3` | 16 | 92〜107 |
| Triad シナリオ掛け声 | `VOICE_Triad_Scenario1_1`〜`1_3`、`2_1`〜`2_2`、`3_1`〜`3_3`、`4`、`5_1`〜`5_3`、`6_1`〜`6_3` | 15 | 108〜122 |
| 14号 | `VOICE_Soldier14_Attack`・`Damage`・`Reload`・`SpecialAttack`・`Humming`・`Gameover`・`Gameclear` | 7 | 123〜129 |

## 食い違い

1. **`SE_Cursor` がスプレッドシートにあるが ACB にない。** 前の版にもなかった。コードからも参照していない。
2. **旧名のボイス 9 件が ACB に残っている。** `VOICE_Battlestart`・`VOICE_Home1`・`VOICE_Home3`・`VOICE_Damage_Light`・`VOICE_Damage_Heavy`・`VOICE_Gameclear1`〜`3`・`VOICE_Skill`。スプレッドシートにはなく、`VOICE_Symphony_*` と同じ内容の旧名と見られる（長さがほぼ同じ）。このうち 3 件を `Player.prefab` の `PlayerView` が使っている（`_damageVoiceCueName: VOICE_Damage_Light`、`_stageStartVoiceCueName: VOICE_Battlestart`、`_skillVoiceCueName: VOICE_Skill`）。サウンドが旧名を消すと鳴らなくなるので、`VOICE_Symphony_*` へ付け替えるか、旧名を残してもらうかを決める必要がある。
3. **`SE_Just` はまだない**（スプレッドシートにも ACB にもない）。#2540 は引き続きサウンド待ち。

## 長さが変わった既存キュー（差し替えと見られる）

| キュー名 | 前 (ms) | 今 (ms) |
| --- | --- | --- |
| `VOICE_Symphony_Gameclear2` | 6912 | 6881 |
| `VOICE_Symphony_Gameclear3` | 7078 | 7005 |
| `VOICE_Symphony_Skill` | 3355 | 3291 |
| `VOICE_Triad_Tutorial2` | 4300 | 4237 |
| `VOICE_Triad_Tutorial9` | 4866 | 4816 |

## 未対応（実装が要るもの）

- 追加した 39 件は、まだどのコード・アセットからも参照していない。シナリオ掛け声はシナリオ（会話）データ、14号のボイスは 14号の View に割り当てる作業が要る。
- スプレッドシートの「その他設定」（ボイスプール 50、BGM カテゴリのキューリミット 2、VOICE カテゴリのキューリミット 1、SE の距離優先）は ACF・Atom Craft 側の設定で、この確認では見ていない。
