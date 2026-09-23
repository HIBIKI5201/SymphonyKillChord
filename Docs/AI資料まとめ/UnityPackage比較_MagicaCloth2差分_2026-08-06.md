# エクスポート済みUnityPackageと現プロジェクトの比較（パッケージ/ライブラリ単位）

対象ファイル: `C:\Users\kouki\Downloads\Export_AssetStoreToolsPackage_20260804_201710.zip`
中身: `AllPackages_20260804_201710.unitypackage`（2026-08-04 20:26 エクスポート、圧縮前約1.58GB）

このUnityPackageに記録されている `pathname`（インポート先パス）を全807件抽出し、現在のプロジェクト（`Assets/AssetStoreTools/`, 665ファイル）とパッケージ/ライブラリ単位で突き合わせました。個別ファイル単位の一覧は前回版（同フォルダ内、旧内容）から今回はパッケージ単位の集計に置き換えています。

## パッケージ単位 比較表

| パッケージ/ライブラリ | Package内総数 | 一致 | Packageのみ | Projectのみ | 状態 |
|---|---:|---:|---:|---:|---|
| CRIWARE (`CRIMW`) | 77 | 77 | 0 | 8 | コア一致（Project側にMac/WebGL向けネイティブファイル等が追加で存在） |
| 52SpecialEffectPack | 5 | 5 | 0 | 0 | 完全一致 |
| AKITO | 4 | 4 | 0 | 0 | 完全一致 |
| Farming_game_FX | 11 | 11 | 0 | 0 | 完全一致 |
| Free Slash VFX | 8 | 8 | 0 | 0 | 完全一致 |
| GabrielAguiarProductions (FreeQuickEffectsVol1) | 2 | 2 | 0 | 0 | 完全一致 |
| Hands_Weapons_Animations_Pack_Update | 1 | 1 | 0 | 0 | 完全一致 |
| JMO Assets (WarFX) | 11 | 11 | 0 | 0 | 完全一致 |
| Kevin Iglesias (Human Animations) | 2 | 2 | 0 | 0 | 完全一致 |
| **MagicaCloth2** | **194** | **0** | **194** | 0 | **現プロジェクトに完全未導入**（後述） |
| Microsoft.Bcl.AsyncInterfaces 6.0.0 | 2 | 2 | 0 | 6 | コア一致（Project側にLICENSE/nuspec等の補助ファイルのみ追加） |
| Microsoft.Bcl.TimeProvider 8.0.0 | 2 | 2 | 0 | 11 | コア一致（同上、buildTransitive含む） |
| MoCapCentral (MC_Sample) | 6 | 6 | 0 | 0 | 完全一致 |
| R3 1.3.0 | 2 | 2 | 0 | 3 | コア一致（補助ファイルのみ追加） |
| Rapier_Anim_Set | 4 | 4 | 0 | 0 | 完全一致 |
| Shinabro (Platform_Animation) | 2 | 2 | 0 | 0 | 完全一致 |
| Soldiers-Pack | 418 | 418 | 0 | 1 | ほぼ完全一致（Project側に `US-Soldier.prefab` が1件追加） |
| System.ComponentModel.Annotations 5.0.0 | 2 | 2 | 0 | 7 | コア一致（補助ファイルのみ追加） |
| System.Threading.Channels 8.0.0 | 2 | 2 | 0 | 11 | コア一致（同上、buildTransitive含む） |
| Unknowns / Effect | 17 | 17 | 0 | 0 | 完全一致（出典不明、要確認は継続） |
| VanillaLoopStudio (FreeSampleAnimationSet) | 11 | 11 | 0 | 0 | 完全一致 |
| Vefects (Free Fire VFX URP) | 16 | 16 | 0 | 0 | 完全一致 |
| WeaponsAsset (AK47_Textures) | 6 | 6 | 0 | 1 | ほぼ完全一致（Project側に `AK_Normal_OpenGL.png` が1件追加） |
| ZLinq 1.5.5 | 2 | 2 | 0 | 4 | コア一致（補助ファイルのみ追加） |

計23パッケージがこのExportと現プロジェクトの両方に確認でき、うち1パッケージ（MagicaCloth2）はPackageのみに存在します。Projectのみに存在する新規パッケージ（Exportに一切含まれない未知のパッケージ）はありませんでした。

## 状態の凡例・補足

- **完全一致**: Package内の全ファイルがProject内の同一パスに存在し、Project側に余分なファイルもない。
- **コア一致（補助ファイルのみ追加）**: NuGet系パッケージ（Microsoft.Bcl.*, System.*, R3, ZLinq）は、Exportにはコアのdllやアセンブリ関連ファイルのみが含まれ、LICENSE.TXT・THIRD-PARTY-NOTICES.TXT・`.nuspec`・`.signature.p7s`・Icon.png などの補助メタファイルはExport対象から漏れている。実害はなし。
- **CRIWARE**: Project側にのみ存在する8件はmacOS用バンドル(`cri_ware_unity.bundle`)とWebGL用`.jslib`、Editor拡張の一部シェーダー/アイコン。プラットフォーム対応ファイルがExport時に除外された可能性が高い。
- **Soldiers-Pack / WeaponsAsset**: Project側にのみ存在する1件ずつは、それぞれ `US-Soldier.prefab`（プレハブ本体）、`AK_Normal_OpenGL.png`（OpenGL向け法線マップ）。Export時に取りこぼした可能性あり。

## ⚠️ MagicaCloth2（194ファイル）— 現プロジェクトに完全未導入

このPackageには `Assets/AssetStoreTools/MagicaCloth2/` が丸ごと含まれていますが、現在のワーキングツリーには一切存在しません。

内訳: `Scripts/` 166、`Example (Can be deleted)/` 16、`Res/` 11、`MagicaCloth2.asmdef` 1（計194）

- このエクスポート作成時点（8/4）では導入されていたが、その後プロジェクトから削除された可能性
- 別環境/別ブランチで作成されたエクスポートで、現ブランチ（`feature/demo/level-design/master`）にはまだ未反映の可能性

どちらか判断がつかないため、MagicaCloth2を現プロジェクトに戻すべきか、このエクスポート自体が古い状態のバックアップなのか確認をお願いします。
