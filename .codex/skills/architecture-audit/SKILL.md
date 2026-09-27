---
name: architecture-audit
description: "Audit whether the whole Symphony Kill Chord codebase (Assets/Scripts/Runtime and related) follows the project's design philosophy (Assets/Scripts/DesignPhilosophy.md): module dependencies, class responsibilities, DI in the Composition layer, cross-module references only through the Adaptor layer, circular dependencies, and an architecture quality score with concrete improvement proposals. Use when the user asks for an architecture audit, a design-philosophy compliance check, a refactoring survey, or a module/layer dependency analysis across the project (not a review of one diff — use code-guideline-check for that)."
---

# Architecture Audit

プロジェクト全体が設計思想に従って実装されているかを、ソフトウェアアーキテクトとして調査する。
差分のレビューではなく、モジュール・レイヤー単位の横断的な監査に使う（1 つの変更のレビューは `code-guideline-check` を使う）。

## 必須参照

監査を始める前に、次の正本を全文読む。

- `Assets/Scripts/DesignPhilosophy.md`（レイヤー構成・責務・DI・モジュール間公開の規則）
- `Assets/Scripts/CodeGuidelines.md`（命名・クラスの種類）

`Assets/Docs/ScriptsDocs/Architecture.txt` は設計思想の写しなので、正本と食い違うときは正本を優先する。

## 最重要事項

推測ではなく、実際のコードから依存関係を解析する。必ず次を根拠として判断し、報告には `ファイル:行` を付ける。

- 名前空間、クラス名、using
- 継承、Interface 実装
- コンストラクタ DI
- メソッド呼び出し、static 参照
- イベント
- Unity の SerializeField、ScriptableObject 参照

レイヤー間の参照は、まず各レイヤーの asmdef（`Assets/Scripts/Runtime/*/KillChord.*.asmdef`）の `references` で確かめ、次に using と型参照で確かめる。

## 調査の範囲

ユーザーの指定が無ければ `Assets/Scripts/Runtime/` 全体を対象にする。範囲が広く一度に読み切れないときは、モジュール（`InGame/Enemy` など）ごとに分けて調べ、どこまで調べたかを報告に書く。調べていない範囲を「問題なし」と書かない。

## 調査内容

### 1. モジュール依存関係

各モジュールについて、参照しているモジュールと、参照されているモジュールを一覧にする。依存方向を図（Mermaid など）でも整理する。

### 2. クラス責務の検証

各クラスが設計思想で定義された責務になっているかを確かめる。対象のクラスの種類の例:

Entity / ValueObject / Factory / Controller / Presenter / State / DTO / ViewModel / Signal / Spawner / Config / Asset / Repository / Initializer / Debugger

責務と異なる実装があれば報告する。

### 3. DI の確認

依存性が Composition 層で解決されているかを確かめる。永続的なオブジェクトを `new` している箇所があれば報告する。

### 4. モジュール間依存

設計では「他モジュールへの依存は Adaptor 層のみ」となっている。これを満たしているかを確かめ、違反箇所を一覧にする。

### 5. 循環参照

循環依存を、クラス単位・モジュール単位・名前空間単位のそれぞれで検出する。

### 6. アーキテクチャ品質

次の観点を 100 点満点で評価する。点数ごとに、根拠になった箇所を示す。

- レイヤー分離
- SOLID
- DDD
- CQRS
- MVVM
- 保守性
- 拡張性
- テスト容易性

### 7. 改善案

改善案には、修正対象クラス・問題点・改善理由・修正方法・影響範囲を具体的に書く。

## 出力形式

### レイヤー一覧

| Layer | Classes |
|--------|---------|

### モジュール依存

| Module | Depends On | Referenced By |
|--------|------------|---------------|

### クラス責務違反

| Class | Expected | Actual |
|-------|----------|--------|

### 循環依存

| Classes | Description |
|---------|-------------|

### 改善案

| Priority | Class | Recommendation |
|----------|-------|----------------|

最後に、この設計思想への準拠率を「〇〇%」として評価し、その算出の根拠を書く。さらに、追加すると有効な観点があれば挙げる。

## 報告の置き場

報告を保存するときは `Docs/agent/`（gitignore 対象）に置く。既に Issue がある指摘（`runtime-audit` ラベルなど）は、重複して報告せず Issue 番号を示す。
