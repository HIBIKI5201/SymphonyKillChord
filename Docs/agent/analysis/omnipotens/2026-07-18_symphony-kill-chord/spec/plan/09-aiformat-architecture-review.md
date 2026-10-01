# Stage 7: AI Formatとアーキテクチャ健全性レビュー

**ステータス: complete（静的証拠について完了）。ランタイム品質は未検証。**

## 総合評価

Static readiness: **C**。Clean Architecture/DDDの意図、typed concepts、明示的composition lifecycleは強い。一方、仕様35ページとUnity-aware graphは rhythm recovery、obstacle hit、adaptive music、reward/save、guidance traceability、automated verificationの高優先gapを示す。

| Area | Grade | Evidence |
|---|---|---|
| Design strength | B | 7 layer assemblies、typed IDs/value objects、phased init/reverse shutdown |
| Domain alignment | C | 10境界を認識できるが Rhythm/Combat/Narrativeのcohesionが低くspec linksは8 |
| Unity lifecycle handling | B- | lifecycle認識でorphans 802→715、74 views検出 |
| Scene/flow concentration | C | 98–102 coupling、47–50 cyclomaticのscene transition hotspots |
| Persistence | C- | category一部実装、version/encryption/atomic recovery不足 |
| Data-driven design | C | asset定義あり、enemy damage・一部effectがhard-coded/TODO |
| Test strategy | D | tracked executable tests/CI performance gateを確認できない |
| Build reproducibility | C | action pinningは良いが外部ZIPがmutable/unchecked |
| Runtime performance | not rated | FPS/load/memory目標はあるが測定なし |

201 cycles、715 orphans、792 spec gapsは自動候補であり欠陥数として扱わない。reflection、DI、serialized Unity referencesと同名method誤結合を人手で除外してから改修する。

## 推奨アーキテクチャ境界

- Musical clockとBGM arrangementを分け、Kill Chord loadoutは公開event/portでarrangementへ渡す。
- Rhythm inputはnote判定とcommand resolutionを所有し、Combat effectへ成立結果だけを渡す。
- GuidanceはView寄せ集めではなく、cue priority、tutorial competence、recovery stateを所有するapplication/domain boundaryにする。
- Scene transition hotspotはrequest、policy、loading progress、completion notificationを分け、exactly-once条件をtestする。
- `GS-*` requirement IDをtest/asset metadataへ持たせ、68 clauses→code/testのtraceabilityを増やす。

最初の実装束は All/timeout、line-of-sight、reward idempotency、save recovery、HUD cue acceptance の5本とし、各々をEditMode/PlayModeで固定してから構造分割へ進む。
