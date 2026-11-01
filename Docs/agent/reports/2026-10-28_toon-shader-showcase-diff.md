# ToonShader_Showcase と KillChord のシェーダー差分（概要）

- 比較元: `5unad0ke1/ToonShader_Showcase` `Assets/ToonShader/Scripts/Runtime/Shaders`（最終コミット 2026-05-30）
- 比較先: 本リポジトリ `Assets/Scripts/Shaders/SilToon`（最終変更 2026-10-21）
- 方法: ファイル単位の `diff`。描画結果・コンパイルは未確認。URPは両方 17.3.0。

## 結論
KillChord 側が大きく先行しています。Showcase 更新は「KillChord の SilToon 一式で置き換え、Showcase 固有の旧ファイルを整理する」形が現実的です。

## KillChord にだけある機能
| 機能 | ファイル | 備考 |
| --- | --- | --- |
| キャラ専用セルフシャドウ | `HLSL/CharacterShadow.hlsl`、`Runtime/SilToonShadowCaster/*` | Renderer Feature（`SilToonShadowFeature`）の追加が必要 |
| FakeShadow（髪→顔の擬似影） | `HLSL/FakeShadow/FakeShadow.hlsl`、`FAKE_SHADOW` パス | LightMode `SilToonFakeShadow`。RenderObjects 設定とステンシル bit1 が必要 |
| Toon PBR | `Fragment/ToonPBR.hlsl` | `_PBR_ON` |
| Sheer / Sheen | `Fragment/Sheen.hlsl` | `_SHEEN_ON` |
| 異方性ハイライト | `Fragment/AnisotropicHighlight.hlsl` | `_ANISO_ON`。uv3 の平滑化法線を使用 |
| 簡易SSS | `Fragment/SimplifiedSSS.hlsl` | `SSS_ON` |
| スメア（アウトライン） | `OutLine/Smears.hlsl` | `SMEARS_ON` |
| 入力定義の共通化 | `HLSL/SilToonInput.hlsl` | 各HLSLに散っていたプロパティ宣言を集約 |

## 共通ファイルの主な変更
- `SilToon.shader`: `[Toggle]` をキーワード付きにし、`shader_feature_local` で機能ごとに分岐。`_FORWARD_PLUS` → `_CLUSTER_LIGHT_LOOP`。ステンシル Read/WriteMask 追加。`[Headedr]` の誤記修正。include パスを `/` 区切りに統一。
- `Lights.hlsl`: `MainLight_float`（ShaderGraph 形式）→ `GetToonMainLight` 等に置換。`GetToonColor` は削除。影は法線方向へずらしてサンプリング。
- `OutLine.hlsl` / `LuminanceToOutlineThickness.hlsl`: スクリーン座標を `SV_Position` に変更、追加ライトのループ削除、`MAIN_LIGHT_CALCULATE_SHADOWS` で分岐。
- `Dither.hlsl`: 4x4 Bayer → 5段階の簡易パターンに変更（見た目が変わります）。
- `Fragment` / `ThroughFragment` / `NormalCombine` / `SilToonFresnel`: half 精度化、法線マップ・視差除去をキーワードで無効化可能に。
- `FaceLight` / `PerspectiveRemoval`: ほぼ同一。`ZOffset` / `ZeroZ` / `UVToSmoothNormal`: 同一。

## Showcase にだけあるもの
`AdditionalLights.hlsl`、`FaceShadow.hlsl`、`ManualPBR.hlsl`、`PerspectiveChange.hlsl`、`Shadow.hlsl`（KillChord では `DevelopProducts/Research` 側に残る旧版）、`AdvancedOutline.hlsl`、`TestPerspectiveRemoval.hlsl`（KillChord では `Assets/Scripts/Shaders/Graph/HLSL` へ移動）、`Test.shader`、`DebugShadowKeywords.shader`、ShaderGraph 5件。ShaderGraph から参照されているかは未確認です。

## 移植時の注意
- include パスが `Assets/Scripts/Shaders/SilToon/...` 固定のため、Showcase の配置に合わせて置換が必要。
- `[Toggle]` のキーワード化で既存マテリアルのキーワードが立たない可能性があり、マテリアルの再保存が必要になる見込み。
- C# 側（`SilToonGUI`、`StencilBits`、`HeadNFaceDTO`、asmdef）も差分あり。Showcase の `OutLineInjector` 等の Editor ツールは KillChord 側にない。
