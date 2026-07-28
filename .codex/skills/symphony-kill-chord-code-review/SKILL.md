---
name: symphony-kill-chord-code-review
description: Review Symphony Kill Chord Unity/C# changes against the repository's coding guidelines, design philosophy, module boundaries, feature specifications, and ubiquitous language. Use for code reviews, staged or working-tree diff reviews, architecture checks, module/class necessity checks, refactoring reviews, and implementation-plan validation in this repository.
---

# Symphony Kill Chord Code Review

根拠となるコードと文書を照合し、仕様上の正しさ、責務、依存方向、既存機構への統合可能性をレビューする。

## 必須参照

レビュー開始時に、次の正本を全文読む。

- `Assets/Scripts/DesignPhilosophy.md`
- `Assets/Scripts/CodeGuidelines.md`
- `references/source-routing.md`

対象機能に関係する仕様書と用語ファイルだけを追加で読む。仕様書群全体を無差別に読み込まない。

## レビューフロー

1. `git status --short`で状態を確認する。
2. ユーザーが「ステージ済み」と指定した場合は`git diff --cached`を使う。差分が空ならその事実を明示し、文脈上明らかな場合だけ作業ツリー差分を代替対象にする。
3. 差分の型名、名前空間、機能語から関連する用語・仕様書を特定する。
4. 変更箇所だけでなく、その呼び出し元、公開先、購読解除、生成・破棄、既存の類似クラスを検索する。
5. 推測ではなく、`using`、継承、interface実装、コンストラクタ、メソッド呼び出し、イベント、`SerializeField`、ScriptableObject参照、ServiceLocator登録を根拠に判断する。
6. 指摘を重要度順に報告する。問題がなければ明示する。

## 判定順序

次の順で確認する。

1. 現在の明示仕様とユビキタス言語に反していないか。
2. 実行時の不具合、状態破損、イベント多重登録、解除漏れ、初期化順違反がないか。
3. レイヤーとモジュールの依存方向が正しいか。
4. 既存クラスへ自然に統合できる責務を重複実装していないか。
5. コードガイドライン、Unityシリアライズ互換性、命名に反していないか。
6. 必要なテスト・検証が不足していないか。

## 新規クラス・Moduleの必要性

新設を認める前に次を確認する。

- 同じ状態、イベント、ViewModel、Repository、初期化ライフサイクルを既存クラスが既に所有していないか。
- 新クラスの責務を一文で説明できるか。
- 新Moduleが独立したロード、Build、Ready、Shutdownまたは実行順を本当に必要とするか。
- Containerの公開物を別Moduleが実際に消費するか。登録元しか使わないContainerは作らない。
- イベントの記録は既存のRecorder、表示は既存Presenter、生成制御は対象システムのController/Initializerへ置けないか。
- 統合によって既存クラスが複数の変更理由を持つ場合は、無理に統合しない。

`InitializationCoordinator`が全Moduleの各フェーズを順番に完了してから次フェーズへ進むことと、対象オブジェクトがBuild/Readyのどちらで公開されるかを確認して実行順を判断する。

## 仕様・用語の扱い

- ユーザーが現在のタスクで明示した決定を最優先する。
- 対象機能の最新計画書、Notion仕様、用語リスト、コードの順に意味を照合する。
- `【要確認】`、説明が空の用語、相互に矛盾する文書は確定事項として扱わず、不明点として報告する。
- コード上の名前が仕様語と異なる場合、単なる表記差か概念の混同かを区別する。
- 仕様語をクラス名へ機械的に直訳せず、Domain上の責務と既存命名に合わせる。

## 出力

指摘を先に示し、各指摘へ次を含める。

- 重要度
- 対象ファイルと最小行範囲
- 発生条件または依存関係
- 仕様・設計上の根拠
- 最小の修正方針と影響範囲

要約では、残す新規クラス、既存へ統合するクラス、削除可能なクラスを分ける。スコアや準拠率はユーザーが求めた場合だけ出す。

レビュー依頼だけならコードを変更しない。修正も依頼された場合は、指摘を確定してから変更し、関連テストまたは静的検証を行う。
