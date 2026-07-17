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

さらに、今後データ型が増えることを前提に、**新しいデータ型の追加時に SourceDataProvider 本体コードを毎回編集しない**ことを重要方針とする。
加えて、**SourceAsset と collection を分離して管理する**ことを構造上の前提とする。

### 5.2 守ること

- Editor 拡張は `Assets/Editor` 配下へ閉じ込める。
- Runtime の ScriptableObject 定義は再利用する。
- データ解決は可能な限り `SourceDataProviderSettings` と、拡張後の汎用 Resolver を使う。
- 個別アセットの Inspector 運用と共存させる。
- 巨大な 1 クラスではなく、系統ごとのページクラスへ分割する。
- ScriptableObject の一般メタデータと、Repository 系の列挙メタデータを分ける。
- 新規データ型の追加は、原則として `ProjectSettings` への登録と、必要なら Attribute 付与だけで成立させる。
- SourceAsset 登録画面と collection 登録画面は分離し、非 collection アセットに不要な repository 枠を表示しない。

### 5.3 拡張性要件

将来の新規データ型追加時に必要な作業は、次のどちらかに収まる状態を目標にする。

1. `SourceDataProviderSettings` の SourceAsset 一覧へ Addressable ScriptableObject 情報を追加する。
2. collection を持つ場合のみ、collection 一覧へ設定を追加する、または対象型に最小限のメタデータを付与する。

避けたい状態:

- データ型を 1 つ増やすたびに Resolver の `switch` や `if` を増やす。
- Planner Window の本体コードに型別分岐を追記する。
- SourceDataProviderSettings のコード側デフォルト一覧へ毎回手書き追加する。

例外として許容するのは、**そのデータ型を専用のビジュアルシミュレーションで表示したい場合の、ページクラスまたはプレビューパネルの追加**までとする。

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

現状の `RepositoryMapping` は、SourceAsset と collection の情報が 1 行に混在しており、概念として狭すぎる。
将来的には、以下のように **2段構成へ分離**する。

- `SourceAssetMapping`
  - Addressable でロードする ScriptableObject 全体の登録情報
- `SourceCollectionMapping`
  - どの SourceAsset のどの配列 / List を collection として扱うか
  - 必要ならカテゴリ名、表示名、個別要素型を持つ
- `SourceCategoryMapping`
  - `DataID` 用カテゴリと、参照元 collection の対応情報

重要なのは、**SourceAsset の登録だけで完結する単体アセット**と、**collection として個別要素を列挙するアセット**を設定上で区別できることにある。

初期段階では、既存 `RepositoryMapping` をすぐ全面置換せず、互換レイヤを挟んで段階移行する。

このとき、設定モデルは「コードで型ごとの対応表を書く」のではなく、**Settings によるデータ駆動定義**を基本とする。

### 7.4 Attribute 設計

collection を持つ ScriptableObject に対して、個別データ列挙のための Attribute を追加する。

候補:

- `SourceDataCollectionAttribute`
- `SourceDataRepositoryAttribute`

本計画書では、意味が広すぎない `SourceDataCollectionAttribute` を仮称とする。

この Attribute で宣言したい情報:

- この ScriptableObject が collection 候補を持つこと
- 列挙対象の配列 / List のプロパティパス
- 必要なら表示ラベル

重要なのは、この Attribute を **SourceDataProvider 本体の分岐を増やすためではなく、型自身が自分の列挙方法を宣言するため**に使うことである。

例:

- `SkillNodeDataRepo` は `SkillNodes` を列挙対象に持つ。
- `OutGameSkillRepository` は `_skillDataAssets` を列挙対象に持つ。
- `BackgroundCatalogAsset` は `_entries` を列挙対象に持つ。

### 7.5 SourceAsset と collection の分離方針

今後は次のように分ける。

#### SourceAsset

- Addressables でロードする ScriptableObject 本体
- すべての対象 ScriptableObject をここへ登録する
- 単体設定アセットもここへ含む

#### collection

- SourceAsset 内の「個別データを列挙する配列 / List」
- 0 件でもよい
- 1 SourceAsset に複数あってよい

この分離により、以下を解決できる。

- 1 つの SourceAsset に複数の repository 相当 collection があるケースへ対応できる。
- collection を持たない単体アセットに、不要な repository 用 UI を出さずに済む。
- Planner 側で「SourceAsset 単位表示」と「collection 単位表示」を分けやすい。

### 7.6 Category の扱い

Category は今後、collection 単位で扱う。

1. `SourceAssetMapping` は SourceAsset 自体の登録だけを持つ。
2. `SourceCollectionMapping` が collection とカテゴリの対応を持つ。
3. 必要なら `SourceDataCollectionAttribute` は collection 候補の宣言補助に使う。

両者がある理由:

- Settings はプロジェクト全体の登録台帳である。
- Attribute は「この型に collection 候補がある」ことを型側から示す補助である。

最終的には、カテゴリ名の正本は `SourceCollectionMapping` 側に置き、Attribute 側は「どのプロパティが collection 候補か」の補助情報として使う。

### 7.7 Resolver の一般化

`SourceDataProviderRepositoryResolver` は、名称と責務を一般化する。

候補:

- `SourceDataProviderAssetResolver`
- `SourceDataProviderObjectResolver`

役割:

- Addressable キーから任意の ScriptableObject を解決する。
- SourceAsset として解決したあと、設定済み collection 一覧を引く。
- collection 設定がなければ単体 ScriptableObject として扱う。
- `DataID` 候補収集も「repository 前提」ではなく「collection 設定前提」へ改める。

ここでは、型ごとのハードコード分岐を禁止し、**Reflection と Settings / Attribute の組み合わせで自己記述的に解決する**ことを原則とする。

### 7.8 Header UI の一般化

`SourceDataRegistrationHeader` も repository 固有 UI から一段一般化する。

- 単体 ScriptableObject の場合
  - SourceDataProvider 登録状態を表示する。
  - 対応 Addressable キーや系統を確認できる。
- collection を持つ ScriptableObject の場合
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
- SourceAsset ごとの collection 定義保持
- ScriptableObject 実体の解決
- collection 候補 Attribute の解釈
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
2. 各系統で必要な SourceAsset を `SourceAssetMapping` から引く。
3. Resolver で Addressable 先の ScriptableObject を取る。
4. 必要なら `SourceCollectionMapping` から collection 一覧を引く。
5. collection が無ければ単体アセットとして扱う。
6. 編集対象は `SerializedObject` で描画する。

この流れにより、新しいデータ型が追加されても、Resolver 側のコード変更は不要にする。

### 10.3 単体アセットと collection アセットの扱い

- 単体アセット
  - 例: `ScenarioSettingsAsset`
  - 右ペインに状態・Validation・関連リンクを出す。
- collection アセット
  - 例: `SkillNodeDataRepo`
  - 個別要素一覧、登録状況、ID 候補列挙を出す。

同じ SourceAsset に複数 collection がある場合は、Planner 側では collection 単位でタブまたはサブセクションを分ける。

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

### 12.1 SkillTree 系統

対象:

- `SkillNodeDataRepo`
- `SkillNodeBindRepo`
- `SkillNodePhaseBindDataRepo`

提供したいもの:

- ノード一覧
- 接続整合性チェック
- ノード接続プレビュー

### 12.2 StageSelect 系統

対象:

- `StageTreeAsset`
- `EnemyMissionKeyAsset`
- `EnemyWaveDefinitionAsset`

提供したいもの:

- ステージノード一覧
- 初期解放状態確認
- 分岐プレビュー

### 12.3 Scenario 系統

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

### 13.1 基本方針

初回は「実行シミュレーター」ではなく「編集内容の視覚化」を優先する。

### 13.2 系統ごとの例

#### SkillTree

- ノード接続の簡易グラフ
- フェーズごとの到達範囲表示

#### StageSelect

- ノード間の接続表示
- バトル / シナリオの色分け

#### Scenario

- 背景、立ち絵、アニメーションの登録状況一覧
- テキスト速度設定のパラメータプレビュー

### 13.3 実装方式

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
- collection 設定未登録
- collection プロパティパス不正
- null 要素混入

### 14.2 SourceDataProvider 拡張用

- SourceAsset は登録されているが collection 設定が欠けている
- collection Attribute が付いているのに配列 / List が存在しない
- 単体アセットなのに collection 設定が付いている
- `DataID` 候補収集対象が 0 件
- 新規追加型が Settings 登録だけで解決できず、コード側分岐を要求していないか

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

- `RepositoryMapping` 中心設計から、`SourceAssetMapping` と `SourceCollectionMapping` に分離する移行方針を作る。
- Resolver を repository 前提から ScriptableObject + collection 前提へ一般化する。
- `SourceDataCollectionAttribute` は collection 候補補助として導入する。
- collection 系 ScriptableObject から個別データを列挙できるようにする。
- 既存 `DataIDPropertyDrawer` の候補収集経路を新 Resolver へ寄せる。
- 既存 `RepositoryAddressSelectorDrawer` の名称と表示文言を、必要なら汎化する。
- 「新規データ型追加時にコード編集が不要であること」を受け入れ条件に含める。

### フェーズ1: 互換レイヤ整備

- 既存 `RepositoryMapping` を読みつつ、`SourceAssetMapping` / `SourceCollectionMapping` へ橋渡しする互換層を作る。
- `SourceDataRegistrationHeader` を単体アセット / collection アセット両対応へ広げる。
- 既存カテゴリ設定が壊れないことを確認する。
- 代表的な新規型追加手順を、Settings 追加だけで再現できることを検証する。

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

## 17. 新規データ型追加フロー

理想的な追加フローは以下とする。

### 17.1 単体 ScriptableObject の場合

1. Addressables に登録する。
2. `SourceAssetMapping` に対象 ScriptableObject 情報を追加する。
3. 必要なら Planner 側の既存系統へ紐付ける。

この場合、SourceDataProvider 本体コードの編集は不要とする。

### 17.2 collection 系 ScriptableObject の場合

1. Addressables に登録する。
2. `SourceAssetMapping` に対象 ScriptableObject 情報を追加する。
3. `SourceCollectionMapping` に、どの配列 / List を collection とみなすかを追加する。
4. 必要なら対象型へ `SourceDataCollectionAttribute` を付け、候補補助だけを行う。
5. 必要なら Planner 側の既存系統へ紐付ける。

この場合も、SourceDataProvider 本体コードの編集は不要とする。

### 17.3 専用画面が必要な場合

既存の汎用ページで十分なら追加コードは不要とする。

専用の可視化やシミュレーションが必要な場合のみ、

- 系統ページ
- プレビューパネル

の追加を許容する。

## 18. リスクと対策

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

SourceAsset 設定と collection 設定が分かれることで、設定間の参照不整合が起きる可能性がある。

対策:

- Validation で差分検出を行う。
- 正本を Settings、型宣言を Attribute と役割分担する。

### リスク5

「コード編集不要」を目指した結果、Settings 入力項目が増えすぎて運用が複雑になる可能性がある。

対策:

- Settings は SourceAsset 共通情報と collection 情報に分ける。
- collection 固有情報は Settings を正本にし、Attribute は候補補助に留める。
- Planner 側は汎用ページを先に作り、専用ページ追加を後回しにする。

## 19. 最終提案

現状に合わせた最も自然な進め方は、**既存の `SourceDataProvider` を「Repository 専用ツール」から「Addressables でロードする全 ScriptableObject のメタデータ基盤」へ拡張し、その内部を `SourceAsset` と `collection` の2層構造へ分離した上で、プランナー向け統合 EditorWindow を実装すること**である。

この前提なら、既にある以下をそのまま再利用できる。

- `ProjectSettings` でのカテゴリ管理
- Addressable リポジトリ解決
- `RepositoryAddressSelector`
- `DataID` 入力補助
- 個別アセットの登録 / 解除

そのうえで今回新たに必要なのは、次の 5 点である。

- SourceDataProvider の一般化
- SourceAsset / collection 分離モデルの導入
- Repository / collection 系向け Attribute 導入
- データ駆動での型追加を前提にした Settings / Resolver 設計
- 系統単位で切り替える上位 UI
- 複数カテゴリを束ねる編集ページ
- 構造を見せるプレビュー / Validation

最終的なあるべき姿は、「新しいデータ型が増えても、単体アセットなら SourceAsset 登録だけ、collection 系でも SourceAsset 登録 + collection 設定追加だけで統合管理画面に載る」構造である。

実装順としては、まず SourceDataProvider の一般化と互換レイヤを整備し、その後 `SkillTree` を最初の実運用系統として Planner Window に載せ、次に `StageSelect`、`Scenario` の順で広げるのが安全である。
