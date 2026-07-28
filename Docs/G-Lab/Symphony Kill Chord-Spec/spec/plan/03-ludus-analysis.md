# Stage 2: Ludus プレイ分類分析

**ステータス: complete（分類対応は完了）。製品固有の結論は本リポジトリ内だけに保持する。**

Ludus 公開 OKF bundle を commit `b949cfa136fa27de101ace324f99a715f17e6846` に固定し、`tools/verify-okf-bundle.mjs` で 907 type documents、906 property-graph nodes、2,863 edges を検証した。辞書全体は複製せず、選択した stable ID と採否だけを `spec/data/ludus-okf-selection.json` に保存した。

## 選択した分類

| Ludus ID | Symphony Kill Chord への適用 | 判断 |
|---|---|---|
| `genre:rhythm` | 入力間隔の拍判定、コマンド列、音楽同期 | 主分類。ただし既存譜面を再生する標準音ゲーではなく、TPS行動がノーツになる差分を保持 |
| `genre:action` | 移動、回避、ロックオン、即時フィードバック | 主分類。入力応答と回復可能性をリズム判定と同時に設計する |
| `genre:shooter` | TPS照準、障害物、命中、ヒットフィードバック | 補助分類。リコイル／弾薬など未採用の一般要素は自動要件化しない |
| `system:rhythm:s01` | ノーツ／判定 | 適用。判定規則はゲーム固有の6拍種＋All/Justを優先 |
| `system:rhythm:s02` | 段階的難易度 | 適用。仕様の4/8拍→3/6/1拍、研究による救済と対応 |
| `system:action:character-controller` | 移動・回避・入力応答 | 適用。入力バッファ等は候補であり未確認要件ではない |
| `system:action:camera` / `system:action:lock-on` | 手動カメラ、自動／手動ロック | 適用。意図しない視点移動を設定で解除できる仕様を重視 |
| `system:action:combo` | 最大コンボと達成フィードバック | 部分適用。仕様上のコンボ報酬式は未確定 |
| `system:action:i-frames` | 回避・立て直し | 調査候補。仕様／コードで無敵時間の正式契約を確定できない |
| `system:shooter:s03` | 命中の即時フィードバック | 適用。HUD仕様が薄く、複合フィードバックの受入条件が必要 |
| `system:shooter:s06` | 障害物と命中方式 | 部分適用。リアル弾道を要求せず、仕様のline-of-sightだけを採用 |

## ドメインへの影響

Ludus の `音とリズム`、`入力と操作`、`演出とフィードバック`、`アクセシビリティと支援` を横断要素として確認した。これにより Music と Rhythm、Action と Targeting、Guidance と View adapter を別境界として保つ判断が補強された。一方、Ludus は汎用辞書であり、製品固有のKill Chord、2小節BGM編成、研究ツリー、ミッション報酬の正本にはしない。

## データ境界

この解析結果、固有名、ローカルパス、実装ギャップは Ludus へ自動書き戻ししない。公開可能な汎化知識を提案する場合のみ、別途人間レビュー付きPRで扱う。
