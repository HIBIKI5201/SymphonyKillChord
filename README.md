https://hibiki5201.github.io/SymphonyKillChord/home/

## セットアップ

`Assets/AssetStoreTools` は private リポジトリ [SymphonyKillChord_AssetStoreTools](https://github.com/HIBIKI5201/SymphonyKillChord_AssetStoreTools) をサブモジュールとして参照している（閲覧権限が必要）。クローン時にサブモジュールも取得する。

```bash
git clone --recurse-submodules https://github.com/HIBIKI5201/SymphonyKillChord.git
# クローン済みの場合
git submodule update --init
```

GitHub Desktop でクローンした場合は、サブモジュールも自動で取得される。
