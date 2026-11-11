# Symphony SkinnedMeshRenderer統合

## 目的
`Player.prefab` のSymphonyモデル(Symphony_0828.fbx)は、SkinnedMeshRendererが52個ある。マテリアルが同一で競合しないメッシュを統合して数を減らす。

## 変更
- `Assets/Arts/Models/Symphony/Merged/`: 生成したPrefab Variant(`Symphony_0828_Merged.prefab`)と統合メッシュ5個(読み取り不可)。
- `Assets/Level/Prefabs/Master/InGame/Character/Player.prefab`: モデルの参照をVariantへ差し替え。モデルルート名は `Symphony_0324_SilToon` のまま。
- `OutGame.unity`・`DemoEnd.unity`: モデルルートTransformとAnimatorの参照を新しいfileIDへ張り替え。
- `SilToonShadowRenderPass`: 毎フレームの `sharedMaterials` 配列確保をやめ、再利用バッファへ取得する。
- 統合に使ったEditorツールは、再利用の予定がないため削除した。再統合の手順は下の「再統合の手順」に残す。

## 結果
- 描画されるSkinnedMeshRenderer: 52個 → 9個(統合5個 + 未統合4個)。
- 統合: SilToonTop 24個、SilToon 15個、Eye+EyeBrowOverlay 2個、Eye+EyeOverlay 2個、Hair 5個。
- 統合しない: Eyelashes・Skin2・Teeth(BlendShape付き。表情アニメーションの対象になるため)、Tongue(同マテリアルの相手がBlendShape付きのみ)。
- 元のRendererはVariantでは削除できないため、`enabled=false` にしたうえでGameObjectも非アクティブにしている(48個。いずれも子なし)。

## 検証
- compile: エラー0。
- 全ボーンをランダムに回転したポーズで、統合メッシュと元メッシュの頂点(ワールド座標)を比較。最大誤差 4e-7。
- PlayMode(OutGame): 統合Rendererが全て表示され、元Rendererが `enabled=false`。
- PlayMode(InGame): 攻撃ボタン押下後も、描画されるRendererが9個のまま。
- 未確認: Android実機、影の見た目の比較、GC・フレーム時間の計測。

## 再統合の手順
モデルを差し替えたときなど、やり直す場合の手順。

1. **グルーピング**: FBXのインスタンスで、次が全て同一のSkinnedMeshRendererを1グループにする。2個以上のグループだけ統合する。
   - マテリアル配列(**`Player.prefab` 側で上書きした値**。FBX既定のマテリアルでは別グループになる)
   - rootBone・影設定(shadowCastingMode・receiveShadows)・lightProbeUsage・motionVectorGenerationMode・allowOcclusionWhenDynamic
   - レイヤー(`Player`。`Player.prefab` の同名Rendererから写す)
   - 対象外: enabledがfalse、BlendShapeあり、サブメッシュが1でない、ボーンまたはマテリアルが未設定。
2. **メッシュ結合**: 頂点・法線・接線・UV0〜3・頂点カラーを元の座標のまま並べ、三角形のインデックスを頂点数ぶんオフセットする。Unity標準の `Mesh.CombineMeshes` はBlendShapeとボーンウェイトの扱いが合わないため使わない。
   - ボーンは(Transform, バインドポーズ)の組で重複を除いて並べ、ボーンウェイトの番号を振り直す。同じボーンでバインドポーズが違う場合は別エントリにする。
   - ボーンウェイトは `GetBonesPerVertex` と `GetAllBoneWeights` で扱い、4本を超える影響にも対応する。
   - 境界は元メッシュの境界の合併。Rendererの `localBounds` も元の合併にする。
   - 結合メッシュは `m_IsReadable: 0` で保存する。
3. **Variant作成**: FBXのインスタンスに、結合メッシュを持つSkinnedMeshRendererを子として追加し(bonesと rootBone は元と同じ)、元のRendererを `enabled=false` にしてGameObjectも非アクティブにする(子を持つものは止めない)。`PrefabUtility.SaveAsPrefabAsset` でFBXのVariantとして保存する。
4. **Playerへ反映**: `Player.prefab` を開き、モデルのインスタンスに `PrefabUtility.ReplacePrefabAssetOfPrefabInstance` でVariantを指定する(`KeepAllPossibleOverrides`)。ルート名が変わっていたら `Symphony_0324_SilToon` に戻す。
5. **参照の張り替え**(下の注意を参照)。

## 注意
- マテリアルとレイヤーはFBX既定ではなく `Player.prefab` 側で上書きしている。統合Rendererを `Default` レイヤーのままにすると、ホーム画面のプレビューカメラ(`Player` レイヤーのみ描画)に映らず、肌しか見えなくなる。
- Variantへ差し替えると、モデルルートTransformとAnimatorのfileIDが変わる。`Player.prefab` を参照する全シーン(OutGame・InGame・DemoEnd・WeaponViewTest)で、`guid: 85e4d6417f0f1bf4e8d3bb1a10e81051` の参照が `Player.prefab` に存在するかを確認し、変わったIDは旧→新を対応づけて張り替える。旧→新の対応は、旧Playerを一時コピーして開き、階層パスと型で新旧の `AssetDatabase.TryGetGUIDAndLocalFileIdentifier` を突き合わせて作った。このとき切れていたのは、OutGame・DemoEndの2種類(モデルルートTransformとAnimator)だった。
- `SilToonShadowCaster._renderers` は保存済みの配列で、旧52個のままだとキャラ影が描かれない。描画される9個へ設定し直す(コンポーネントの `Collect Renderers` は無効なRendererも含めるが、描画側で除外される)。
- `SilToonRenderController._skinnedMeshRenderers` は `SetUpRenderer` の実行時にだけ更新される。統合Rendererを含めるなら再実行する。
- 攻撃ポーズのFBXアニメーション(`Symphony_ARpose_*`・`Symphony_HGpose_*` の `Take 001`)は、元の階層パスの51個のRendererに `m_Enabled=1` のカーブを持つ。攻撃で再生されると、`enabled=false` にした元Rendererが再び有効になる。元RendererのGameObjectを非アクティブにして、有効になっても描画されないようにしている。コンポーネントの `enabled` はカーブで1に戻るため、Inspector上ではONに見える。
- 元Rendererが57個のまま `Player.prefab` に残る(FBXのVariantでは削除できない)ため、メモリ・インスタンス化のコストは減らない。減らすにはFBX自体を統合して出し直す必要がある。
