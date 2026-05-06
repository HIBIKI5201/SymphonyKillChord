# シナリオシステム改修レポート

## 概要
以下の機能を、CSVイベント変換を含めて実装しました。

- レイヤーシステム
- 立ち絵表示（ADV形式）
- 立ち絵位置調整
- 立ち絵更新（同スロットで表情・差分の差し替え）

## 追加したイベント種別

- `Portrait` イベント
  - Domain: `PortraitEvent`, `PortraitSlot`, `PortraitDefinition`
  - スロット指定、立ち絵ID、座標、スケール、表示/非表示に対応
- `Layer` イベント
  - Domain: `LayerEvent`, `LayerTarget`
  - UI要素の前後順（Sibling Index）および Canvas の SortingOrder 変更に対応

## CSV変換対応

`ScenarioRepository` で以下 `Type` を新規対応しました。

- `Portrait`
- `Layer`

### 追加CSV列

- `PortraitSlot` (`Left|Center|Right`)
- `PortraitId`
- `PortraitPosX`
- `PortraitPosY`
- `PortraitScale`
- `PortraitVisible` (`true/false` または `0/1`)
- `LayerTarget` (`Background|PortraitLeft|PortraitCenter|PortraitRight|Text|Canvas`)
- `LayerOrder` (int)

既存のCSVパーサで、上記列を新規Domainイベントへ変換するように実装しています。

## ランタイム配線の変更

- `ScenarioCom` のDI配線に以下を追加
  - `PortraitRepository`
  - `PortraitPresenter`
  - `LayerPresenter`
  - `PortraitEventHandler`
  - `LayerEventHandler`
- `ScenarioPresenterFacade` / `IOutputPort` に立ち絵・レイヤー出力を追加
- `ViewModel` に立ち絵・レイヤー通知を追加
- `ScenarioView` を拡張
  - 立ち絵スロット (`PortraitLeft`, `PortraitCenter`, `PortraitRight`) の生成/保持
  - 立ち絵の画像更新、位置、スケール、表示状態の反映
  - 対象UI要素/Canvasへのレイヤー順反映

## カタログ/リポジトリ追加

- `PortraitCatalogAsset`
- `PortraitRepository`
- `IPortraitRepository`

## サンプルCSV更新

更新対象:

- `Assets/StreamingAssets/Scenario/ScenarioTest.csv`
- `Assets/StreamingAssets/ScenarioAuthoring/ScenarioTest.events.csv`

`Layer` / `Portrait` のサンプルイベントを追加済みです。

## 立ち絵を表示するまでの手順

1. `PortraitCatalogAsset` を作成する  
   - Unityメニューから  
     `Create > KillChord > Runtime > Scenario > Portrait Catalog`  
   - 任意の場所に `PortraitCatalogAsset` を作成

2. `PortraitCatalogAsset` に立ち絵を登録する  
   - `Entries` に要素を追加  
   - `Id`: シナリオCSVで参照する立ち絵ID（例: `hero_default`）  
   - `AssetKey`: 空でも可（空の場合は `Asset.name` を使用）  
   - `Asset`: 表示したい `Sprite`

3. `ScenarioCom` にカタログを割り当てる  
   - シーン上の `ScenarioCom` を選択  
   - `_portraitCatalog` フィールドへ手順1の `PortraitCatalogAsset` を設定

4. シナリオCSVに `Portrait` イベントを記述する  
   - 例:
   - `Type=Portrait`
   - `PortraitSlot=Left|Center|Right`
   - `PortraitId=hero_default`（手順2の `Id` と一致させる）
   - `PortraitPosX=-460`, `PortraitPosY=-120`
   - `PortraitScale=1`
   - `PortraitVisible=true`

5. 必要に応じて `Layer` イベントで前後関係を調整する  
   - 例:
   - `Type=Layer`
   - `LayerTarget=PortraitLeft`
   - `LayerOrder=2`

6. シナリオを再生して確認する  
   - `ScenarioSettingsAsset` の `DefaultScenarioId` が対象CSV名と一致していることを確認  
   - 再生時に `Portrait` イベントが処理され、指定スロットに立ち絵が表示される

## ビルド確認

実行コマンド:

- `dotnet build SymphonyKillChord.slnx`

結果:

- ビルド成功（`0 errors`）
- 既存の無関係な warning は継続
