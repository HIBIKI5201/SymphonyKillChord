# プランナー向けマスターデータ管理画面 設計計画書

## 1. 目的

`InfraStructure` 層のマスターデータ系 ScriptableObject を、プランナーが Unity Editor 上で安全に確認・編集・検証できる統合管理画面を設計する。

本計画書は、マージ後の現状実装を前提に、既存の `SourceDataProvider` 基盤を拡張しつつ「系統切り替え式の EditorWindow」をどう載せるかを整理する。

## 2. 現状整理

### 2.1 既に存在する基盤

現状のブランチには、前提として以下が実装済みである。

- `Assets/Editor/Scripts/SourceDataProvider/SourceDataProviderSettings.Editor.cs`
  - `ProjectSettings/SourceDataProviderSettings.asset` にカテゴリと Addressable リポジトリ対応を保持する。
- `Assets/Editor/Scripts/SourceDataProvider/SourceDataProviderSettingsProvider.cs`
  - `Project/Source Data Provider` の Settings 画面からカテゴリ設定を編集できる。
- `Assets/Editor/Scripts/SourceDataProvider/SourceDataProviderRepositoryResolver.cs`
  - Addressable の address からリポジトリアセットを解決し、配列プロパティや登録済み `DataID` 一覧を取得できる。
- `Assets/Editor/Scripts/SourceDataProvider/RepositoryAddressSelectorDrawer.cs`
  - `RepositoryAddressSelectorAttribute` 付き string フィールドに対し、登録済み Addressable キーをポップアップ選択できる。
- `Assets/Editor/Scripts/SourceDataProvider/SourceDataRegistrationHeader.cs`
  - 個別 ScriptableObject を Inspector で開いたとき、対応リポジトリへの登録・解除をヘッダーから実行できる。
- `Assets/Editor/Scripts/SourceDataProvider/DataIDPropertyDrawer.cs`
  - `DataCategory` と `DataID` を使った ID 入力・参照補助がある。

つまり、現状は「Addressables 連携の下地が無い」のではなく、**カテゴリ定義・リポジトリ解決・ID 選択補助までは既に揃っている**状態である。

### 2.2 現在の SourceDataProvider の制約

ただし、現状の `SourceDataProvider` は実質的に「Repository クラスを扱うための仕組み」として設計されている。

具体的には以下の前提が強い。

- Settings が `RepositoryMapping` という名前と責務で設計されている。
- Addressable キーから解決する対象を「リポジトリアセット」として扱っている。
- 個別データ列挙は `ArrayPropertyPath` を使った repository 内配列前提になっている。
- `DataID` の参照候補収集も repository を起点にしている。
- Inspector Header の登録 UI も「このアセットを、どの repository 配列へ登録するか」という発想になっている。

このため、`ScenarioSettingsAsset` のような単体設定アセットや、将来的に「Addressables でロードされるが repository ではない ScriptableObject」を同じ基盤で扱いにくい。

### 2.3 Runtime 側の接続状況

Runtime / Composition 側でも、Addressables キーの一部に `RepositoryAddressSelector` が使われ始めている。

代表例:

- `StageSelectInitializer`
- `ScenarioCom`
- `TitleSceneInitializer`

一方で、すべてのキーが統一されているわけではなく、従来どおりプレーンな string フィールドで保持している箇所も残っている。

### 2.4 現時点の課題

`SourceDataProvider` は「設定・登録・ID 補助」の基盤としては機能しているが、次の 2 つの不足がある。

1. プランナー向けの統合編集画面として不足している。
2. SourceDataProvider 自体が Repository 専用寄りで、対象 ScriptableObject の一般化が不十分である。

- 系統単位で編集対象を切り替える UI が無い。
- 複数リポジトリを横断してまとめて確認する画面が無い。
- ビジュアルシミュレーションが無い。
- Validation が Settings / PropertyDrawer / Inspector Header に分散している。
- プランナー導線が「各アセットを個別に開く」前提のままで、一覧性が低い。
- Addressables でロードする単体 ScriptableObject を、Repository と同じ仕組みで扱えない。
- `ArrayPropertyPath` が設定値ベースなので、Repository 側に「どの配列を列挙対象にするか」の宣言が埋まっていない。
- カテゴリと列挙可否の責務が Settings 側に寄りすぎており、ScriptableObject 型側の意図が見えにくい。

## 3. 今回のゴール

- `SourceDataProvider` を「Addressables でロードする全 ScriptableObject」を扱える基盤へ拡張する。
- Repository 系 ScriptableObject には、個別データ列挙とカテゴリを宣言できる Attribute を導入する。
- 既存 `SourceDataProvider` を土台にした統合 `EditorWindow` を用意する。
- 「系統」単位でタブまたはボタンを切り替え、それぞれ専用の編集画面を表示する。
- 各系統は、拡張後の `SourceDataProviderSettings` と型側メタデータを利用してデータ解決する。
- 数値や構造に意味がある系統には、ビジュアルプレビューまたは簡易シミュレーションを付ける。
- 既存の個別 Inspector 運用を壊さず、追加の上位導線として成立させる。

## 4. スコープ

対象:

- 統合 `EditorWindow` の構成
- `SourceDataProvider` の拡張計画
- 既存 `SourceDataProvider` との責務分担
- 系統切り替え UI
- リポジトリ解決と編集フロー
- ビジュアルプレビューの設計
- 実装フェーズ

対象外:

- すべての Runtime Addressables キーの完全統一
- 全マスターデータ型への一括対応
- CSV インポート / エクスポート
- Addressables グループ設計の全面見直し

## 5. 基本方針

### 5.1 方針

新しい管理画面は `SourceDataProvider` を置き換えるのではなく、**拡張した SourceDataProvider の上に乗る「プランナー向けフロントエンド」**として実装する。

### 5.2 守ること

- Editor 拡張は `Assets/Editor` 配下へ閉じ込める。
- Runtime の ScriptableObject 定義は再利用する。
- データ解決は可能な限り `SourceDataProviderSettings` と、拡張後の汎用 Resolver を使う。
- 個別アセットの Inspector 運用と共存させる。
- 巨大な 1 クラスではなく、系統ごとのページクラスへ分割する。
- ScriptableObject の一般メタデータと、Repository 系の列挙メタデータを分ける。

## 6. 想定ユーザー体験

1. プランナーが `Tools/KillChord/Planner Master Data` を開く。
2. 左側に系統ボタンが並ぶ。
3. `SkillTree`、`StageSelect`、`Scenario` などを切り替えると中央の編集内容が切り替わる。
4. 右側にプレビュー、参照状況、Validation 結果が出る。
5. 必要に応じて対象リポジトリへ `Ping` したり、個別アセット Inspector へ飛べる。
6. 保存前に簡易 Validation を流し、危険な不整合を先に見つけられる。

## 7. SourceDataProvider 拡張方針

### 7.1 拡張の考え方

`SourceDataProvider` は、今後は「Repository を探すツール」ではなく、**Addressables でロード対象になる ScriptableObject のメタ情報と Editor 導線を管理するツール**として再定義する。

その上で、Repository 系クラスだけが持つ追加責務として、

- 個別データの列挙元
- `DataID` 候補の収集対象
- Inspector Header からの登録先

を扱えるようにする。

### 7.2 役割の再定義

#### ScriptableObject 共通で扱うもの

- Addressable キー
- 表示名
- 種別
- プランナー向け系統
- 任意の説明

#### Repository 系だけが追加で扱うもの

- 個別データ配列の列挙
- 個別要素型
- DataID 候補収集の起点
- 個別アセット登録 / 解除

### 7.3 Settings の見直し

現状の `RepositoryMapping` は、概念としては狭すぎるため、将来的に以下のような汎化を行う。

- `SourceAssetMapping`
  - Addressable でロードする ScriptableObject 全体の登録情報
- `SourceCategoryMapping`
  - `DataID` 用カテゴリと、参照元 ScriptableObject の対応情報

初期段階では、既存 `RepositoryMapping` をすぐ全面置換せず、互換レイヤを挟んで段階移行する。

### 7.4 Attribute 設計

Repository 系 ScriptableObject に対して、個別データ列挙のための Attribute を追加する。

候補:

- `SourceDataCollectionAttribute`
- `SourceDataRepositoryAttribute`

本計画書では、意味が広すぎない `SourceDataCollectionAttribute` を仮称とする。

この Attribute で宣言したい情報:

- この ScriptableObject が個別データ列挙対象であること
- 列挙対象の配列 / List のプロパティパス
- 対応カテゴリ名
- 必要なら表示ラベル

例:

- `SkillNodeDataRepo` は `SkillNodes` を列挙対象に持つ。
- `OutGameSkillRepository` は `_skillDataAssets` を列挙対象に持つ。
- `BackgroundCatalogAsset` は `_entries` を列挙対象に持つ。

### 7.5 Category の扱い

Category は今後、次の 2 段階で扱う。

1. `SourceDataProviderSettings` に、Addressable ScriptableObject としての登録情報を持つ。
2. Repository 系クラスでは `SourceDataCollectionAttribute` でもカテゴリを宣言できる。

両者がある理由:

- Settings はプロジェクト全体の登録台帳である。
- Attribute は型側の意図を表現する。

最終的には、カテゴリ名の正本は Settings に置きつつ、Attribute 側は「この型がどのカテゴリ群を提供するか」の宣言として使う。

### 7.6 Resolver の一般化

`SourceDataProviderRepositoryResolver` は、名称と責務を一般化する。

候補:

- `SourceDataProviderAssetResolver`
- `SourceDataProviderObjectResolver`

役割:

- Addressable キーから任意の ScriptableObject を解決する。
- その型に `SourceDataCollectionAttribute` が付いていれば、列挙対象要素を取得する。
- 付いていなければ単体 ScriptableObject として扱う。
- `DataID` 候補収集も「repository 前提」ではなく「collection を持つ ScriptableObject 前提」へ改める。

### 7.7 Header UI の一般化

`SourceDataRegistrationHeader` も repository 固有 UI から一段一般化する。

- 単体 ScriptableObject の場合
  - SourceDataProvider 登録状態を表示する。
  - 対応 Addressable キーや系統を確認できる。
- Collection を持つ ScriptableObject の場合
  - 個別アセットの登録 / 解除を表示する。

これにより、単体設定アセットと collection 系アセットを同じ文脈で扱える。

## 8. 全体構成

### 8.1 構成要素

1. `PlannerMasterDataWindow`
2. `PlannerMasterDataCategoryDefinition`
3. `PlannerMasterDataCategoryRegistry`
4. `PlannerMasterDataPageBase`
5. `PlannerMasterDataPreviewPanelBase`
6. `PlannerMasterDataValidationService`

### 8.2 既存基盤との責務分担

#### 拡張後 `SourceDataProvider` 側

- Addressables でロードする ScriptableObject の登録台帳
- Addressable キーと ScriptableObject の対応保持
- ScriptableObject 実体の解決
- collection Attribute の解釈
- `DataID` 入力補助
- collection 系 ScriptableObject からの候補列挙
- 個別アセットの登録 / 解除

#### 新規 Planner Window 側

- 系統単位の編集導線
- 複数カテゴリ / 複数 ScriptableObject をまとめた画面構成
- 一覧表示、検索、選択状態管理
- 系統別プレビュー
- 系統別 Validation

## 9. 系統の考え方

ここでいう「系統」は、`SourceDataProvider` のカテゴリ 1 件と完全一致する必要はない。

`SourceDataProvider` のカテゴリは ID 解決やリポジトリ特定の単位であり、プランナーの編集単位はそれより大きくてよい。

例:

- `SkillTree` 系統
  - `SkillNode`
  - 必要なら `Skill`
- `Scenario` 系統
  - `ScenarioBackground`
  - `ScenarioPortrait`
  - `ScenarioAnimation`
  - `ScenarioSettingsAsset`
- `StageSelect` 系統
  - `Stage`
  - `EnemyMissionKey`
  - `StageEffect`

つまり、**Planner Window は複数カテゴリを束ねる上位概念**として扱う。

## 10. 現状に合わせたデータ取得方式

### 10.1 基本方針

EditorWindow 自体は、独自に Addressables から非同期ロードするのではなく、拡張後の Resolver を利用して **Addressable address から Editor 上の ScriptableObject 実体を解決する**方式を優先する。

理由:

- 現実装が既に `AssetDatabase.LoadMainAssetAtPath` ベースで安定している。
- Settings 画面と同じ解決経路を使える。
- プランナー用画面と `SourceDataProvider` の挙動差を減らせる。

### 10.2 解決手順

1. 系統定義が必要カテゴリ一覧を持つ。
2. 各カテゴリについて `SourceDataProviderSettings.instance.TryGetMapping` で対応設定を引く。
3. Resolver で Addressable 先の ScriptableObject を取る。
4. 型に `SourceDataCollectionAttribute` が付いていれば、列挙対象配列を取得する。
5. Attribute が無ければ単体アセットとして扱う。
6. 編集対象は `SerializedObject` で描画する。

### 10.3 単体アセットと collection アセットの扱い

- 単体アセット
  - 例: `ScenarioSettingsAsset`
  - 右ペインに状態・Validation・関連リンクを出す。
- collection アセット
  - 例: `SkillNodeDataRepo`
  - 個別要素一覧、登録状況、ID 候補列挙を出す。

### 10.4 例外ケース

以下は `SourceDataProvider` 設定だけでは足りないので、系統ページ側で追加解決する。

- `ScenarioSettingsAsset` のような単体アセット
- 1 カテゴリ 1 リポジトリの形に乗らない補助アセット
- 既存カテゴリへまだ登録されていない新設データ

この場合でも、まずは `SourceDataProviderSettings` と Attribute の拡張で吸収できないかを先に検討する。

## 11. 画面レイアウト案

3 ペイン構成を推奨する。

- 左: 系統選択
- 中央: 編集
- 右: プレビュー / Validation / 参照情報

### 左ペイン

- 系統ボタン一覧
- 未設定カテゴリ数バッジ
- エラー件数表示

### 中央ペイン

- 系統タイトル
- 対象カテゴリとリポジトリの状態表示
- 対象要素一覧
- 選択中要素の編集フォーム

### 右ペイン

- 構造プレビュー
- 登録先リポジトリ情報
- `Ping`
- Validation 結果
- 関連カテゴリ一覧

## 12. 系統別の初回対応候補

### 11.1 SkillTree 系統

対象:

- `SkillNodeDataRepo`
- `SkillNodeBindRepo`
- `SkillNodePhaseBindDataRepo`

提供したいもの:

- ノード一覧
- 接続整合性チェック
- ノード接続プレビュー

### 11.2 StageSelect 系統

対象:

- `StageTreeAsset`
- `EnemyMissionKeyAsset`
- `EnemyWaveDefinitionAsset`

提供したいもの:

- ステージノード一覧
- 初期解放状態確認
- 分岐プレビュー

### 11.3 Scenario 系統

対象:

- `BackgroundCatalogAsset`
- `PortraitCatalogAsset`
- `AnimationCatalogAsset`
- `ScenarioSettingsAsset`

提供したいもの:

- カタログ要素一覧
- ID / 参照漏れチェック
- 再生設定の簡易プレビュー

## 13. ビジュアルシミュレーション方針

### 12.1 基本方針

初回は「実行シミュレーター」ではなく「編集内容の視覚化」を優先する。

### 12.2 系統ごとの例

#### SkillTree

- ノード接続の簡易グラフ
- フェーズごとの到達範囲表示

#### StageSelect

- ノード間の接続表示
- バトル / シナリオの色分け

#### Scenario

- 背景、立ち絵、アニメーションの登録状況一覧
- テキスト速度設定のパラメータプレビュー

### 12.3 実装方式

初期は IMGUI ベースで十分とする。

理由:

- 既存 EditorWindow 群が IMGUI ベースで統一されている。
- `SerializedObject` 描画と相性が良い。
- `SourceDataProvider` の既存ツール群とも実装感を揃えやすい。

ノード系プレビューだけ、必要になった段階で別描画コンポーネントへ切り出す。

## 14. Validation 方針

### 14.1 共通

- カテゴリ未登録
- Addressable キー未解決
- collection Attribute 未設定
- collection プロパティパス不正
- null 要素混入

### 14.2 SourceDataProvider 拡張用

- Settings 上のカテゴリ名と Attribute 上のカテゴリ名不一致
- collection Attribute が付いているのに配列 / List が存在しない
- 単体アセットなのに collection 前提で登録されている
- `DataID` 候補収集対象が 0 件

### 14.3 SkillTree

- ノード ID 重複
- Bind 先欠落
- フェーズ参照欠落

### 14.4 StageSelect

- 接続先欠落
- ルートノード不在
- シナリオ / バトル遷移情報不足

### 14.5 Scenario

- カタログ ID 重複
- 設定値範囲不正
- 参照先欠落

## 15. 推奨ディレクトリ構成

- `Assets/Editor/Scripts/PlannerMasterData/Window`
- `Assets/Editor/Scripts/PlannerMasterData/Registry`
- `Assets/Editor/Scripts/PlannerMasterData/Pages`
- `Assets/Editor/Scripts/PlannerMasterData/Preview`
- `Assets/Editor/Scripts/PlannerMasterData/Validation`

`SourceDataProvider` 本体は既存の

- `Assets/Editor/Scripts/SourceDataProvider`

に残し、責務を混ぜない。

追加候補:

- `Assets/Scripts/Runtime/0.Utility/Identity`
  - Attribute を Runtime 側に置く場合
- `Assets/Editor/Scripts/SourceDataProvider/Metadata`
  - Settings と Resolver の汎化補助

## 16. 実装フェーズ

### フェーズ0: SourceDataProvider の一般化

- `RepositoryMapping` 中心設計から、ScriptableObject 全般を扱える設定モデルへ移行方針を作る。
- Resolver を repository 前提から ScriptableObject 前提へ一般化する。
- `SourceDataCollectionAttribute` を追加する。
- collection 系 ScriptableObject から個別データを列挙できるようにする。
- 既存 `DataIDPropertyDrawer` の候補収集経路を新 Resolver へ寄せる。
- 既存 `RepositoryAddressSelectorDrawer` の名称と表示文言を、必要なら汎化する。

### フェーズ1: 互換レイヤ整備

- 既存 `RepositoryMapping` を読みつつ、新モデルへ橋渡しする互換層を作る。
- `SourceDataRegistrationHeader` を単体アセット / collection アセット両対応へ広げる。
- 既存カテゴリ設定が壊れないことを確認する。

### フェーズ2: Window の土台

- `PlannerMasterDataWindow` 作成
- 系統切り替え UI
- `SourceDataProviderSettings` からカテゴリ状態を読む処理
- ScriptableObject 解決結果の一覧表示

### フェーズ3: 主要系統の編集対応

- `SkillTreePage`
- `StageSelectPage`
- `ScenarioPage`
- `SerializedObject` ベースの編集

### フェーズ4: Validation とプレビュー

- 系統別 Validation
- 右ペインへの表示
- ノード系簡易プレビュー

### フェーズ5: 導線統合

- 個別 Inspector から Planner Window を開く導線
- `SourceDataProvider` Settings へのショートカット
- ScriptableObject `Ping`、個別アセット選択、フィルタ

## 17. リスクと対策

### リスク1

`SourceDataProvider` のカテゴリ粒度と、プランナーの編集したい系統粒度が一致しない。

対策:

- Planner 側は複数カテゴリ束ね前提で設計する。

### リスク2

カテゴリ未登録や collection メタデータ不備が残っていると、統合画面が壊れやすい。

対策:

- Window 起動時に設定不備を先に列挙し、編集 UI より先に警告する。

### リスク3

既存 `RepositoryMapping` ベースのツールと、新しい一般化モデルの二重管理期間が発生する。

対策:

- 互換レイヤを挟み、Settings の即時破壊的変更を避ける。

### リスク4

Attribute と Settings の両方でカテゴリや列挙情報を持つため、不整合が起きる可能性がある。

対策:

- Validation で差分検出を行う。
- 正本を Settings、型宣言を Attribute と役割分担する。

## 18. 最終提案

現状に合わせた最も自然な進め方は、**既存の `SourceDataProvider` を「Repository 専用ツール」から「Addressables でロードする全 ScriptableObject のメタデータ基盤」へ拡張し、その上にプランナー向け統合 EditorWindow を実装すること**である。

この前提なら、既にある以下をそのまま再利用できる。

- `ProjectSettings` でのカテゴリ管理
- Addressable リポジトリ解決
- `RepositoryAddressSelector`
- `DataID` 入力補助
- 個別アセットの登録 / 解除

そのうえで今回新たに必要なのは、次の 4 点である。

- SourceDataProvider の一般化
- Repository / collection 系向け Attribute 導入
- 系統単位で切り替える上位 UI
- 複数カテゴリを束ねる編集ページ
- 構造を見せるプレビュー / Validation

実装順としては、まず SourceDataProvider の一般化と互換レイヤを整備し、その後 `SkillTree` を最初の実運用系統として Planner Window に載せ、次に `StageSelect`、`Scenario` の順で広げるのが安全である。
