# 背景の頂点カラーをテクスチャへベイクしLOD対応する 企画・仕様検討

- ブランチ: `feature/demo/background-vertex-color-bake/agent`（ローカルでリネーム済み。未push・未コミット）
- 状態: 調査と企画のみ。コード・アセットは未変更。Unityでの実測は未実施。

## 1. 現在のシェーダー仕様（調査結果）

対象は `Assets/Arts/Models/Backgrounds/` の Shader Graph（URP Lit, URP 17.3 / Unity 6000.3.10f1）。

| シェーダー | 頂点カラー | 使っているマテリアル |
| --- | --- | --- |
| `building_wall.shadergraph` | 使う | `BuildingParts/Material/` の BrokenBrick 系・Concrete_wall 系 計6個 |
| `building_Floor.shadergraph` | 使う | Concrete / Concrete_Color_01 / 02 の3個 |
| `BigBuildingWall.shadergraph` / `BasicMaterial*.shadergraph` | 使わない | 対象外 |

頂点カラーの使い道（両シェーダー共通の構造）:

- **R**: ÷ → Clamp → Saturate → Subtract。ダメージ（Damage_BC / Normal / MetallicSmoothness）を出すマスク。
- **G**: wall は Multiply の連鎖→Add、Floor は ÷ → Saturate → Add/Subtract。汚れ（DirtColor / DirtAmount）のマスク。
- **RGB全体**: `SwitchVertexColorDisplay`（bool, 既定 False）で BaseColor に直接出すデバッグ表示。
- B・A の使用は今回の追跡範囲（下流6段）では見つからなかった。
- テクスチャのサンプルはすべて UV0（タイリング用）。ノイズ・位置・Triplanar 系ノードは無い。

頂点カラー付きメッシュ: `Backgrounds/vertexpainter/*_Edited.asset` 50個、計約20.7万頂点（最大3.1万）。
UV1 を持つものが39個、持たないものが11個。UV1 が重なりの無い展開かは未確認。
これらは `Field_Prefab*.prefab` 経由で `Stage_01.unity` / `Stage_02.unity` に置かれている。

**LODで色が崩れる理由**: マスクが頂点単位の値なので、頂点を減らすと補間の範囲が変わり、ダメージ・汚れの境界が移動・消失する。

## 2. ベイクツールの実現性

実現できる。Editor拡張（`Assets/Editor/Scripts/` 配下）で次の手順を組める。

1. **ベイク用UVの用意**: UV1 が無い・重なるメッシュは `Unwrapping.GenerateSecondaryUVSet` で生成する。テクスチャはUV0のタイリングと独立させる必要があるため、UV0 は使えない。
2. **ラスタライズ**: UV1 空間で三角形を描き、頂点カラーを補間してテクスチャに書く。方法は2案。
   - A: CPU で三角形を走査して `Texture2D` に書く。実装が単純で依存が少ない。
   - B: `CommandBuffer` + 専用シェーダーで UV1 を出力座標にして RenderTexture に描く。速いがシェーダーが1本増える。
3. **にじみ対策**: UVアイランドの外側を数px押し広げる（パディング）。無いとミップでシームが出る。
4. **保存**: PNG（リニア、sRGB オフ、R・G だけなら単チャンネル圧縮も可）で書き出し、TextureImporter を設定する。
5. **LOD生成**: ベイク後は UV1 とテクスチャだけ維持すればよい。LOD 側の簡略化で UV1 の境界を保つ必要がある。

## 3. ランタイム側の仕様案

- 新規プロパティ `_VertexMaskTex`（UV1 でサンプル）を追加し、R・G の取得元を頂点カラーから差し替える。
- 移行期間はキーワード（例 `_MASK_SOURCE_TEXTURE`）で頂点カラー版と切り替え、見た目の比較を可能にする。
- `SwitchVertexColorDisplay` はマスクテクスチャ表示に置き換える。
- テクスチャサンプルが1回増えるが、頂点数削減の効果の方が大きい見込み（要実測）。

## 4. 未確認・判断が必要な点

- 実際のボトルネックが頂点数か（Profiler / Frame Debugger での確認が先）。
- UV1 が既存のライトマップ用UVか、重なりが無いか。
- 解像度（メッシュあたり 256〜1024 のどれか）とテクスチャメモリの予算。アトラス化するか。
- LOD メッシュの生成手段（手作業・DCC・Unity の Mesh LOD / Asset Transformer など）。
- B・A チャンネルを将来使う予定があるか。

## 5. 次の作業候補

1. Unity で Stage_01 の描画負荷を計測し、頂点数が主因か確認する。
2. 1メッシュでベイクの試作（案A）を作り、頂点カラー版と見比べる。
3. シェーダーにマスクテクスチャ経路を追加する。
4. 全50メッシュに展開し、LOD を設定する。
