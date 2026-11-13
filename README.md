https://hibiki5201.github.io/SymphonyKillChord/home/

## セットアップ

1. Unity Hub で `ProjectSettings/ProjectVersion.txt` のバージョン（Android Build Support 付き）を入れる。
2. リポジトリをクローンする。**フォルダ名は `SymphonyKillChord` のままにし、パスに日本語や空白を含めない。**（Unity と IDE はフォルダ名からソリューションを作るので、名前が違うと別名の `.slnx` ができる）
3. リポジトリ直下のセットアップをダブルクリックする。
   - Windows: `Setup.bat`（PowerShell からなら `./PowerShell/ProjectSetup/Setup-Project.ps1`）
   - macOS: `Setup.command`（Finder でダブルクリックするとターミナルで開く。ターミナルからなら `./Setup.command`）。Linux はターミナルから `./Setup.command`
   - サブモジュールの取得と git の設定（`submodule.recurse` / `core.longpaths`）を行い、Unity のバージョンと Android Build Support を確かめる。
   - Unity CLI（`unity` コマンド）が無ければ、公式のスクリプトで入れる。AI エージェントが Unity を操作するのに使う（エディタの操作はプロジェクトの `com.unity.pipeline` 経由）。
   - 直せない項目は、直し方を表示する。確認だけをするときは `Setup.bat -Check`（macOS / Linux は `./Setup.command -Check`）。
4. Unity Hub からプロジェクトを開く。セットアップが済んでいないと、起動時に「プロジェクトのセットアップ」ウィンドウが出る。

サブモジュールは次の3つで、どれも private リポジトリである（閲覧権限が必要。無ければリードに GitHub の招待を頼む）。必須は `Assets/AssetStoreTools` だけで、ほかは取得できなくてもセットアップは警告に留めて完了する（必須の一覧は `PowerShell/ProjectSetup/project-setup.json` の `RequiredSubmodules`）。

| パス | リポジトリ | 中身 | 必須 |
| --- | --- | --- | --- |
| `Assets/AssetStoreTools` | [SymphonyKillChord_AssetStoreTools](https://github.com/HIBIKI5201/SymphonyKillChord_AssetStoreTools) | 外部アセットと NuGet パッケージ。無いとコンパイルエラーになる | 必須 |
| `Docs/NotionSpecifications` | [SymphonyKillChord_Specifications](https://github.com/HIBIKI5201/SymphonyKillChord_Specifications) | Notion のプロジェクト ドキュメントの写し。CI が3時間ごとに更新する。仕様書ビューア・仕様検索で使う | 任意（警告） |
| `Sound/CRIAtomCraft` | [SymphonyKillChord_CriAtomCraft](https://github.com/HIBIKI5201/SymphonyKillChord_CriAtomCraft) | CRI Atom Craft のサウンドの元データ。サウンドを編集する人だけ使う | 任意（警告） |

セットアップの手順を増やしたら、`PowerShell/ProjectSetup/project-setup.json` の `SetupVersion` を上げる。既存のメンバーも、次に Unity を開いたときに再実行を促される。

## スクリプト管理ツール（Windows）

ルートの `ScriptLauncher.exe` をダブルクリックすると、初期化・ブランチの掃除・Android 端末へのインストール・ビルドランナーなどのスクリプトを一覧から動かせる。ツールは通常の権限で動き、管理者が要るスクリプトだけ実行のときに UAC の確認が出る。新しい `.bat` / `.command` を足したら `ScriptLauncher/launcher-scripts.json` にも登録する（詳しくは [ScriptLauncher/README.md](ScriptLauncher/README.md)）。

## Gemini CLI を使う

リポジトリのルートで `gemini` を実行する。Windows では `PowerShell/StartGemini.ps1` を実行すると、ルートへ移動してから起動する。macOS / Linux はターミナルでルートへ移動して `gemini` を実行する。
