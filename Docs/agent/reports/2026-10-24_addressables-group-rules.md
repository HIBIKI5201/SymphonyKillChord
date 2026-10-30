# Addressables グループ規約（案）

Issue #2083。2026-10-24 時点の `Assets/AddressableAssetsData/AssetGroups/` の構成から作った案。確定後に Notion へ書き、このファイルは調査記録として残す。

## 1. グループの一覧

グループはこの表のものだけを使う。新設はエンジニアが行い、この表を更新する。

| グループ | 入れるもの | ファイルの場所 | ビルドに入るか |
| --- | --- | --- | --- |
| `GameData.Shared` | 製品版と体験版で共通のマスターデータ・設定アセット | `Assets/Level/Data/Master/`（演出用は `Assets/Arts/` も可） | 常に入る |
| `GameData.Release` | 製品版だけのデータ | `Assets/Level/Data/Master/` | 製品版のビルドだけ |
| `GameData.Demo` | 体験版だけのデータ・Prefab | `Assets/Level/Data/Demo/`、`Assets/Level/Prefabs/Demo/` | 体験版のビルドだけ |
| `Localization-*` | Localization パッケージが作るテーブル・ロケール | `Assets/Localization/` | 常に入る |
| `Default Local Group` | 何も入れない（Addressables の既定グループとして残す） | — | — |

- `GameData.*` の「ビルドに入るか」は `GameDataVariantBuildSettings` が Build Profile から切り替える。手で `Include In Build` を変えない。
- `Localization-*` はパッケージが管理する（エントリが読み取り専用）。グループ名・ラベル（`Locale`・`Locale-ja`）を手で変えない。テーブルは Localization の画面から追加する。
- `Assets/Level/Data/Develop/`・`Research/` のアセットは登録しない。本番で使うなら先に本番フォルダへ移す。

## 2. グループ名

- ゲームのデータは `<分類>.<版>` にする。版は `Shared`・`Release`・`Demo` のどれか。今の分類は `GameData` だけ。
- 分類を増やすとき（例: 音声やシーンを別に分けるとき）も同じ形にする（`Audio.Shared` など）。
- `Stream_*` のような接頭辞や、作業者名・日付を含む名前は使わない。

## 3. アドレス

- 拡張子を除いたファイル名にする（例: `StageTreeAsset.asset` → `StageTreeAsset`）。`AddressableKeyValidator` がビルド前に次を確かめ、違えばビルドを止める。
  - ファイル名に含まれる文字列であること
  - `/` や `\` を含まないこと（パスをアドレスにしない）
  - ビルドに入るグループの中で重複しないこと
- 版ごとに内容が違うデータは、同じアドレスで `GameData.Release` と `GameData.Demo` の両方に置く。同じアドレスを `GameData.Shared` に残さない。
- コードからはアドレスの定数（`nameof(型名)` など）で読み込む。

## 4. ラベル・設定

- `GameData.*` ではラベルを使わない。
- 全グループをストア同梱（ローカルのビルド・読み込みパス、Remote Catalog なし）にする。`AddressableStoreBuildValidator` が確かめる。
- バンドルの分け方は既定のまま（Pack Together）。

## 5. 消すとき

- アドレスを消したり変えたりしたら、`ProjectSettings/SourceDataProviderSettings.asset` の `_dataAssetMappings` から古いキーも消す。この一覧は Addressables から自動で足されるが、自動では消えない。

## 6. 今回の PR で直したもの

| 対象 | 内容 |
| --- | --- |
| `SourceDataProviderSettings.asset` | 存在しないアドレスを指す 8 件を削除した: `Assets/Level/Data/Master/OutGame/StageSelect/Stream_StageTreeAsset.asset`（`Stream_*` の残骸）、`Assets/Localization/Tables/UICommon Shared Data.asset`、`MobileStickFlickInput`、`TestSkillData1`、`TestSkillData2`、`TutorialPopupImages Shared Data`、`TutorialPopupImages_en`、`TutorialPopupImages_ja` |
| `Localization-Assets-Shared` | `TutorialPopups Shared Data` のアドレスがパス（`Assets/Localization/Tables/TutorialPopups Shared Data.asset`）だったので、ほかのテーブルと同じくファイル名にした。パスのままだと `AddressableKeyValidator` でビルドが止まる |

`Stream_*` のグループ・アセット・コードは、上の 1 件のほかにはリポジトリに無かった。

## 7. 規約から外れていて、今回は直していないもの

- `SkillInputProgressViewConfigAsset`: どのグループにも無いが、`PlannerMasterDataEditorSettings.cs` の既定一覧にも書かれているので `_dataAssetMappings` に残した。
- `DemoEndSequenceConfig`: ファイルは `Assets/Level/Data/Demo/` にあるが `GameData.Shared` に入っている。体験版の終了演出のシーンからシリアライズで参照されていて、アドレスでは読み込まれていない。
- `GameData.Shared` にデバッグ・テスト用のアセットがある: `OwnedSkillRepositoryDebug`、`SkillBuildRepositoryDebug`、`SkillTreeTestInputData`。製品版にも入る。
- 仕様ミラーの「敵・ボスのステータス」に「ボスのデータは `Develop/Boss/` にあり `GameData.Shared` に登録」とあるが、今は `Assets/Level/Data/Master/InGame/Enemy/Boss/` に移っている（記述が古い）。

## 8. 未確認

- Unity でのコンパイル・Addressables のビルド（製品版・体験版）は未実施。
