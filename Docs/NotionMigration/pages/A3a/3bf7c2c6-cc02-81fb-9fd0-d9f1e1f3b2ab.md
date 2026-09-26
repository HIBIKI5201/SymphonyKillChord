# ① スキル装備フロー（ドラッグ&ドロップ）

- page_id: 3bf7c2c6-cc02-81fb-9fd0-d9f1e1f3b2ab
- Notion パス: Symphony Kill Chord / システム概要 / スキル編成 / ① スキル装備フロー（ドラッグ&ドロップ）
- スナップショットの最終更新: 2026-08-17T17:49:29.758Z
- 書き込み許可: 可（システム概要 配下）
- 処置: 本文置き換え
- 確認日 / 確認したコミット: 2026-09-22 / 73fefeb45

## 判定

- 信頼性: 一部古い — 「編集中の状態を先に更新し、確定時にまとめて検証・保存する」という流れは現行と合う。ただし図の経路が実装と違う。編集中の状態を持つのは `SkillBuildViewModel`（View）で、ドロップは `SkillBuildViewModel.ApplyDrop` が `SkillBuildSlotState.ChangeCurrentSkill` で反映する。保存は `SkillBuildViewModel.SaveAsync` → `ISkillBuildCommand.SaveAsync`（実装は `SkillBuildController`）→ `SkillBuildUseCase.SaveSkillBuildAsync` で行い、成功したら ViewModel が保存済み状態へ確定する（`CommitCurrentAsInitial`）。コントローラーでの装備（PR #1640・#1689）が無い
- 整合性:
  - 実装: `Assets/Scripts/Runtime/4.View/OutGame/SkillBuild/SkillBuildViewModel.cs:105-150,177-187,280-290`、`Assets/Scripts/Runtime/3.Adaptor/OutGame/SkillBuild/SkillBuildController.cs:57`、`Assets/Scripts/Runtime/2.Application/OutGame/SkillBuild/SkillBuildUseCase.cs:35`、`Assets/Scripts/Runtime/4.View/OutGame/SkillBuild/SkillElementControllerEquipController.cs:219-235`、`Assets/Scripts/Runtime/4.View/OutGame/Screen/SkillBuildScreenView.cs:709`（保存ボタン）
  - 他ページ: 未保存のまま戻るときのダイアログなど画面の操作は `仕様概要 / 改造画面`（3437c2c6-cc02-80a5-bcb6-ed675cbb7fb2）の担当（OG-120）
- 変更点:
  - 導入文: コントローラーでも同じ編集中状態を更新することを追記
  - シーケンス図: 参加者に `SkillBuildViewModel` を追加し、ドロップ → `ApplyDrop` → `ChangeCurrentSkill`、保存 → `SaveAsync` → `SkillBuildController` → `SkillBuildUseCase` → `CommitCurrentAsInitial` の経路に直した。コントローラー操作の分岐を追加
- 織り込んだ反映項目: OG-121（スキル編成の子ページの追従）
- 出典: 上記の実装箇所
- 要確認: なし

## 適用する本文

編集中の状態を先に更新し、確定時にまとめて検証・保存する。マウスのドラッグ&ドロップと、コントローラーの2段階の決定（選択 → 持ち上げ → スロットを選ぶ）のどちらも、同じ編集中状態を更新する。
```Mermaid
sequenceDiagram
    autonumber
    actor Player as プレイヤー
    participant Drag as SkillElementDragAndDropManipulator
    participant Pad as SkillElementControllerEquipController
    participant VM as SkillBuildViewModel
    participant State as SkillBuildSlotState
    participant Layout as SkillBuildSlotLayout
    participant Ctrl as SkillBuildController
    participant UseCase as SkillBuildUseCase

    alt マウス
        Player ->> Drag: スキルアイコンをスロットへドロップ
        Drag ->> VM: ApplyDrop(スキルID, スロット番号)
    else コントローラー
        Player ->> Pad: カードを決定（選択）→ もう一度決定（持ち上げ）→ スロットを決定
        Pad ->> VM: 装備先スロットを反映
    end
    VM ->> State: ChangeCurrentSkill（編集中状態を更新）
    VM ->> Layout: UI配置へ反映
    Player ->> VM: 保存ボタン（SaveAsync）
    VM ->> Ctrl: SaveAsync(スキルID一覧)（ISkillBuildCommand）
    Ctrl ->> UseCase: SaveSkillBuildAsync（構成の検証と保存）
    UseCase -->> VM: 保存結果
    VM ->> State: CommitCurrentAsInitial（保存済み状態を更新）
```
