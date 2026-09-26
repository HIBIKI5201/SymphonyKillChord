---
task: developproducts-research-scope-review
project: SymphonyKillChord
kind: 設計相談
created: 2026-08-24
memory_links:
  - Assets/Scripts/DesignPhilosophy.md
---

# DevelopProducts/Research・Design のスコープレビュー

## 目的

2026-08-24 のプログラムドメイン解析で、`Assets/DevelopProducts/Research/**`（402 symbols）と
`Assets/DevelopProducts/Design/**`（189 symbols）を合わせた研究/デモコードが計 591 symbols /
79 modules に達しており、本番の `1.Domain` 層（474 symbols）に匹敵する規模であることが判明した。
`DesignPhilosophy.md` 上は「技術研究向け・先行研究用デモコード」の位置づけだが、内訳は
SaveSystem(92) / BossSystem(85) / SkillTree研究(83) / Pause(42) / BindingSystem(37) /
ToonShader(20) / AnimationControl(19) / EquipmentBGM(8) / RenderingExtension(8) /
Achievement(6) / SpringBone(2)（Research系）と Persistent(102) / GameMode(53) /
Architecture(34)（Design系）に及ぶ。放置すると規模だけが増え続け、本番昇格すべきものと
廃棄すべきものの区別がつかなくなる。

## 完了条件

- Research/Design配下の各サブシステムについて、次のいずれかの方針が決まっている:
  昇格（Runtimeへ本実装として取り込む） / 継続研究（保留） / 廃棄（削除 or アーカイブ移動）。
- 方針が「昇格」または「廃棄」に決まったサブシステムは、対応する後続実装taskとして
  `spec/tasks/` へ別途分解されている（このtask自体はコード変更を伴わない）。
- 方針とその理由が本taskの完了報告、または `spec/domains/` 側のドメイン定義
  （`formalize-program-domains` task）に反映されている。

## スコープ (編集可ディレクトリ)

- 方針決定のみ。このtaskでのソースコード変更は行わない。
- 実際の昇格/削除作業（`Assets/DevelopProducts/**` の変更）は、方針確定後に
  サブシステム単位で別taskへ分解する。
