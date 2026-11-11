using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace KillChord.Editor.MeshMerge
{
    /// <summary>
    ///     モデルのSkinnedMeshRendererをマテリアル単位で統合し、Prefab Variantとして保存します。
    ///     元のRendererは削除できないため無効化(GameObjectも非アクティブ化)し、統合Rendererを子として追加します。
    /// </summary>
    public static class SkinnedMeshMergeVariantBuilder
    {
        /// <summary>
        ///     統合済みのPrefab Variantを作成します。
        /// </summary>
        /// <param name="sourceAsset"> 元のモデル(FBXまたはPrefab)です。 </param>
        /// <param name="outputFolder"> Variantと統合メッシュの保存先フォルダです。 </param>
        /// <param name="materialReference">
        ///     マテリアルの参照元Prefabです。同名のRendererのマテリアルとレイヤーを元モデルへ反映してから統合します。
        ///     モデルを使う側のPrefabでマテリアルを差し替えている場合に指定します。
        /// </param>
        /// <returns> 作成したVariantのアセットパスです。失敗した場合は空文字です。 </returns>
        public static string CreateVariant(GameObject sourceAsset, string outputFolder, GameObject materialReference = null)
        {
            EnsureFolder(outputFolder);
            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(sourceAsset);
            try
            {
                ApplyReferenceMaterials(instance, materialReference);
                List<List<SkinnedMeshRenderer>> groups = CollectGroups(instance);
                int mergedCount = 0;
                int index = 0;
                foreach (List<SkinnedMeshRenderer> group in groups)
                {
                    string meshName = $"{sourceAsset.name}_Merged_{index:00}_{group[0].sharedMaterials[0].name}";
                    if (!TryMergeGroup(instance, group, outputFolder, meshName))
                    {
                        return string.Empty;
                    }
                    mergedCount += group.Count;
                    index++;
                }

                string variantPath = $"{outputFolder}/{sourceAsset.name}_Merged.prefab";
                PrefabUtility.SaveAsPrefabAsset(instance, variantPath);
                AssetDatabase.SaveAssets();
                Debug.Log($"[{nameof(SkinnedMeshMergeVariantBuilder)}] {mergedCount}個のRendererを{groups.Count}個に統合しました: {variantPath}");
                return variantPath;
            }
            finally
            {
                Object.DestroyImmediate(instance);
            }
        }

        /// <summary>
        ///     参照元Prefabの同名Rendererからマテリアルを写します。
        /// </summary>
        private static void ApplyReferenceMaterials(GameObject instance, GameObject materialReference)
        {
            if (materialReference == null)
            {
                return;
            }

            Dictionary<string, Material[]> referenceMaterials = new();
            Dictionary<string, int> referenceLayers = new();
            foreach (SkinnedMeshRenderer reference in materialReference.GetComponentsInChildren<SkinnedMeshRenderer>(true))
            {
                referenceMaterials.TryAdd(reference.name, reference.sharedMaterials);
                referenceLayers.TryAdd(reference.name, reference.gameObject.layer);
            }
            foreach (SkinnedMeshRenderer renderer in instance.GetComponentsInChildren<SkinnedMeshRenderer>(true))
            {
                if (referenceMaterials.TryGetValue(renderer.name, out Material[] materials))
                {
                    renderer.sharedMaterials = materials;
                }
                if (referenceLayers.TryGetValue(renderer.name, out int layer))
                {
                    renderer.gameObject.layer = layer;
                }
            }
        }

        /// <summary>
        ///     保存先フォルダを再帰的に作成します。
        /// </summary>
        private static void EnsureFolder(string folder)
        {
            if (AssetDatabase.IsValidFolder(folder))
            {
                return;
            }
            string parent = System.IO.Path.GetDirectoryName(folder).Replace('\\', '/');
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, System.IO.Path.GetFileName(folder));
        }

        /// <summary>
        ///     統合可能なRendererを、描画条件が同一のグループへ分けます。2個以上のグループだけを返します。
        /// </summary>
        private static List<List<SkinnedMeshRenderer>> CollectGroups(GameObject instance)
        {
            // 出現順を保つため、キーの初出順に並べる。
            Dictionary<string, List<SkinnedMeshRenderer>> groups = new();
            foreach (SkinnedMeshRenderer renderer in instance.GetComponentsInChildren<SkinnedMeshRenderer>(false))
            {
                if (!IsMergeable(renderer))
                {
                    continue;
                }
                string key = BuildGroupKey(renderer);
                if (!groups.TryGetValue(key, out List<SkinnedMeshRenderer> list))
                {
                    list = new List<SkinnedMeshRenderer>();
                    groups.Add(key, list);
                }
                list.Add(renderer);
            }
            return groups.Values.Where(list => list.Count >= 2).ToList();
        }

        /// <summary>
        ///     統合の対象にできるRendererか判定します。
        ///     BlendShape付きは表情アニメーションの対象になるため統合しません。
        /// </summary>
        private static bool IsMergeable(SkinnedMeshRenderer renderer)
        {
            Mesh mesh = renderer.sharedMesh;
            if (!renderer.enabled || mesh == null || mesh.blendShapeCount > 0 || mesh.subMeshCount != 1)
            {
                return false;
            }
            return renderer.rootBone != null && renderer.sharedMaterials.All(material => material != null);
        }

        /// <summary>
        ///     描画結果が変わらずに統合できる条件を文字列にします。
        /// </summary>
        private static string BuildGroupKey(SkinnedMeshRenderer renderer)
        {
            string materials = string.Join(",", renderer.sharedMaterials.Select(material => material.GetInstanceID()));
            return $"{renderer.rootBone.GetInstanceID()}|{materials}|{renderer.shadowCastingMode}|{renderer.receiveShadows}"
                + $"|{renderer.gameObject.layer}|{renderer.lightProbeUsage}|{renderer.motionVectorGenerationMode}"
                + $"|{renderer.allowOcclusionWhenDynamic}";
        }

        /// <summary>
        ///     1グループを統合し、統合Rendererの追加と元Rendererの無効化を行います。
        /// </summary>
        private static bool TryMergeGroup(GameObject instance, List<SkinnedMeshRenderer> group, string outputFolder, string meshName)
        {
            Mesh combined = SkinnedMeshCombiner.Combine(group, meshName, out Transform[] bones);
            if (combined == null)
            {
                return false;
            }
            Mesh saved = SaveMesh(combined, $"{outputFolder}/{meshName}.asset");

            SkinnedMeshRenderer first = group[0];
            GameObject mergedObject = new(meshName);
            mergedObject.transform.SetParent(instance.transform, false);
            mergedObject.layer = first.gameObject.layer;

            SkinnedMeshRenderer merged = mergedObject.AddComponent<SkinnedMeshRenderer>();
            merged.sharedMesh = saved;
            merged.bones = bones;
            merged.rootBone = first.rootBone;
            merged.sharedMaterials = first.sharedMaterials;
            merged.shadowCastingMode = first.shadowCastingMode;
            merged.receiveShadows = first.receiveShadows;
            merged.lightProbeUsage = first.lightProbeUsage;
            merged.motionVectorGenerationMode = first.motionVectorGenerationMode;
            merged.allowOcclusionWhenDynamic = first.allowOcclusionWhenDynamic;
            merged.updateWhenOffscreen = group.Any(source => source.updateWhenOffscreen);
            merged.localBounds = UnionLocalBounds(group);

            foreach (SkinnedMeshRenderer source in group)
            {
                source.enabled = false;
                // アニメーションのm_Enabledカーブで再有効化されても描画されないよう、GameObjectごと止める。
                // Rendererを載せたGameObjectに子が無い前提のため、子を持つ場合は止めない。
                if (source.transform.childCount == 0)
                {
                    source.gameObject.SetActive(false);
                }
            }
            return true;
        }

        /// <summary>
        ///     統合メッシュをアセットとして保存します。既存アセットがあれば、GUIDを保って内容を上書きします。
        /// </summary>
        private static Mesh SaveMesh(Mesh mesh, string path)
        {
            Mesh existing = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (existing != null)
            {
                EditorUtility.CopySerialized(mesh, existing);
                Object.DestroyImmediate(mesh);
                mesh = existing;
            }
            else
            {
                AssetDatabase.CreateAsset(mesh, path);
            }

            // 実行時にCPUへ頂点を残さないよう、読み取り不可にする。
            SerializedObject serialized = new(mesh);
            serialized.FindProperty("m_IsReadable").boolValue = false;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return mesh;
        }

        /// <summary>
        ///     グループ内のRendererのローカル境界を包含する境界を返します。
        /// </summary>
        private static Bounds UnionLocalBounds(List<SkinnedMeshRenderer> group)
        {
            Bounds bounds = group[0].localBounds;
            for (int i = 1; i < group.Count; i++)
            {
                bounds.Encapsulate(group[i].localBounds);
            }
            return bounds;
        }
    }
}
