# ランタイム境界

ステータス: partial（部分完了）。

リポジトリはRuntimeをUtility、Domain、Application、Adaptor、View、Infrastructure、Compositionの7層へ分けている。module間の協調はAdaptor契約を介し、最終的なobject配線はCompositionで行う想定である。Composition initializerは `Init`、`ResourceLoadAsync`、`Build`、`Ready` の順で処理し、終了時は逆順でshutdownする。

## 境界の観測結果

- assembly definitionによりlayer境界が明示され、強いarchitecture controlとして機能する。
- `ServiceLocator` の利用はCompositionへ集中しており、リポジトリ文書のcomposition規約と一致する。
- Adaptorの `BattleSortieSelectionService` もglobal selection stateを検索・登録する（`Assets/Scripts/Runtime/3.Adaptor/OutGame/StageSelect/BattleSortieSelectionService.cs:44`）。これはglobal配線をCompositionが所有する原則の例外である。
- Anatomia結果ではscenario／screen-flow codeの手動境界面が最大である。これは変更riskのsignalであり、欠陥の証明ではない。
- 静的call graphには1,954件の未解決呼出とoverload／同名解決の曖昧さがある。cycleやfan-outは調査候補として扱う。
