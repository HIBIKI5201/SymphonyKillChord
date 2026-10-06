# アセットパイプライン設計 設計レビュー（2026-10-01, Codex）

## 要約

全体評価は **C：実行時の参照・梱包は成立しているが、納品から採用版までの追跡と再現性が不足**。
Driveは提出品ごとのSource / Exportへ整理されたが、Unity側には旧名・日付付き階層・制作原本が残る。
Artsの種類別36ファイル標本では、接頭辞と禁止文字等の形式条件を満たすものは5件、13.9%だった。
AssetStoreToolsはサブモジュールとDrive ZIPの二経路を持ち、CIは更新日時が最新のZIPを採用する。
ローカルAssetsは24.086 GiB、うちAssetStoreToolsは23.109 GiB。Importの既定Presetは未登録である。
今週は採用版と配布版の固定、Exportだけの取り込みを優先し、改名とAndroid最適化は対象を絞って進める。

## 現状の構造

### 調査条件と根拠の限界

- 見出しの日付は依頼のレビュー基準日。実ファイルの調査日は2026-10-06である。
- Driveは10月1日の保存スナップショットと#2141保存記録を使用し、現物へ接続していない。
- コード・meta・Addressables設定・CI・ローカルファイル容量を読み取りで確認した。
- 指定された最新Notion本文ディレクトリは、このチェックアウトには存在しない。
- `Docs/NotionSpecifications/`にはREADMEとサブモジュールの管理ファイルのみがある。
- 指定された別作業ルートにも同名のNotion本文ディレクトリは存在しなかった。
- よって各トップページ、仕様書ガイド、仕様概要、システム概要、ワークフローの直接読解は未実施。
- Notionに関する記述は完了済みDrive・仕様書レビューの引用元情報による二次確認と明記する。
- #2084/#2111/#1996/#1985/#2137の`gh issue view`は、接続先プロキシで失敗した。
- 関連Issueの現在の本文・コメント・開閉状態・完了状況は未確認である。
- Issueの方針は#2141保存記録、命名提案、完了済みレビュー、保存Issue要約から確認した。
- Git操作、Unity起動、Import、テスト、ビルド、Drive更新は行っていない。
- 出力は依頼の指定先1ファイルのみ。一般の出力配置・コミット規則より今回の明示指示を優先した。
- 評価尺度：A＝継続運用検証済み、B＝軽微な不足、C＝重要な不足あり、D＝重大な継続障害、E＝未成立。
- 推測と静的に確認した事実を分け、未確認を「未実装」「障害発生」と読み替えない。

### 根拠ファイルの略称

| 略称 | リポジトリ相対パス |
|---|---|
| Snapshot | `.design-review/drive_snapshot.md` |
| Drive記録 | `.design-review/drive_issue_2141.md` |
| 命名提案 | `Docs/アセット名リスト_命名規則提案.md` |
| Sync | `Assets/Editor/Scripts/AssetManagement/DriveImportSync.cs` |
| Manifest | `Assets/Editor/Scripts/AssetManagement/DriveImportManifest.cs` |
| Sync設定 | `ProjectSettings/KillChord/DriveImportSettings` |
| CSV | `Assets/Editor/Scripts/CsvImporter/CsvImporter.cs` |
| Upload | `Assets/Editor/Scripts/AssetStorePackagePipeline/DriveUploadPackageStrategy.cs` |
| Single設定 | `Assets/Editor/SymphonyFrameWork/Configs/AssetStoreToolsPackagePipeline_Single.asset` |
| CI | `.github/workflows/BuildAndRelease.yml` |
| Key検査 | `Assets/Editor/Scripts/Addressables/AddressableKeyValidator.cs` |
| Store検査 | `Assets/Editor/Scripts/Addressables/AddressableStoreBuildValidator.cs` |
| Loader | `Assets/Scripts/Runtime/5.InfraStructure/Addressables/ScriptableObjectAddressableLoader.cs` |
| Scenario読込 | `Assets/Scripts/Runtime/5.InfraStructure/OutGame/Scenario/ScenarioRepository.cs` |
| N/ | `Docs/NotionSpecifications/Symphony Kill Chord/`。今回は直接読めていない |

他観点の既存レビューは、指定された別作業ルートの`Docs/agent/reports/`を読んだ。
以下では`2026-10-01_design-review-google-drive.md`を「Driveレビュー」、同`notion-spec.md`を「仕様書レビュー」と呼ぶ。
Driveの所有権・共有権限・認証の一般的な問題はDriveレビューD01〜D07、D18へ委ねる。

### 全体経路

```text
制作担当のDCC・音声編集・シナリオ編集
├─ Drive 03_Art/<種類>/<提出品>/Source          修正原本
│                     └─ Export              採用候補のfbx/png/prefab/meta等
│                          ├─ 手動取得・配置 ────────────────┐
│                          └─ DriveImportSync               │
│                             個人の設定先へ階層ごと取得    │
│                             ※Sourceの既定除外なし         ▼
│                       Assets/Arts/{Models,Images,Animation,VFX,...}
│                       Assets/AssetStoreTools/_drive/      旧分類の複製も存在
│                          └─ Unity Import + .meta
│                               ├─ Scene / Prefabの直接GUID参照
│                               └─ Assets/Level/DataのConfig・Catalog
│                                     └─ Addressables GameData.Shared
│                                          └─ Initializer → Loader → View
├─ Drive 04_Sound/{BGM,SE,Voice,CRI}
│    └─ CRI編集・書き出し・Unity配置（自動化の接続は未確認）
│         ├─ Arts/Audio/CriData/Settings                     Editor設定
│         └─ StreamingAssets/*.acf, *.acb                   実行データ
│              └─ CRI設定・Cue → MusicPlayer / SoundEffectSource / VoiceSource
└─ Drive 02_Scenario/Csv
     └─ CSV Importer（手動実行、直下CSV/Sheets）
          └─ StreamingAssets/ScenarioAuthoring/*.csv
               └─ ScenarioRepository → ScenarioDefinition → シナリオ再生

外部素材・SDK・Drive素材の配布
Assets/AssetStoreTools（サブモジュール）
  └─ Single / Combine + UsedDependencies + ZIP
       └─ SingleではDriveへUpload → 05_Programming/AssetStoreTools/SinglePackages
            └─ CI：最新ZIP → unitypackage → pathname/metaを直接配置 → Unity Build
```

Addressablesは全制作物が必ず通る単一路ではない。
CRIデータとシナリオCSVはStreamingAssetsを通り、画像・モデル・VFXには直接参照もある。
AddressablesのCatalogやConfigが参照する素材は、独立登録されなくても依存として梱包され得る。
そのため「Artsファイル数／Addressables登録数」を採用率として算出してはいけない。

### 種類ごとの接続と手作業

| 制作物 | Driveの位置 | Unity側の確認先 | 実行参照 | 手作業・断絶 |
|---|---|---|---|---|
| 2D | `03_Art/2D/<分類>/<提出品>/Export` | `Arts/Images/Sprites/UI`、`Arts/UI`、`Arts/TestUI` | Sprite、Catalog、UI、Localization | UIの配置先が複数。提出品からGUIDへの対応台帳なし |
| 3D | `03_Art/3D/{Character,Weapon,Environment,Vehicle}/.../Export` | `Arts/Models/{Symphony,Soldier14,Weapons,Backgrounds}` | Prefab・Scene・Config | Drive名とモデル名が不一致。テクスチャ・Material設定は別工程 |
| Motion | `03_Art/Motion/MOT_*/Export` | `Arts/Animation/Clips`、PlayerAnimationConfig | ClipのGUID、Config経由 | FBXの採用版・rig・clip抽出・差し替えを人が判断 |
| VFX | `03_Art/VFX/VFX_*/Export` | `Arts/VFX/Prefabs_*`等 | Prefab・演出Config | prefabだけでなく材質・shader・textureの依存確認が必要 |
| BGM / SE | `04_Sound/BGM,SE,CRI` | `StreamingAssets/*.acb,*.acf` | CRI Cue、MusicPlayer等 | 原音→CRIプロジェクト→出力の採用版を追跡できない |
| Voice | `04_Sound/Voice,CRI` | CRIデータ、VoiceSource | CriAtomSourceに設定したCue | 全ボイスの存在・Cue対応・採用済み率は未確認 |
| Scenario CSV | `02_Scenario/Csv` | `StreamingAssets/ScenarioAuthoring` | ID→`<id>.events.csv` | ダウンロード成功とCSV列・参照IDの妥当性は別 |

根拠：Snapshot「00_README」、Arts実配置、CSV:178・281・363、Scenario読込:30〜56。
音声の実行先は`4.View/Persistent/Music/MusicPlayer.cs:13,37`、`SoundEffectSource.cs:11,101`、`Voice/VoiceSource.cs:11,116`。
「自動化の接続未確認」は、CRIツールや担当者の手順が存在しないという断定ではない。

### Driveの再編とUnity側の残存構造

Drive記録の最後の追記では、提出品75件をSource / Exportに分けたとされる。
内訳は3D 19、Motion 6、VFX 14、2D 31、広報5。納品完了や実装済みの件数ではない。
33ファイルの改名記録がある一方、Unity側改名は残作業として記載されている。
歩兵モデル仮名、サブマシンガン原本とExportの版差、移動モーション2候補も確認待ちである。
これらの所有権・コピー履歴はDriveレビューに譲り、本レビューでは採用版と実行参照の接続を扱う。

Syncは指定フォルダ以下を再帰走査し、子フォルダ名をローカルにそのまま組み合わせる（Sync:283〜305）。
既定のフォルダ除外はDocumentのみで、Source・Archiveはない（Sync設定:15〜17）。
拡張子除外にはzip等があるが、psd・spp・mb・fbxはない（同:18〜42）。
Manifestの差分はファイル識別子と更新日時で、UnityのGUID・採用状態・改名計画ではない（Sync:164〜178）。
したがって提出品の上位を指定すると、Sourceも取り込まれる可能性がある（推測）。
その可能性とは別に、`AssetStoreTools/_drive/`内の制作原本の存在は今回の静的確認事実である。

### 命名規則と実名の定量照合

規則の版を区別する必要がある。
命名提案は2026-07-19時点の107項目を元に`CHR_`等を提案し、実際の改名は行っていないと明記する。
Snapshotの現行READMEは`CHM_`/`CHS_`等と`<接頭辞>_<カテゴリ>_<名前>[_<番号>]`を案内する。
日付・版・空白・記号を避ける方針は共通だが、旧提案の全候補を現行の確定名と扱えない。
#2084は規則確定、#2111はDBに合わせた改名、#1996は綴り・仮名の関連としてDrive記録に登場する。
Issue本文が未取得のため、これらの完了状態や例外規定は判定していない。

標本はArts配下のpng・fbx・prefabを拡張子ごとにパスの辞書順で並べ、各12件を等間隔に選んだ。
母集団はpng 327件、fbx 122件、prefab 77件。meta・外部パッケージは分母から除いた。
画像にはモデルの補助テクスチャも含むため、DBの「提出品1件」に対する適合率ではない。
等間隔標本であり無作為標本ではない。信頼区間・全体推定値は提示しない。
接頭辞は種類に対応するSnapshotの集合、文字条件は英数字とunderscore、日付型の数値を除外した。
名称の意味・PascalCase・正式カテゴリの完全一致は、この簡易形式検査の対象外である。

| 標本 | 件数 | 接頭辞一致 | 接頭辞＋文字・日付条件 | 形式適合率 |
|---|---:|---:|---:|---:|
| PNG | 12 | 6 | 5 | 41.7% |
| FBX | 12 | 0 | 0 | 0.0% |
| Prefab | 12 | 0 | 0 | 0.0% |
| 合計 | 36 | 6 | 5 | 13.9% |

以下は同じ標本36件の全件である。パスは`Assets/Arts/`からの相対表記。

| 種類 | 相対パス | 形式適合 | 主な不一致 |
|---|---|---|---|
| PNG | `Images/2ビットマップ.png` | × | 接頭辞・日本語 |
| PNG | `Images/Sprites/TutorialPopupImages/New/OrangeAttack_jp.png` | × | 接頭辞 |
| PNG | `Images/Sprites/UI/OutGame/UI_ ResearchScene_EmphasisPanel.png` | × | 空白 |
| PNG | `Images/Sprites/UI/Scenario/Command/UI_HideAllToggle.png` | ○ | 簡易形式のみ確認 |
| PNG | `Images/Sprites/UI/UI_InGame_HP_BackGround.png` | ○ | 同上 |
| PNG | `Images/Textures/EnemyAttack_Circle_BigGear.png` | × | 接頭辞 |
| PNG | `Models/Backgrounds/BuildingParts/Props/CenterTower/BgMD_CenterTower_BgM_CenterTower_MetallicSmoothness.png` | × | 接頭辞 |
| PNG | `Models/Backgrounds/Props/Helicopter/BgM_helicopter_01_MetallicSmoothness.png` | × | 接頭辞 |
| PNG | `Models/Weapons/Pistol/WPN_HandGun[0812]-20260819T013936Z-1-001/WPN_HandGun[0812]/texture/HandGun_Tx_Metallic.png` | × | 接頭辞。親階層にも日付・記号 |
| PNG | `TestUI/UI_Research_Reset.png` | ○ | 簡易形式のみ確認 |
| PNG | `UI/UI_Button.png` | ○ | 同上 |
| PNG | `UI/UI_Status_Range.png` | ○ | 同上 |
| FBX | `Animation/Clips/20260816_dodge.fbx` | × | 接頭辞・日付 |
| FBX | `Animation/Clips/Sympnonhy_HGpose_0615_3.fbx` | × | 接頭辞・日付型。綴りも要確認 |
| FBX | `Models/Backgrounds/BuildingParts/Broken_Walls/wall_2window_01_Broken_03.fbx` | × | 接頭辞 |
| FBX | `Models/Backgrounds/BuildingParts/Broken_Walls/wall_Top_Broken_01.fbx` | × | 接頭辞 |
| FBX | `Models/Backgrounds/BuildingParts/Floor/Floor_Break_01_07.fbx` | × | 接頭辞 |
| FBX | `Models/Backgrounds/BuildingParts/Floor/stairs_01_Broken_01.fbx` | × | 接頭辞 |
| FBX | `Models/Backgrounds/BuildingParts/Walls/Corner_wall_01_01.fbx` | × | 接頭辞 |
| FBX | `Models/Backgrounds/BuildingParts/Walls/wall_Full_04.fbx` | × | 接頭辞 |
| FBX | `Models/Backgrounds/BuildingParts/Walls/wall_window_01.fbx` | × | 接頭辞 |
| FBX | `Models/Backgrounds/Props/Helicopter/Bg_helicopter_01.fbx` | × | 接頭辞 |
| FBX | `Models/Soldier14/Soldier14_0808.fbx` | × | 接頭辞・日付型 |
| FBX | `Models/Weapons/Sniper Rifle/M82_SniperRifle[0911].fbx` | × | 接頭辞・日付・記号 |
| Prefab | `Models/Backgrounds/BuildingParts/Building_Prefabs/Building_01_01.prefab` | × | 接頭辞 |
| Prefab | `Models/Backgrounds/BuildingParts/Building_Prefabs/Building_03_01.prefab` | × | 接頭辞 |
| Prefab | `Models/Backgrounds/BuildingParts/Building_Prefabs/Building_07_05.prefab` | × | 接頭辞 |
| Prefab | `Models/Backgrounds/BuildingParts/Prefabs/BrokenFloor_01.prefab` | × | 接頭辞 |
| Prefab | `Models/Backgrounds/BuildingParts/Prefabs/BrokenFloor_08.prefab` | × | 接頭辞 |
| Prefab | `Models/Backgrounds/BuildingParts/Prefabs/Floor_Break_01_03.prefab` | × | 接頭辞 |
| Prefab | `Models/Backgrounds/BuildingParts/Prefabs/building_01_02.prefab` | × | 接頭辞 |
| Prefab | `Models/Backgrounds/BuildingParts/Prefabs/building_07_02.prefab` | × | 接頭辞 |
| Prefab | `Models/Backgrounds/Field_Prefab_BuildingsCombined_01.prefab` | × | 接頭辞 |
| Prefab | `VFX/NewExplosionEffect.prefab` | × | 接頭辞 |
| Prefab | `VFX/Prefabs_Firing/Firing_Masingan_05.prefab` | × | 接頭辞 |
| Prefab | `VFX/Prefabs_idou_sunasmoke/walk_smoke.prefab` | × | 接頭辞 |

別集計として命名提案の表から候補名を抽出し、重複・REF・明示的な親カテゴリ5件を除いた。
96候補のうちArtsのpng/fbx/prefab/anim/asset/mat/tga/jpgのstemに完全一致したのは3件、3.1%。
一致名は`UI_LogDisplay`、`UI_AutoButton`、`UI_HideAllToggle`で、UIカテゴリでは3/22＝13.6%だった。
音声候補28件を除いた68候補では3/68＝4.4%。これは旧提案に対する実名存在率である。
未制作・別名・CRIのCue・対象外データを区別していないため、欠品率や命名違反率とは呼ばない。
最新マスターアセットDBとUnityの**正確な名前一致率は未算出**。本文取得後に採用対象を固定して再集計する。

### AssetStoreToolsの位置付けと配布

`.gitmodules:1〜4`はAssetStoreToolsを別リポジトリのサブモジュール、追従ブランチをmainとする。
実配置には外部VFX、Animation、CRI、APIライブラリに加え、`_drive/`の制作物が混在する。
`_drive/3D/背景/乗り物/戦車/tank_01.spp`は209,495,066 bytes、`tank_T-72.mb`は134,785,948 bytes。
従って「第三者製品だけの領域」でも「実行用Exportだけの領域」でもない。
`_drive`の非metaファイルは172件、1,111,243,323 bytes（約1.035 GiB）である。
内訳にはPNG82、FBX33、Prefab14、MB10、MP4 10、WAV6、unitypackage4、SPP1がある。
外部製品の総容量と、旧Drive複製の容量は区別して削減対象を選ぶ必要がある。
#1985の素材集約・パッケージ配布と#2137のサブモジュール化の関係はDrive記録で未決とされる。
現在のIssue完了状態は未取得であり、未決という評価は保存資料と現行二経路に基づく。

| 経路 | 確認した構成 | 固定できるもの／できないもの |
|---|---|---|
| サブモジュール | `.gitmodules`、実ディレクトリ | 本体のgitlinkで版を固定する設計が可能。今回gitlink値は取得していない |
| Single | Single設定:24,27,30,33 | 個別Package→使用依存→ZIP→Drive Upload |
| Combine | 同ディレクトリのCombine設定:23,26,29 | 結合Package→使用依存→ZIP。Uploadステップはない |
| CI取得 | CI:1013〜1095 | 検索条件の最新ZIP1件。コードと同時点の承認版保証はない |
| CI配置 | CI:1099〜1113以降 | Unitypackageをtar展開しpath/metaを直接配置 |

CI checkoutにはsubmodules指定がない（CI:438〜442）。一方、Driveからの取得は明示的なビルド工程である。
開発端末のサブモジュールとCIのZIPが同じ内容であるかを照合する固定値は今回確認できなかった。
配布経路を二つ残す場合でも、「サブモジュールの版から作ったZIP」である証拠が必要になる。
Discordでは手動PackagerのImport案内が残る（`Docs/DiscordLog/プランナー_1429496779073912985.txt:8604`）。
ビルド依存だけを抽出するフィルターの要望もある（`Docs/DiscordLog/プログラマー_1429496672613826620.txt:2899〜2900`）。
これは運用・要求の証拠であり、現在のフィルター実装完了を示す証拠ではない。
Uploadは送信失敗をcatchしてログにし次へ進む（Upload:69〜91）。送信成功を上位へ保証する契約は弱い。
サービスアカウント・My Driveの成立性と権限対応はDriveレビューD05/D06/D18を参照する。

### Addressablesと実行参照

AssetGroupsの保存設定にはGameData.Shared 50件、Demo 5件、Release 3件がある。
同じStageTreeAsset等のaddressがDemo/Releaseにあるが、直ちに重複不具合とは判定しない。
`GameDataVariantBuildProcessor.cs:24〜59`がProfileに応じたgroup適用と必須登録を検査する。
Key検査はビルド対象groupを選び、空・区切り文字・ファイル名との関係・重複を検査する（Key検査:73〜103,125〜152）。
Loaderはcontextごとにhandleを保持し、失敗時および明示解放時にReleaseする（Loader:38〜45,93〜96,144〜162）。
例として`OutGame/Scenario/ScenarioCom.cs:83〜86`が背景・Motion・立ち絵Catalogと設定をロードする。
CatalogのEntryはSprite / AnimationClipの直接参照を持つ（`BackgroundCatalogEntry.cs:20`、`AnimationCatalogEntry.cs:20`）。
`Assets/Level/Data/Master/Character/PlayerAnimationConfig.asset:18〜48`にもClipの直接GUID参照がある。
metaのGUIDを静的に逆引きすると、同Config:23は`Arts/Animation/Clips/Symphony_run_v001_nons.fbx`、
同:28は`Arts/Animation/Clips/20260816_dodge.fbx`、同:18と33は`AssetStoreTools/AKITO/`のanimへ解決する。
旧日付名のMotionと外部領域のClipが、同じConfigの採用参照として混在する具体例である。
PortraitCatalogAsset:20,25,30は`CHS_Symphony.png`、`CHS_Prelude.png`、`CHS_Triad.png`へ解決する。
こちらは接頭辞とGUID参照が接続済みの例。全体を一律に未整備とは評価しない。
改名時はファイル名だけでなく、metaと既存GUID参照を保全する必要がある。

Sharedの梱包設定は`AssetGroups/Schemas/GameData.Shared_BundledAssetGroupSchema.asset`に集中する。
Store検査はLocal Path、LZ4、PackTogether、Remote Catalog無効、Player同時Build、AABを要求する。
ただしコード全体が`#if UNITY_ANDROID`で囲まれる（Store検査:12,184）。Steam側での同検査実行は保証されない。
ローカル同梱の方針は明確だが、Shared 50件の実依存サイズ・重複・ロード常駐量はBuild Layoutなしでは判断できない。

### Import統一とAndroid

`ProjectSettings/PresetManager.asset:7`は`m_DefaultPresets: {}`。Assets内の`.preset`は0件だった。
Assets/Scripts、Assets/Editor、SinfoniaOperatorのC#検索でTexture/Model/AudioのOnPreprocess実装は見つからない。
AssetPostprocessor自体は2件あり、ProjectWindowのIndex更新とSolution注入用である。
根拠：`SourceDataProjectWindowIcons.cs:96`、`SolutionProjectInjector.cs:16`。
外部パッケージの独自Import処理・Unity標準機能まで不存在と主張するものではない。

Artsの全PNG327件のmetaを集計した。これは標本ではなく、この拡張子に対する全件集計である。

| 項目 | 件数 | 読み取り |
|---|---:|---|
| Read/Write無効 | 325/327 | 多数はCPU側複製を要求しない設定 |
| Read/Write有効 | 2/327 | OutGame_NameTab、Skill_Panel_frame。利用理由は未確認 |
| Mipmap有効／無効 | 147 / 180 | 3D textureとUIが混在するため差自体を不具合としない |
| 最初のmaxTextureSizeが2048／512 | 325 / 2 | 共通設定の保存値。実寸・最終Android値とは別 |
| Androidの明示override有効 | 5/327＝1.5% | 4件はmax512・format50、1件はmax2048・format47 |
| Android項目あり、override無効 | 276/327 | 多数が自動/defaultの選択に依存 |
| Android項目なし | 46/327 | Android非対応の意味ではない |

formatの数値だけから対応端末・実効GPU形式・画質を確定していない。
例：M82 BaseColorのmeta:97〜105はAndroid、max2048、format -1、override 0。
モデルではSymphony_0828のmeta:33,37がReadable 0 / meshCompression 0。
Soldier14_0808のmeta:48,52はReadable 1 / meshCompression 0で、モデル種別ごとの判断が必要。
ProjectSettings:273はAndroidTargetArchitectures 2、同:298〜299はAABサイズ検査150の保存設定。
これらはImportの画質・メモリ予算や実機での性能達成を証明しない。
ASTC/ETC2を一律強制するより、UI・法線・背景・キャラ・Motionごとに用途別の既定値を決めるべきである。
Assetsのファイル検索でSpriteAtlasアセットは見つからなかった。動的生成やビルド時生成の有無は未確認。

### 大容量ファイルとLFS

容量はローカルの通常ファイルの論理サイズ合計。1 GiB＝1,073,741,824 bytes。
meta・未追跡物を含み、Git操作をしていないため追跡ファイルだけの値ではない。
各行は重複する範囲を含む。Assetsとその子、Git objectsとmodulesを単純加算しない。

| 領域 | ファイル数 | bytes | GiB |
|---|---:|---:|---:|
| Assets全体 | 22,695 | 25,862,491,328 | 24.086 |
| AssetStoreTools | 13,760 | 24,812,933,132 | 23.109 |
| Arts | 2,097 | 693,305,399 | 0.646 |
| StreamingAssets | 60 | 115,267,953 | 0.107 |
| 本体`.git/objects` | 5,134 | 1,526,861,433 | 1.422 |
| `.git/modules` | 5,076 | 8,199,608,537 | 7.636 |
| Library | 166,179 | 55,795,226,527 | 51.963 |

Libraryは生成キャッシュでありリポジトリ配布サイズではない。上記値をclone容量と呼ばない。
本体とサブモジュールのGit保存域は既に相応の容量だが、履歴全量・リモート転送量は未計測である。
ルート`.gitattributes:1〜3`はsh/commandのLF指定のみで、LFS filter設定はない。
Assets配下の追加`.gitattributes`と本体`.git/info/attributes`も今回の検索では見つからなかった。
グローバル属性・サブモジュールのローカル属性・リモートLFS実使用までは確認していない。

| 大容量の例 | bytes | 意味・注意 |
|---|---:|---|
| `AssetStoreTools/_drive/3D/背景/乗り物/戦車/tank_01.spp` | 209,495,066 | 制作原本がAssets内にある |
| 同`tank_T-72.mb` | 134,785,948 | 同上。実行依存かは未確認 |
| `AssetStoreTools/Soldiers-Pack/Mesh/US-Soldier.fbx` | 67,817,612 | 外部モデル |
| `AssetStoreTools/Soldiers-Pack/Textures/backpack2_Roughness.tga` | 67,108,908 | 外部textureの例 |
| `StreamingAssets/Title/GamePV.mp4` | 83,948,288 | 約80.1 MiBの動画。配信サイズ確認対象 |
| `StreamingAssets/CueSheet_0.acb` | 16,311,680 | 約15.6 MiBのCRI出力 |
| `StreamingAssets/MusicSyncMock.acb` | 14,305,088 | 名前だけでテスト専用と断定しない |
| `Arts/Models/Weapons/Sniper Rifle/M82UV_M82_MATERIAL_BaseColor.png` | 18,347,304 | 原画像サイズとGPUメモリは別 |

## 良い点

- Source / Exportを提出品単位に揃えたため、修正原本とUnity用納品を同じ対象として探せる（Snapshot）。
- 命名提案は重複タイトルを接頭辞で分け、銃器名の方針や親カテゴリの扱いも記す（命名提案）。
- CSV取得は全件ダウンロード後に保存し、保存名衝突で停止する（CSV:207〜248）。
- Syncは更新時刻による差分、キャンセル、最後のまとめImportを備える（Sync:164,215〜247）。
- 配布設定はSingle / Combineと使用依存処理に分かれ、処理順がアセットとして読める（各Pipeline設定）。
- CIはZIP内unitypackageが0件なら失敗にする（CI:1090〜1093）。
- Addressablesのキー検査とDemo/Release切替の必須データ検査がある（Key検査、VariantBuildProcessor）。
- Loaderが失敗時のReleaseまで共通化している（Loader:93〜96）。
- Androidのローカル同梱・AAB条件をコードで検査する（Store検査:80〜92,119〜167）。
- 大半のPNGはRead/Write無効であり、無差別に読み取り可能にしている状態ではない（meta全件集計）。

## 問題点

高＝ビルド再現性・主要データ保全に影響、中＝採用・納品・メモリ管理の継続負担、低＝局所的な検証不足。
Driveレビューとの重複は参照に留め、アセット採用・Import・梱包への影響を記す。

| ID | 重大度 | 問題 | 根拠 | 影響 |
|---|---|---|---|---|
| A01 | 高 | CIの素材版が更新日時最新ZIPに依存 | CI:1025〜1037、DriveレビューD10/D11 | 同じコードから異なる素材でビルドされ得る（推測） |
| A02 | 中 | サブモジュールとZIPの内容同一性・役割が不明 | `.gitmodules:1〜4`、CI:438〜442,1013、Single設定、#1985/#2137保存記録 | 開発端末とCIの差を追えない |
| A03 | 中 | 提出品の採用ExportとUnity GUIDの対応が未整理 | Drive記録の採用候補残件、PlayerAnimationConfig:18〜48、DriveレビューD12/D15 | 差し替え対象と採用版の判断が担当者に依存 |
| A04 | 中 | 命名規則が提案・DB・Drive・実ファイルで揃わない | 命名提案、Snapshot、36件標本5件適合、#2084/#2111/#1996関連記録 | 探索・自動照合・納品判定が困難 |
| A05 | 高 | 制作原本がAssets/AssetStoreTools内に残る。SyncもSourceを既定除外しない | `_drive/.../tank_01.spp`、`tank_T-72.mb`、Sync設定:15〜42、Sync:283〜305 | 開発環境・配布が肥大。上位同期で原本を再取得し得る（推測） |
| A06 | 中 | Importの用途別既定値がない | PresetManager:7、preset0件、自作OnPreprocess検索 | 差し替え時に設定判断を繰り返し、担当間の差が残る |
| A07 | 中 | Androidの形式・画質・メモリ予算を確認できない | PNG override5/327、モデルmeta差、Store検査の範囲 | 性能達成を検証しにくい。性能不良の実発生は未確認 |
| A08 | 高 | 大きいbinaryとGit保存域に対し共有LFS方針が見えない | `.gitattributes:1〜3`、容量表、209 MB原本 | 履歴・取得・CI準備の負担が増える可能性。LFS必須とは断定しない |
| A09 | 中 | CRI原音・プロジェクト・acb/acf・Cueの採用版連携が未確認 | Snapshot 04_Sound、StreamingAssets、VoiceSource、DriveレビューD15 | 音声差し替えと復元の版対応を証明できない |
| A10 | 中 | CSV取り込みは列・Catalog・Cueの整合を完了条件にしない | CSV:281〜299、Scenario読込:30〜56、CatalogEntry:20 | ダウンロード成功後も再生時まで内容不整合が遅延し得る（推測） |
| A11 | 中 | Uploadの失敗がログで吸収される | Upload:69〜91 | パッケージ生成成功と配布成功を分けて判断する必要 |
| A12 | 低 | Steam向けと説明するStore検査がAndroid条件付き | Store検査:12,17,184 | Windows側の同条件チェックをこのコードでは保証しない |
| A13 | 中 | 共通bundleとStreamingAssetsの実梱包・重複・未使用量が未測定 | Shared50件、Schema、StreamingAssetsの動画・develop CSV | 最適化の優先順位を採用素材の実サイズで決められない |

## 改善提案

工数は担当1名の目安。S＝半日以内、M＝1〜3人日、L＝4人日以上。全面改名・実機測定は別工数になり得る。
今回の作業は提案のみであり、以下の変更・検証は実行していない。

### すぐやる（今週）

#### P1. ビルドに使う素材版を固定する

- **何を:** 当面の承認済みZIPとサブモジュール版の対応を1組固定する。
- **なぜ:** CIの「最新」選択ではコードの再ビルド結果を説明できない。
- **手順:** 開発担当がZIP内容、依存、metaと承認素材を確認し、版・内容hashを内部の配布記録へ残す。
- **手順:** CIで承認版または期待hashを選び、不一致を失敗にする。現在のlatestは移行期間のみとする。
- **手順:** DriveレビューP3の取得・展開検証と合わせ、生成成功・Upload成功・CI採用を別に確認する。
- **完了条件:** 同じ本体版と素材版で再取得でき、無関係な新規ZIPに切り替わらない。
- **工数感:** M。**関連問題ID:** A01、A02、A11。

#### P2. 現行採用物だけで提出品とUnity参照を結ぶ

- **何を:** #2111の対象と、サブマシンガン・移動Motion・主人公を優先して採用対応表を作る。
- **なぜ:** 名前を揃える前に、同じ制作物・採用版であることを確定する必要がある。
- **手順:** 制作担当と実装担当がSource版、Export版、Unityパス、GUID、参照Prefab/Configを照合する。
- **手順:** DB正式名は本文取得後に確認し、仮名・候補複数・原本欠落を別状態で残す。
- **手順:** 公開報告には論理名とUnityパスのみを出し、Drive識別情報は内部管理に留める。
- **完了条件:** 優先対象について「どれを差し替えるか」を担当者以外も判断できる。
- **工数感:** M。**関連問題ID:** A03、A04、A09。

#### P3. 取り込み範囲をExportに限定する

- **何を:** 上位フォルダの無差別同期を避け、既存原本と実行用素材を切り分ける。
- **なぜ:** 新しいDrive階層をそのままAssetsへ複製すると、整理済みの原本が再流入する。
- **手順:** 当面は各Exportを指定。Source・Archive除外と、許可するExport拡張子を設定する。
- **手順:** `_drive`の原本は参照・必要な再書き出し・保管先を確認してから移す。即削除しない。
- **手順:** 取得計画で追加・更新・同名衝突・meta衝突を確認し、移行前後のGUIDを照合する。
- **完了条件:** 代表提出品の取り込みでSourceが増えず、採用素材のGUIDを維持できる。
- **工数感:** M。**関連問題ID:** A05、A08。DriveレビューP5/P7と共同。

### 次に（1か月）

#### P4. 命名を採用対象から段階的に統一する

- **何を:** DB正式名と旧名の対応を確定し、Motion・モデル・VFXを少量ずつ改名する。
- **なぜ:** 標本ではUIに適合例がある一方、FBXとPrefabは接頭辞なしの旧名が多い。
- **手順:** #2084/#2111/#1996の最新内容を取得し、CHR/CHM/CHSと補助textureの例外を決める。
- **手順:** Unityの改名操作でmetaを保持し、address・CSV ID・Cueの変更要否を別に判断する。
- **手順:** 旧名検索、GUID参照、代表Scene、Addressables Buildを改名単位で確認する。
- **完了条件:** 採用対象の完全一致率をDB行単位で示し、未制作・対象外・未改名を分けられる。
- **工数感:** M〜L。**関連問題ID:** A03、A04。

#### P5. 用途別Import既定値とAndroid予算を導入する

- **何を:** UI、3D texture、normal、character model、Motionに少数のPreset・検査を用意する。
- **なぜ:** 既定Presetが空で、プラットフォーム差を個々のmetaだけから理解する必要がある。
- **手順:** 既存の代表metaを制作・開発担当で確認し、正当なReadableやmipmap例外を残す。
- **手順:** Androidの画質・max size・圧縮・model/animation精度を実機で比較して決める。
- **手順:** 新規Importに既定を適用し、既存全件の強制再Importは避ける。逸脱理由を検査する。
- **完了条件:** 同じ用途の新規納品が同じ設定になり、代表端末のメモリ・描画・音声同期基準を満たす。
- **工数感:** M〜L。**関連問題ID:** A06、A07。

#### P6. 配布正本と大容量保管を決める

- **何を:** AssetStoreToolsの外部製品・制作Export・制作Sourceの役割と版管理を分ける。
- **なぜ:** 23.109 GiBの領域に原本とSDKが同居し、二経路の更新順を判断しにくい。
- **手順:** #1985/#2137を確認し、サブモジュール正本か固定Package正本かを選ぶ。
- **手順:** 二経路を残すなら一方を他方から生成し、元版とhashを必須にする。
- **手順:** 原本の保管を先に整理し、残る大型binaryへのLFS/成果物配布の費用と取得条件を比較する。
- **手順:** LFS採用時はサブモジュール側・CI取得も揃える。履歴移行は別の合意済み作業にする。
- **完了条件:** 開発端末とCIが同じ素材版を使い、原本の修正・復元先が分かる。
- **工数感:** L。**関連問題ID:** A01、A02、A05、A08。

#### P7. CSVとCRIの採用整合を検査する

- **何を:** CSVの名前・列・参照IDと、音声のCue・出力版をImport後に照合する。
- **なぜ:** 転送成功はシナリオ再生や音声差し替えの成功を保証しない。
- **手順:** `<id>.events.csv`と列の形式、Catalogにある参照、必要Cueの存在を採用対象で検査する。
- **手順:** CRIプロジェクト版、acb/acf版、論理Cue名、組み込んだPrefabの対応を記録する。
- **手順:** AndroidのStreamingAssets読込と代表シナリオ・Voiceを確認する。Drive権限変更はDrive観点で扱う。
- **完了条件:** 未知ID・欠落Cueがビルド/採用前に分かり、正常な代表再生を確認できる。
- **工数感:** M。**関連問題ID:** A09、A10。

### 将来

#### P8. 実梱包サイズを基準に最適化する

- **何を:** Android/Windows、Demo/ReleaseのBuild Layout・梱包一覧・ロード量を比較する。
- **なぜ:** raw Assets容量だけでは未使用・重複・常駐bundle・StreamingAssetsの寄与を判断できない。
- **手順:** GameData.Sharedの50登録と依存を追い、シーン直接参照との重複を測る。
- **手順:** GamePV、CRI mock名データ、develop CSVの採否を参照根拠で決める。
- **手順:** 実測で必要ならgroup分割・atlas化を行い、画質とロード時の挙動を比較する。
- **手順:** Store検査を対象OSに合わせ、説明とコンパイル条件を揃える。
- **完了条件:** 配布サイズとピークメモリの上位原因を示し、削減効果を数値で確認できる。
- **工数感:** L。**関連問題ID:** A07、A12、A13。

## 他観点との接点

| 観点 | 本レビューで扱う境界 | 引き継ぐもの |
|---|---|---|
| Drive | ExportからUnity GUIDまでの採用接続 | DriveレビューD08〜D15、#2141、#2353。所有権・認証は既存レビュー参照 |
| 仕様書 | 規則とDB正式名・補助素材例外の一致 | 仕様書レビューの納品規則競合、命名提案、#2084/#2111 |
| コード | Config/Catalog、直接参照、handle寿命 | Loader、Initializer、改名時のGUID/address保持 |
| ゲーム | 見た目・Motion・音声の採用と体感 | 使用Motion候補、圧縮画質、CRI同期、Android代表端末 |
| 運用 | 素材版固定と配布成否の証拠 | CI最新ZIP、Upload結果、PRテンプレートの確認済み/未確認欄 |
| QA | 転送とImportと再生を別に判定 | CSV未知ID、欠落Cue、改名後Scene、Build Layout、実機再生 |

Driveの整理を再度全面変更するより、既存の提出品名を入口として採用対応表を整える方が範囲を限定できる。
既存レビューの権限・正本議論と重ねて新しい台帳を増やさず、アセットDBに実装対応を接続する。

## 未確認事項

- 最新Notion本文・マスターDB行の直接照合。今回取得不能なので仕様への完全適合を保証しない。
- #2084/#2111/#1996/#1985/#2137の現在の結論、#2353との分担、完了PR。
- Drive全ファイルの実一覧、Export実体、Sourceとのhash/版対応。
- サブモジュールの本体gitlink値、CI runner残存物、採用ZIPとの内容一致。
- AssetStoreToolsの外部パッケージャー本体とUsedDependenciesの実抽出範囲。
- グローバル・サブモジュールローカルのGit属性、リモートLFS使用量・料金・履歴全量。
- `_drive`とArtsの重複binaryのhash一致、制作原本がUnity依存として必要か。
- 全Scene/Prefabにおける採用モデル・VFX・Motion・音声Cueの網羅的参照。
- CRIプロジェクト原本、Android用出力条件、全VoiceのCue対応・採用率。
- CSV全件の内容検証、Catalog空行や未登録参照が実シナリオに与える影響。
- Android実機のGPU形式、画質、ピークメモリ、描画時間、リズム・音声同期。
- Addressables Build Layout、Scene/bundle重複、Shared依存サイズ、ビルド時Atlas生成。
- StreamingAssetsのdevelop CSV・mock名データ・動画の最終梱包と採用理由。
- Unity未起動のため、今回の設定がImport・コンパイル・実行で正常に働くという動的証拠はない。
