# ① セーブデータ読み込みフロー（初回アクセス時）

- page_id: 3bf7c2c6-cc02-81da-bb64-e5bfd9ab5ae5
- Notion パス: Symphony Kill Chord / システム概要 / セーブ / ① セーブデータ読み込みフロー（初回アクセス時）
- スナップショットの最終更新: 2026-08-17T13:20:28.119Z
- 書き込み許可: 可
- 処置: 本文置き換え

## 判定

- 信頼性: 一部古い — ローダーの参加者が `PersistentFileSaveDataLoaderStrategy` のままだが、実行時は `SaveDataConfig.asset` が指す `JsonUtilitySaveDataLoaderStrategy`（SymphonyFrameWork）が使われる。初回アクセスは常駐シーンの `SavedataSystemInitializer`（Order 10）であることも書かれていない。根拠: `Assets/Resources/SymphonyFrameWork/SaveDataConfig.asset:15-22`、`Assets/Scripts/Runtime/6.Composition/Persistent/Savedata/SavedataSystemInitializer.cs:25-37`
- 整合性: 親ページ「セーブ」（39e7c2c6-cc02-80c6-a4e3-e064efa25c2c）の原稿と揃えた
- 変更点:
  - 冒頭に説明文を追加（初回アクセスは常駐シーン起動時、以降はキャッシュ）
  - シーケンス図: 旧: 呼び出し元「Title等」 → 新: `SavedataSystemInitializer`（以降の呼び出し元はキャッシュを受け取る）
  - シーケンス図: 旧: `PersistentFileSaveDataLoaderStrategy` / `{persistentDataPath}のJSON` → 新: `SaveDataConfig` で指定されたローダー（現在は `JsonUtilitySaveDataLoaderStrategy`）。保存先ファイルは【要確認】
  - シーケンス図: 旧ID移行と保存を追加
- 織り込んだ反映項目: OG-014
- 出典: 実装（上記ファイル）、commit c8f1cea47（30_ OG-014 の記載）
- 要確認: 実行時の保存先ファイル（フレームワークのソースがリポジトリに無い）→ 要実機確認。どちらのローダーを正とするか → プログラム担当に確認

## 適用する本文

セーブデータは常駐シーンの`SavedataSystemInitializer`（Order 10）が起動時に1回だけ読み込み、以降の呼び出し元（Title・各機能モジュール）は`SaveStore`のキャッシュを受け取る。読み書きに使うローダーは`Assets/Resources/SymphonyFrameWork/SaveDataConfig.asset`で指定し、現在はSymphonyFrameWorkの`JsonUtilitySaveDataLoaderStrategy`である。
```Mermaid
sequenceDiagram
    autonumber
    participant Init as SavedataSystemInitializer（Order 10）
    participant Store as SaveStore
    participant Loader as ローダー（SaveDataConfigで指定。現在はJsonUtilitySaveDataLoaderStrategy）
    participant File as JSONファイル

    Init ->> Store: LoadAsync<SaveData>()
    alt 読み込み済み
        Store -->> Init: キャッシュされたSaveDataを返却
    else 未読み込み
        Store ->> Loader: 読み込み
        Loader ->> File: JSONを読み込み（保存先は【要確認】）
        alt ファイルが存在する
            File -->> Loader: JSON文字列
        else ファイルが存在しない
            Note over Loader: 既定値のまま（新規プレイヤー扱い）
        end
        Store ->> Store: キャッシュに格納
        Store -->> Init: SaveDataを返却
    end
    alt 旧形式の連番IDが含まれる
        Init ->> Init: LegacyDataIdMigration.TryMigrate(saveData)
        Init ->> Store: SaveAsync<SaveData>()
    end
    Note over Store: 以降の LoadAsync / IsLoaded・Get はキャッシュを返す
```
