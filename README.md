https://hibiki5201.github.io/SymphonyKillChord/home/

## セットアップ

1. Unity Hub で `ProjectSettings/ProjectVersion.txt` のバージョン（Android Build Support 付き）を入れる。
2. リポジトリをクローンする。**フォルダ名は `SymphonyKillChord` のままにし、パスに日本語や空白を含めない。**（Unity と IDE はフォルダ名からソリューションを作るので、名前が違うと別名の `.slnx` ができる）
3. リポジトリ直下の `Setup.bat` をダブルクリックする（PowerShell からなら `./PowerShell/ProjectSetup/Setup-Project.ps1`）。
   - サブモジュールの取得と git の設定（`submodule.recurse` / `core.longpaths`）を行い、Unity のバージョンと Android Build Support を確かめる。
   - 直せない項目は、直し方を表示する。確認だけをするときは `Setup.bat -Check`。
4. Unity Hub からプロジェクトを開く。セットアップが済んでいないと、起動時に「プロジェクトのセットアップ」ウィンドウが出る。

サブモジュールは次の2つで、どちらも private リポジトリである（閲覧権限が必要。無ければリードに GitHub の招待を頼む）。

| パス | リポジトリ | 中身 |
| --- | --- | --- |
| `Assets/AssetStoreTools` | [SymphonyKillChord_AssetStoreTools](https://github.com/HIBIKI5201/SymphonyKillChord_AssetStoreTools) | 外部アセットと NuGet パッケージ。無いとコンパイルエラーになる |
| `Docs/NotionSpecifications` | [SymphonyKillChord_Specifications](https://github.com/HIBIKI5201/SymphonyKillChord_Specifications) | Notion 仕様書の写し。CI が3時間ごとに更新する |

セットアップの手順を増やしたら、`PowerShell/ProjectSetup/project-setup.json` の `SetupVersion` を上げる。既存のメンバーも、次に Unity を開いたときに再実行を促される。
