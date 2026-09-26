# ③ GameDataVariant切替でRelease/Demoのデータを出し分ける仕組み

- id: 3d67c2c6-cc02-81c5-bdd1-fad0d552e2d2
- path: Symphony Kill Chord / システム概要 / SourceDataProvider / ③ GameDataVariant切替でRelease/Demoのデータを出し分ける仕組み
- last_edited: 2026-09-09T03:35:11.409Z

```Mermaid
sequenceDiagram
    autonumber
    actor Engineer as エンジニア
    participant Menu as KillChord/Game Data Variant メニュー
    participant State as GameDataVariantEditorState
    participant BuildSettings as GameDataVariantBuildSettings
    participant Addr as Addressables Groups
    participant Player as PlayerSettings / EditorBuildSettings

    Engineer ->> Menu: Release または Demo を選択
    Menu ->> State: SetSelectedVariant(variant)
    State -->> State: EditorPrefsへユーザー単位で保存
    State ->> BuildSettings: Apply(variant)
    BuildSettings ->> Addr: GameData.Release / GameData.Demo / GameData.Shared をEnsureGroup
    BuildSettings -->> Addr: 選択中Variantのgroupだけ IncludeInBuild=true（Sharedは常にtrue）
    BuildSettings ->> Player: KILLCHORD_DEMO の Scripting Define Symbol を追加/削除
    BuildSettings ->> Player: Demo/DemoEnd.unity を EditorBuildSettings.scenes へ有効/無効登録
    Note over BuildSettings: IPreprocessBuildWithReport(GameDataVariantBuildPreprocessor)が<br/>ビルド直前にも同じApply()を強制実行し、選択忘れによる不整合を防ぐ
```
> Planner Master Data Window自体はどちらのVariantでも両方のGroupの中身を等しく表示する（`SourceDataProviderRepositoryResolver`はGroupのIncludeInBuildを見ない）。データそのものをRelease/Demo間で出し分けたい場合は、対象ScriptableObjectの所属Addressables GroupをGameData.Release / GameData.Demo / GameData.Sharedのいずれかへ振り分けることで、ビルド成果物の含有可否を制御する。