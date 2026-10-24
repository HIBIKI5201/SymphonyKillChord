# コード規約

正本は Notion の「コード規定」。このファイルはその写しで、規則の番号（CG-xxx）も Notion と同じにする。食い違いに気付いたら Notion を正とし、このファイルを直す。

## 設定

- CG-001 cs, md, txtのファイルはUTF-8を使用する
- CG-002 AIもコードの実装・修正とPRの作成を行う。AIの変更を取り込むPRのレビューは人が行う。ブランチ・PRの運用はGitHub 運用規定の「AI エージェントの運用」（リポジトリの [AGENTS.md](../../AGENTS.md)）に従う
  - 実装・修正は、依頼者がその作業を明示的に頼んだときに行う。レビュー・調査だけの依頼ではコードを変更しない（[AGENTS.md](../../AGENTS.md#作業の許可範囲)）

## 順序

CG-010 クラス・構造体のメンバーは次の順に並べる。

1. **コンストラクタ**
2. **イベント (public events)**
3. **パブリックプロパティ**
4. **インターフェースプロパティ**
5. **パブリック定数(const/readonly)**
6. **パブリックメソッド**
7. **パブリックインターフェースメソッド**
8. **パブリックEnum定義**
9. **パブリックclass定義**
10. **パブリックStruct定義**
11. **定数 (const/readonly)**
12. **インスペクター表示フィールド ([SerializeField])**
13. **プライベートフィールド**
14. **Unityライフサイクルメソッド (Awake, Start, Updateなど)**
15. **イベントハンドラメソッド (プレフィックスがHandleのタイプ)**
16. **プロテクトインターフェース・仮想メソッド**
17. **プライベートメソッド**
18. **内部メソッド、抽出メソッド**
19. **プライベートEnum定義**
20. **プライベートclass定義**
21. **プライベートStruct定義**
22. **デバッグ機能**

## サマリー・コメント

- CG-020 全てのメソッドにはサマリーを付ける
- CG-021 パブリックなプロパティとイベントにはサマリーを付ける
- CG-022 プロパティ, イベント, フィールド変数かつ一行のサマリーは以下のように一行かつスペースを入れて書く
/// <summary> comment </summary>
- CG-023 クラス, メソッドまたは二行以上のサマリー以下のように改行してインデントを上げて書く
/// <summary>
///     comment1
///     comment2
/// </summary>
- CG-024 コメントは基本的に日本語を使用する
- CG-025 日本語の最後は「。」で終える
- CG-026 メソッド内の処理には適宜コメントを付ける
- CG-027 SerializeField属性を使用するフィールドには、TooltipAttributeでの説明を付ける。説明は日本語で書く
```csharp
[SerializeField, Tooltip("研究ポイント")]
private int _researchPoint;
```
- CG-028 クラスにもサマリーを付ける

## 命名規則

- CG-030 C#のフィールド変数はキャメルケースでプレフィックスに `_`
- CG-031 C#のクラス、プロパティ、メソッドはパスカルケース
- CG-032 C#の引数はキャメルケース
- CG-033 定数はアッパースネークケース。const だけでなく static readonly の定数も含む
```csharp
private const int MAX_STAGE_READY_WAIT_FRAME = 120;
private static readonly StatusEffectId EFFECT_ID = new("Skill07.AttackPowerIncreaseBuff");
```
- CG-034 イベントのプレフィックスは `On`
- CG-035 イベントハンドラのメソッド名はプレフィックスを `Handle` にする（Handle〇〇）。イベントを処理するクラスの名前はサフィックスを `Handler` にする（〇〇Handler）
```csharp
// メソッド名（EnemyAttackReservationUsecase.cs）
private void HandleReservedTimingReached()
// クラス名（TextEventHandler.cs）
public class TextEventHandler : IScenarioEventHandler<TextEvent>
```
- CG-036 インターフェースは `I` で始める
- CG-037 メソッドは動詞で始める
- CG-038 bool型のプロパティは `Is` または `Has` で始める
- CG-039 uxml, ussの命名はロウワーチェーンケース
- CG-040 名前空間は、レイヤーフォルダの先頭の番号を除いたフォルダ構成をそのまま反映する
```
Assets/Scripts/Runtime/1.Domain/InGame/Skill/SkillDefinition.cs
  → namespace KillChord.Runtime.Domain.InGame.Skill
Assets/Scripts/Runtime/6.Composition/InGame/Player/PlayerInitializer.cs
  → namespace KillChord.Runtime.Composition.InGame.Player
```
- CG-041 クラス名のサフィックスは実際の役割を表す語にする。Controller はUseCaseへの委譲を行うクラスにだけ使い、状態機械の駆動・判定・記録などを行うクラスに付けない（例: 記録するクラスは 〇〇Recorder）。既存クラスの改名は必須とせず、新しく書くクラスから適用する

## その他

- CG-050 `using` ディレクティブはファイルの先頭にまとめて記述する
- CG-051 フィールド変数を公開する際はプロパティを使用する。変更を伴う場合は `public set` ではなく、`SetXxx` / `RecordXxx` のような意図が伝わるメソッドを介する
```csharp
public bool IsCompleted => _isCompleted;

public bool Complete()
{
    if (_isCompleted) { return false; }
    _isCompleted = true;
    return true;
}
```
- CG-052 アクセス修飾子は必ず明示的に記述する
- CG-053 マジックナンバーは排除し、代わりに定数で定義する
- CG-054 一行のブロックの場合でも以下のように、波カッコを使用する
{ Console.WriteLine("message"); }
- CG-055 1ファイルには1つの公開型（class / struct / interface / enum）のみを定義し、ファイル名を型名と一致させる。同じファイルに複数の型が要る場合は、片方をもう一方のプライベートなネスト型にする
- CG-056 ServiceLocatorから依存を取得するときは、必須の依存は `GetInstance`、無くてもよい依存は `TryGetInstance` を使う（設計思想の DP-302 を参照）
- CG-057 ServiceLocatorからの取得と失敗時のログの定型が続く場合は、ヘルパーメソッドにまとめる
- CG-058 静的シングルトン（`static Instance`）を作らない。サービスはComposition層で生成してServiceLocatorへ登録するか、必要な箇所へ注入する
- CG-059 Composition層は依存をつなぐことに専念する。業務ロジックやUIレイアウトの計算はViewまたはAdaptorのクラスに切り出す
- CG-060 1ファイルが400行、1メソッドが60行を超えたら、責務を分けて分割することを検討する
- CG-061 他のオブジェクトのメンバーを3段以上たどらない（`a.B.C.D` のような呼び出し）。必要な値は引数やメソッドで受け取る
- CG-062 値の変化を通知するときは、C# の event より R3 の ReactiveProperty を優先する。トゥイーンは手書きせず LitMotion を使う

## UnityAPI

- CG-070 データコンテナとして使用するScriptableObject（CreateAssetMenuが付く物）は
外部からのアクセスにはパラメータを読み取り専用にして
SerializeFieldのみで変更できるようにする
- CG-071 View層では `FindObjectsByType` / `FindFirstObjectByType` / `FindAnyObjectByType` でシーンを走査して依存を探さない。参照は `[SerializeField]` か、Composition層から渡す。Composition層で走査する場合も、取得に失敗したらログを出す
- CG-072 `Camera.main` を直接参照しない。カメラはカメラの所有者（CameraSystemView）から受け取る

## ログ出力

- CG-080 `Debug.LogError` / `Debug.LogWarning` は `[{nameof(ClassName)}] メッセージ` の形式で書き、MonoBehaviourの場合は第2引数に `this` を渡す
```csharp
Debug.LogError($"[{nameof(IngameComposition)}] バトルステージが選択されていません。", this);
```
- CG-081 この形式に従っていない古いログ（特に 5.InfraStructure と 4.View の一部）は、見つけても修正を必須としない。新しく書くログは必ず CG-080 の形式にする

## Notion に未記載の規則

Roslyn アナライザ（SKC0016）で検査している規則。Notion の「コード規定」へ追記するまで、ここに残す。

- `async void` は、Unityのイベント関数やイベントハンドラなど戻り値を `void` にしなければならない場合だけ使う。その場合は本体全体を `try`/`catch` で囲み、`OperationCanceledException` 以外の例外を `Debug.LogException` で出す。それ以外の非同期メソッドは `Task` / `Awaitable` / `UniTask` を返す
