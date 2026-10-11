---
name: skc-feature-flow
description: "feature-flowで作業ブランチ作成、実装、同階層masterへのセルフマージ、develop向けDraft PR確認まで進める。"
---

# Feature Flow

[AGENTS.md](../../../AGENTS.md) を優先する。Git・PRの段取りを担当し、実装は対象に必要なスキルだけを使う。

1. ブランチ操作前に [運用規則](references/branch-rules.md) と [ブランチ作成・実装](references/branch-and-implementation.md) を読む。新規ブランチを編集前にpushし、同階層masterの生成とSHAを確認してから編集する。
2. 実装・検証・コミットは作業ブランチ上で行う。対象パスだけをstageし、既存変更を保持する。C#変更時は利用可能なcompile・reviewスキルを使う。Unity外の資料変更だけならUnity検証は不要。
3. PR作成前に [二段階PR](references/pull-requests.md) と [.github/PULL_REQUEST_TEMPLATE.md](../../../.github/PULL_REQUEST_TEMPLATE.md) を読む。agent→masterを作成し、チェック成功後にmergeする。自動生成されたmaster→develop Draftの本文・対象を確認する。作成したPRは利用可能ならチャットへ添付する。
4. 自動生成失敗・差分なしのときだけ [復旧](references/recovery.md) を読む。既存PRへの追加push前はstateとheadを確認し、マージ済みPRを更新対象と誤認しない。
5. 作業ブランチ・両PR・確認済みと未確認を報告する。developは明示指示なしにマージせずDraftで残す。

各段階で必要な参照だけを読み、すべてのreferenceや無関係なスキルを一括で読まない。署名・Co-Authored-Byは会話で指定されたものがある場合だけ付ける。
