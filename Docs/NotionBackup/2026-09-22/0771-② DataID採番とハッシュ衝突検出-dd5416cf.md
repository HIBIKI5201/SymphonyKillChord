# ② DataID採番とハッシュ衝突検出

- id: 3d67c2c6-cc02-8160-ad80-da71dd5416cf
- path: Symphony Kill Chord / システム概要 / SourceDataProvider / ② DataID採番とハッシュ衝突検出
- last_edited: 2026-09-09T03:35:09.458Z

```Mermaid
sequenceDiagram
    autonumber
    actor Planner as プランナー
    participant Drawer as DataIDPropertyDrawer
    participant Hasher as DataIDHasher
    participant Resolver as SourceDataProviderRepositoryResolver
    participant Detector as DataIDCollisionDetector

    Planner ->> Drawer: 文字列IDを入力（authoringフィールド）
    Drawer ->> Hasher: Compute(collectionKey, id)
    Hasher -->> Drawer: 数値ID（Animator.StringToHash）
    Drawer -->> Drawer: _hashIdへ焼き込み
    Drawer ->> Resolver: GetOptions(collectionKey)（同一カテゴリの登録済みID一覧）
    Resolver -->> Drawer: IReadOnlyList<SourceDataIDOption>
    Drawer ->> Detector: FindWarning(id, hashId, options)
    Detector -->> Drawer: 重複／ハッシュ衝突があれば警告文
    Drawer -->> Planner: HelpBoxへ警告表示
```
> 一括再計算が必要な場合（大量リネーム後など）は`DataIDRebuildMenu`（`Tools/Source Data Provider/Rebuild All DataID Hashes`）で全ScriptableObject/Prefab/Sceneを走査し、`DataIDHasher.Compute`との不一致を一括修正する。走査中に検出したハッシュ衝突はDialogではなくConsoleへエラーログ出力される点に注意する。