# Symphony SkinnedMeshRenderer統合

## 目的
`Player.prefab` のSymphonyモデル(Symphony_0828.fbx)は、SkinnedMeshRendererが52個ある。マテリアルが同一で競合しないメッシュを統合して数を減らす。

## 変更
- `Assets/Editor/Scripts/MeshMerge/`: 統合ツール。Projectでモデル(FBX)と、マテリアルの参照元Prefabを選び、`Assets/KillChord/Create Merged Skinned Mesh Variant` を実行する。
- `Assets/Arts/Models/Symphony/Merged/`: 生成したPrefab Variant(`Symphony_0828_Merged.prefab`)と統合メッシュ5個。
- `Assets/Level/Prefabs/Master/InGame/Character/Player.prefab`: モデルの参照をVariantへ差し替え。

## 結果
- 有効なSkinnedMeshRenderer: 52個 → 9個(統合5個 + 未統合4個)。
- 統合: SilToonTop 24個、SilToon 15個、Eye+BrowOverlay 2個、Eye+EyeOverlay 2個、Hair 5個。
- 統合しない: Eyelashes・Skin2・Teeth(BlendShape付き。表情アニメーションの対象になるため)、Tongue(同マテリアルの相手がBlendShape付きのみ)。
- 元のRendererはVariantでは削除できないため `enabled=false` にしている。

## 検証
- compile: エラー0。
- 全ボーンをランダムに回転したポーズで、統合メッシュと元メッシュの頂点(ワールド座標)を比較。最大誤差 4e-7。
- 未確認: Play中の見た目、実機、Android。

## 注意
- マテリアルはFBX既定ではなく `Player.prefab` 側で上書きしているため、ツール実行時は `Player.prefab` を参照元として選ぶ。
- 統合メッシュは読み取り不可(`m_IsReadable: 0`)で保存している。
- `SilToonRenderController._skinnedMeshRenderers` は `SetUpRenderer` の実行時にだけ更新される。統合Rendererを含めるなら再実行する。
- `SilToonShadowCaster._renderers` は保存済みの配列。統合後は有効な9個へ設定し直した(旧52個のままだとキャラ影が描かれない)。モデルを再統合したら `Collect Renderers` で更新する。
- `SilToonShadowRenderPass` は毎フレームの `sharedMaterials` 配列確保をやめ、再利用バッファへ取得する。
