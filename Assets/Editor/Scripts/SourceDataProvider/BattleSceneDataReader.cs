using KillChord.Runtime.View.InGame.Enemy;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using Unity.AI.Navigation;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

namespace KillChord.Editor.SourceDataProvider
{
    /// <summary>
    ///     ステージシーンの.unityファイルを直接パースし、スポーンポイントとNavMeshの情報を取得します。
    ///     シーンを開かずに取得するため、現在開いているシーンへ影響を与えません。
    /// </summary>
    internal static class BattleSceneDataReader
    {
        /// <summary>
        ///     1スポーンポイント分の情報です。
        /// </summary>
        public readonly struct SpawnPointInfo
        {
            /// <summary> 人間可読なIDです。 </summary>
            public readonly string Id;
            /// <summary> 実行時に使用する数値IDです。 </summary>
            public readonly int HashId;
            /// <summary> 場外の出現位置（ワールド座標）です。 </summary>
            public readonly Vector3 SpawnPosition;
            /// <summary> 入場演出の移動目的地（ワールド座標）です。 </summary>
            public readonly Vector3 EntryPosition;

            public SpawnPointInfo(string id, int hashId, Vector3 spawnPosition, Vector3 entryPosition)
            {
                Id = id;
                HashId = hashId;
                SpawnPosition = spawnPosition;
                EntryPosition = entryPosition;
            }
        }

        /// <summary>
        ///     NavMeshの三角形メッシュ情報です。
        /// </summary>
        public readonly struct NavMeshMapData
        {
            /// <summary> 頂点一覧（ワールド座標）です。 </summary>
            public readonly Vector3[] Vertices;
            /// <summary> 三角形インデックス一覧です。 </summary>
            public readonly int[] Indices;

            public NavMeshMapData(Vector3[] vertices, int[] indices)
            {
                Vertices = vertices;
                Indices = indices;
            }
        }

        /// <summary>
        ///     1シーン分のマップ情報です。
        /// </summary>
        public sealed class BattleSceneMapData
        {
            /// <summary> スポーンポイント一覧です。 </summary>
            public IReadOnlyList<SpawnPointInfo> SpawnPoints { get; internal set; } = Array.Empty<SpawnPointInfo>();
            /// <summary> NavMeshの三角形メッシュ情報です。 </summary>
            public NavMeshMapData NavMesh { get; internal set; }
            /// <summary> NavMeshを取得できた場合はtrueです。 </summary>
            public bool HasNavMesh { get; internal set; }
        }

        /// <summary>
        ///     指定シーンのマップ情報を取得します。結果はキャッシュされます。
        /// </summary>
        /// <param name="sceneName"> 対象シーン名（拡張子なし）です。 </param>
        /// <param name="mapData"> 取得したマップ情報です。 </param>
        /// <param name="error"> 取得できなかった場合の理由です。 </param>
        /// <returns> 取得できた場合はtrueです。スポーンポイントやNavMeshが0件でも、シーン自体を解決できればtrueを返します。 </returns>
        public static bool TryRead(string sceneName, out BattleSceneMapData mapData, out string error)
        {
            mapData = null;
            error = string.Empty;

            if (string.IsNullOrWhiteSpace(sceneName))
            {
                error = "シーン名が指定されていません。";
                return false;
            }

            if (_cache.TryGetValue(sceneName, out BattleSceneMapData cached))
            {
                mapData = cached;
                return true;
            }

            if (!TryResolveScenePath(sceneName, out string scenePath))
            {
                error = $"シーン「{sceneName}」がBuild Settingsに登録されていません。";
                return false;
            }

            string sceneText;
            try
            {
                sceneText = File.ReadAllText(scenePath);
            }
            catch (Exception exception)
            {
                error = $"シーンファイルの読み込みに失敗しました: {exception.Message}";
                return false;
            }

            List<SceneDocument> documents = ParseDocuments(sceneText);
            List<SpawnPointInfo> spawnPoints = ReadSpawnPoints(documents, out string spawnPointError);

            bool hasNavMesh = TryReadNavMesh(documents, out NavMeshMapData navMesh, out string navMeshError);

            error = !string.IsNullOrEmpty(spawnPointError)
                ? spawnPointError
                : (!hasNavMesh ? navMeshError : string.Empty);

            mapData = new BattleSceneMapData
            {
                SpawnPoints = spawnPoints,
                NavMesh = navMesh,
                HasNavMesh = hasNavMesh,
            };
            _cache[sceneName] = mapData;
            return true;
        }

        /// <summary>
        ///     キャッシュを破棄します。Planner Windowの「Refresh」操作から呼び出します。
        /// </summary>
        public static void ClearCache()
        {
            _cache.Clear();
        }

        /// <summary>
        ///     シーン名からBuild Settings登録済みのシーンパスを解決します。
        /// </summary>
        /// <param name="sceneName"> 対象シーン名（拡張子なし）です。 </param>
        /// <param name="scenePath"> 解決したシーンパスです。 </param>
        /// <returns> 解決できた場合はtrueです。 </returns>
        private static bool TryResolveScenePath(string sceneName, out string scenePath)
        {
            EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
            for (int i = 0; i < scenes.Length; i++)
            {
                if (string.Equals(
                        Path.GetFileNameWithoutExtension(scenes[i].path),
                        sceneName,
                        StringComparison.Ordinal))
                {
                    scenePath = scenes[i].path;
                    return true;
                }
            }

            scenePath = null;
            return false;
        }

        /// <summary>
        ///     シーンテキストからSpawnPositionPairの情報を取得します。
        /// </summary>
        /// <param name="documents"> パース済みドキュメント一覧です。 </param>
        /// <param name="error"> 取得できなかった項目がある場合の警告文です。 </param>
        /// <returns> 取得したスポーンポイント一覧です。 </returns>
        private static List<SpawnPointInfo> ReadSpawnPoints(List<SceneDocument> documents, out string error)
        {
            error = string.Empty;
            List<SpawnPointInfo> result = new();

            GameObject prefabRoot = AssetDatabase.LoadAssetAtPath<GameObject>(POSITION_PAIR_PREFAB_PATH);
            if (prefabRoot == null)
            {
                error = $"PositionPairプレハブが見つかりません。Path: {POSITION_PAIR_PREFAB_PATH}";
                return result;
            }

            SpawnPositionPair prefabComponent = prefabRoot.GetComponent<SpawnPositionPair>();
            if (prefabComponent == null
                || prefabComponent.SpawnPosition == null
                || prefabComponent.EntryPosition == null)
            {
                error = "PositionPairプレハブの構成が想定と異なります。";
                return result;
            }

            string positionPairGuid = AssetDatabase.AssetPathToGUID(POSITION_PAIR_PREFAB_PATH);
            AssetDatabase.TryGetGUIDAndLocalFileIdentifier(prefabRoot.transform, out _, out long rootFileId);
            AssetDatabase.TryGetGUIDAndLocalFileIdentifier(
                prefabComponent.SpawnPosition, out _, out long spawnPosFileId);
            AssetDatabase.TryGetGUIDAndLocalFileIdentifier(
                prefabComponent.EntryPosition, out _, out long entryPosFileId);
            AssetDatabase.TryGetGUIDAndLocalFileIdentifier(prefabComponent, out _, out long componentFileId);

            Vector3 rootDefaultPosition = prefabRoot.transform.localPosition;
            Quaternion rootDefaultRotation = prefabRoot.transform.localRotation;
            Vector3 spawnDefaultPosition = prefabComponent.SpawnPosition.localPosition;
            Vector3 entryDefaultPosition = prefabComponent.EntryPosition.localPosition;

            Dictionary<long, PlainTransform> plainTransforms = BuildPlainTransforms(documents);
            List<PrefabInstanceDoc> instances = BuildPrefabInstances(documents, positionPairGuid);

            bool hasUnmigrated = false;
            for (int i = 0; i < instances.Count; i++)
            {
                PrefabInstanceDoc instanceDoc = instances[i];

                string id = GetOverriddenString(
                    instanceDoc.Modifications, componentFileId, SPAWN_POINT_ID_PROPERTY_PATH, null);
                int hashId = GetOverriddenInt(
                    instanceDoc.Modifications, componentFileId, SPAWN_POINT_HASH_PROPERTY_PATH, 0);
                if (string.IsNullOrEmpty(id) || hashId == 0)
                {
                    hasUnmigrated = true;
                    continue;
                }

                Vector3 rootLocalPosition = GetOverriddenVector3(
                    instanceDoc.Modifications, rootFileId, LOCAL_POSITION_PROPERTY_PREFIX, rootDefaultPosition);
                Quaternion rootLocalRotation = GetOverriddenQuaternion(
                    instanceDoc.Modifications, rootFileId, LOCAL_ROTATION_PROPERTY_PREFIX, rootDefaultRotation);
                Vector3 spawnLocalPosition = GetOverriddenVector3(
                    instanceDoc.Modifications, spawnPosFileId, LOCAL_POSITION_PROPERTY_PREFIX, spawnDefaultPosition);
                Vector3 entryLocalPosition = GetOverriddenVector3(
                    instanceDoc.Modifications, entryPosFileId, LOCAL_POSITION_PROPERTY_PREFIX, entryDefaultPosition);

                Matrix4x4 parentWorld = ResolveWorldMatrix(
                    instanceDoc.TransformParentFileId, plainTransforms, MAX_PARENT_DEPTH);
                Matrix4x4 rootLocal = Matrix4x4.TRS(rootLocalPosition, rootLocalRotation, Vector3.one);
                Matrix4x4 rootWorld = parentWorld * rootLocal;

                Vector3 spawnWorldPosition = rootWorld.MultiplyPoint3x4(spawnLocalPosition);
                Vector3 entryWorldPosition = rootWorld.MultiplyPoint3x4(entryLocalPosition);

                result.Add(new SpawnPointInfo(id, hashId, spawnWorldPosition, entryWorldPosition));
            }

            if (hasUnmigrated)
            {
                error = "IDが未設定のSpawnPositionPairがあります。"
                    + "Tools/Source Data Provider/Migrate SpawnPositionPair Idsを実行してください。";
            }

            return result;
        }

        /// <summary>
        ///     シーンからNavMeshのトライアンギュレーションを取得します。
        /// </summary>
        /// <param name="documents"> パース済みドキュメント一覧です。 </param>
        /// <param name="navMesh"> 取得したNavMeshデータです。 </param>
        /// <param name="error"> 取得できなかった場合の理由です。 </param>
        /// <returns> 取得できた場合はtrueです。 </returns>
        private static bool TryReadNavMesh(
            List<SceneDocument> documents,
            out NavMeshMapData navMesh,
            out string error)
        {
            navMesh = default;
            error = string.Empty;

            if (EditorApplication.isPlaying)
            {
                error = "Play Mode中はNavMeshを取得できません。";
                return false;
            }

            string navMeshSurfaceScriptGuid = FindScriptGuid(typeof(NavMeshSurface));
            if (string.IsNullOrEmpty(navMeshSurfaceScriptGuid))
            {
                error = "NavMeshSurfaceスクリプトが見つかりません。";
                return false;
            }

            List<NavMeshSurfaceInfo> surfaces =
                ReadNavMeshSurfaces(documents, navMeshSurfaceScriptGuid, out int unresolvedSurfaceCount);
            if (unresolvedSurfaceCount > 0)
            {
                error = $"Transformを解決できないNavMeshSurfaceが{unresolvedSurfaceCount}件あります。"
                    + "prefabインスタンス配下のNavMeshSurfaceには対応していません。";
                return false;
            }

            if (surfaces.Count == 0)
            {
                error = "シーンにNavMeshSurfaceが見つかりません。";
                return false;
            }

            List<NavMeshData> navMeshDataAssets = new(surfaces.Count);
            List<NavMeshSurfaceInfo> resolvedSurfaces = new(surfaces.Count);
            for (int i = 0; i < surfaces.Count; i++)
            {
                string navMeshDataPath = AssetDatabase.GUIDToAssetPath(surfaces[i].NavMeshDataGuid);
                NavMeshData navMeshDataAsset = AssetDatabase.LoadAssetAtPath<NavMeshData>(navMeshDataPath);
                if (navMeshDataAsset == null)
                {
                    error = $"NavMeshDataアセットを読み込めません。Path: {navMeshDataPath}";
                    return false;
                }

                navMeshDataAssets.Add(navMeshDataAsset);
                resolvedSurfaces.Add(surfaces[i]);
            }

            // CalculateTriangulationは現在ロード中の全NavMeshを返すため、
            // 追加前後の差分を取って対象シーン分だけを抽出する。
            NavMeshTriangulation before = NavMesh.CalculateTriangulation();

            List<NavMeshDataInstance> instances = new(navMeshDataAssets.Count);
            try
            {
                for (int i = 0; i < navMeshDataAssets.Count; i++)
                {
                    // NavMeshSurfaceはSurfaceのTransformを基準にベイク結果を配置するため、同じ変換を適用する。
                    instances.Add(NavMesh.AddNavMeshData(
                        navMeshDataAssets[i],
                        resolvedSurfaces[i].Position,
                        resolvedSurfaces[i].Rotation));
                }

                NavMeshTriangulation after = NavMesh.CalculateTriangulation();
                if (!TryExtractAddedTriangulation(before, after, out navMesh))
                {
                    error = "対象シーンのNavMeshを取得できませんでした。"
                        + "NavMeshが未ベイクであるか、対象シーンを既に開いている可能性があります。";
                    return false;
                }

                return true;
            }
            finally
            {
                for (int i = 0; i < instances.Count; i++)
                {
                    if (instances[i].valid)
                    {
                        NavMesh.RemoveNavMeshData(instances[i]);
                    }
                }
            }
        }

        /// <summary>
        ///     シーンテキストからNavMeshSurfaceの情報を収集します。
        /// </summary>
        /// <param name="documents"> パース済みドキュメント一覧です。 </param>
        /// <param name="navMeshSurfaceScriptGuid"> NavMeshSurfaceスクリプトのGUIDです。 </param>
        /// <param name="unresolvedSurfaceCount"> Transformを解決できず除外したSurface数です。 </param>
        /// <returns> NavMeshDataを持ち、Transformを解決できたNavMeshSurface一覧です。 </returns>
        private static List<NavMeshSurfaceInfo> ReadNavMeshSurfaces(
            List<SceneDocument> documents,
            string navMeshSurfaceScriptGuid,
            out int unresolvedSurfaceCount)
        {
            unresolvedSurfaceCount = 0;
            List<NavMeshSurfaceInfo> result = new();
            Dictionary<long, PlainTransform> plainTransforms = BuildPlainTransforms(documents);
            Dictionary<long, long> gameObjectToTransform = BuildGameObjectToTransformMap(documents);

            for (int i = 0; i < documents.Count; i++)
            {
                SceneDocument doc = documents[i];
                if (doc.TypeId != MONO_BEHAVIOUR_TYPE_ID || doc.Stripped)
                {
                    continue;
                }

                string scriptGuid = ParseGuid(doc.Body, SCRIPT_FIELD_NAME);
                if (!string.Equals(scriptGuid, navMeshSurfaceScriptGuid, StringComparison.Ordinal))
                {
                    continue;
                }

                string navMeshDataGuid = ParseGuid(doc.Body, NAV_MESH_DATA_FIELD_NAME);
                if (string.IsNullOrEmpty(navMeshDataGuid))
                {
                    continue;
                }

                long gameObjectFileId = ParseFileId(doc.Body, GAME_OBJECT_FIELD_NAME) ?? 0;
                if (!gameObjectToTransform.TryGetValue(gameObjectFileId, out long transformFileId)
                    || !TryResolveWorldMatrix(transformFileId, plainTransforms, MAX_PARENT_DEPTH, out Matrix4x4 world))
                {
                    // Transformを解決できないSurfaceは、誤配置のNavMeshを描画しないよう除外する。
                    unresolvedSurfaceCount++;
                    continue;
                }

                result.Add(new NavMeshSurfaceInfo(navMeshDataGuid, world.GetColumn(3), world.rotation));
            }

            return result;
        }

        /// <summary>
        ///     GameObjectのfileIDから、そのGameObjectが持つTransformのfileIDへの対応表を作成します。
        /// </summary>
        /// <param name="documents"> パース済みドキュメント一覧です。 </param>
        /// <returns> GameObject fileIDをキーとしたTransform fileID一覧です。 </returns>
        private static Dictionary<long, long> BuildGameObjectToTransformMap(List<SceneDocument> documents)
        {
            Dictionary<long, long> result = new();
            for (int i = 0; i < documents.Count; i++)
            {
                SceneDocument doc = documents[i];
                if (doc.TypeId != TRANSFORM_TYPE_ID || doc.Stripped)
                {
                    continue;
                }

                long gameObjectFileId = ParseFileId(doc.Body, GAME_OBJECT_FIELD_NAME) ?? 0;
                if (gameObjectFileId == 0)
                {
                    continue;
                }

                result[gameObjectFileId] = doc.Anchor;
            }

            return result;
        }

        /// <summary>
        ///     追加前後のトライアンギュレーション差分から、追加分のみを抽出します。
        /// </summary>
        /// <param name="before"> NavMeshData追加前のトライアンギュレーションです。 </param>
        /// <param name="after"> NavMeshData追加後のトライアンギュレーションです。 </param>
        /// <param name="navMesh"> 抽出したNavMeshデータです。 </param>
        /// <returns> 抽出できた場合はtrueです。 </returns>
        private static bool TryExtractAddedTriangulation(
            NavMeshTriangulation before,
            NavMeshTriangulation after,
            out NavMeshMapData navMesh)
        {
            navMesh = default;

            Vector3[] beforeVertices = before.vertices ?? Array.Empty<Vector3>();
            int[] beforeIndices = before.indices ?? Array.Empty<int>();
            int baseVertexCount = beforeVertices.Length;
            int baseIndexCount = beforeIndices.Length;

            Vector3[] afterVertices = after.vertices;
            int[] afterIndices = after.indices;
            if (afterVertices == null
                || afterIndices == null
                || afterVertices.Length < baseVertexCount
                || afterIndices.Length < baseIndexCount)
            {
                return false;
            }

            // 追加分が末尾へ追記されるというAPI上の保証はないため、
            // 先頭が追加前と完全に一致することを確認できた場合のみ末尾を追加分として扱う。
            if (!HasSamePrefix(beforeVertices, afterVertices) || !HasSamePrefix(beforeIndices, afterIndices))
            {
                return false;
            }

            int addedVertexCount = afterVertices.Length - baseVertexCount;
            int addedIndexCount = afterIndices.Length - baseIndexCount;
            if (addedVertexCount == 0 || addedIndexCount == 0)
            {
                return false;
            }

            Vector3[] vertices = new Vector3[addedVertexCount];
            Array.Copy(afterVertices, baseVertexCount, vertices, 0, addedVertexCount);

            int[] indices = new int[addedIndexCount];
            for (int i = 0; i < addedIndexCount; i++)
            {
                int rebased = afterIndices[baseIndexCount + i] - baseVertexCount;
                if (rebased < 0 || rebased >= addedVertexCount)
                {
                    // 追加分が末尾へ追記されていない場合は分離できないため、誤った形状を返さず失敗させる。
                    return false;
                }

                indices[i] = rebased;
            }

            navMesh = new NavMeshMapData(vertices, indices);
            return true;
        }

        /// <summary>
        ///     配列の先頭が、指定した配列と完全に一致するか判定します。
        /// </summary>
        /// <typeparam name="T"> 要素型です。 </typeparam>
        /// <param name="prefix"> 先頭に一致することを期待する配列です。 </param>
        /// <param name="source"> 判定対象の配列です。 </param>
        /// <returns> 一致する場合はtrueです。 </returns>
        private static bool HasSamePrefix<T>(T[] prefix, T[] source)
            where T : IEquatable<T>
        {
            for (int i = 0; i < prefix.Length; i++)
            {
                if (!prefix[i].Equals(source[i]))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        ///     指定型のスクリプトアセットGUIDを検索します。
        /// </summary>
        /// <param name="type"> 検索対象の型です。 </param>
        /// <returns> 見つかった場合はGUID、それ以外はnullです。 </returns>
        private static string FindScriptGuid(Type type)
        {
            string[] guids = AssetDatabase.FindAssets($"t:MonoScript {type.Name}");
            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                MonoScript script = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
                if (script != null && script.GetClass() == type)
                {
                    return guids[i];
                }
            }

            return null;
        }

        /// <summary>
        ///     指定fileIDのワールド行列を、親チェーンを辿って合成します。
        /// </summary>
        /// <param name="fileId"> 対象のfileIDです。0はシーンルートを表します。 </param>
        /// <param name="plainTransforms"> シーン内のプレーンTransform一覧です。 </param>
        /// <param name="depthGuard"> 循環参照防止用の残り探索深さです。 </param>
        /// <returns> ワールド行列です。親を解決できない場合は単位行列を基準に途中まで合成した結果です。 </returns>
        private static Matrix4x4 ResolveWorldMatrix(
            long fileId,
            Dictionary<long, PlainTransform> plainTransforms,
            int depthGuard)
        {
            if (fileId == 0 || depthGuard <= 0 || !plainTransforms.TryGetValue(fileId, out PlainTransform t))
            {
                return Matrix4x4.identity;
            }

            Matrix4x4 local = Matrix4x4.TRS(t.LocalPosition, t.LocalRotation, t.LocalScale);
            Matrix4x4 parentWorld = ResolveWorldMatrix(t.FatherFileId, plainTransforms, depthGuard - 1);
            return parentWorld * local;
        }

        /// <summary>
        ///     指定fileIDのワールド行列を、親チェーンを辿って合成します。
        ///     途中のTransformを解決できない場合は失敗させます。
        /// </summary>
        /// <param name="fileId"> 対象のfileIDです。0はシーンルートを表します。 </param>
        /// <param name="plainTransforms"> シーン内のプレーンTransform一覧です。 </param>
        /// <param name="depthGuard"> 循環参照防止用の残り探索深さです。 </param>
        /// <param name="world"> 解決したワールド行列です。 </param>
        /// <returns> 解決できた場合はtrueです。 </returns>
        private static bool TryResolveWorldMatrix(
            long fileId,
            Dictionary<long, PlainTransform> plainTransforms,
            int depthGuard,
            out Matrix4x4 world)
        {
            world = Matrix4x4.identity;

            // シーンルートへ到達したため、これ以上の合成は不要。
            if (fileId == 0)
            {
                return true;
            }

            // prefabインスタンス由来のstripped Transformなどは解決できないため、
            // 誤った座標のNavMeshを返さないよう失敗させる。
            if (depthGuard <= 0 || !plainTransforms.TryGetValue(fileId, out PlainTransform t))
            {
                return false;
            }

            if (!TryResolveWorldMatrix(t.FatherFileId, plainTransforms, depthGuard - 1, out Matrix4x4 parentWorld))
            {
                return false;
            }

            world = parentWorld * Matrix4x4.TRS(t.LocalPosition, t.LocalRotation, t.LocalScale);
            return true;
        }

        /// <summary>
        ///     シーン内のプレーン（非stripped）Transformを収集します。
        /// </summary>
        /// <param name="documents"> パース済みドキュメント一覧です。 </param>
        /// <returns> fileIDをキーとしたTransform一覧です。 </returns>
        private static Dictionary<long, PlainTransform> BuildPlainTransforms(List<SceneDocument> documents)
        {
            Dictionary<long, PlainTransform> result = new();
            for (int i = 0; i < documents.Count; i++)
            {
                SceneDocument doc = documents[i];
                if (doc.TypeId != TRANSFORM_TYPE_ID || doc.Stripped)
                {
                    continue;
                }

                Vector3 localPosition = ParseVector3(doc.Body, LOCAL_POSITION_FIELD_NAME) ?? Vector3.zero;
                Quaternion localRotation = ParseQuaternion(doc.Body, LOCAL_ROTATION_FIELD_NAME) ?? Quaternion.identity;
                Vector3 localScale = ParseVector3(doc.Body, LOCAL_SCALE_FIELD_NAME) ?? Vector3.one;
                long fatherFileId = ParseFileId(doc.Body, FATHER_FIELD_NAME) ?? 0;
                result[doc.Anchor] = new PlainTransform(localPosition, localRotation, localScale, fatherFileId);
            }

            return result;
        }

        /// <summary>
        ///     指定プレハブguidを参照するPrefabInstanceを収集します。
        /// </summary>
        /// <param name="documents"> パース済みドキュメント一覧です。 </param>
        /// <param name="sourcePrefabGuid"> 対象プレハブのguidです。 </param>
        /// <returns> 該当するPrefabInstance一覧です。 </returns>
        private static List<PrefabInstanceDoc> BuildPrefabInstances(
            List<SceneDocument> documents,
            string sourcePrefabGuid)
        {
            List<PrefabInstanceDoc> result = new();
            for (int i = 0; i < documents.Count; i++)
            {
                SceneDocument doc = documents[i];
                if (doc.TypeId != PREFAB_INSTANCE_TYPE_ID)
                {
                    continue;
                }

                string docSourcePrefabGuid = ParseGuid(doc.Body, SOURCE_PREFAB_FIELD_NAME);
                if (!string.Equals(docSourcePrefabGuid, sourcePrefabGuid, StringComparison.Ordinal))
                {
                    continue;
                }

                long transformParentFileId = ParseFileId(doc.Body, TRANSFORM_PARENT_FIELD_NAME) ?? 0;
                List<ModificationEntry> modifications = ParseModifications(doc.Body);
                result.Add(new PrefabInstanceDoc(doc.Anchor, transformParentFileId, modifications));
            }

            return result;
        }

        /// <summary>
        ///     PrefabInstanceのm_Modificationsを解析します。
        /// </summary>
        /// <param name="body"> PrefabInstanceドキュメントの本文です。 </param>
        /// <returns> 解析した上書きエントリ一覧です。 </returns>
        private static List<ModificationEntry> ParseModifications(string body)
        {
            List<ModificationEntry> result = new();
            MatchCollection matches = ModificationRegex.Matches(body);
            foreach (Match match in matches)
            {
                long targetFileId = long.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
                string propertyPath = match.Groups[2].Value.Trim();
                string value = match.Groups[3].Value.Trim();
                result.Add(new ModificationEntry(targetFileId, propertyPath, value));
            }

            return result;
        }

        /// <summary>
        ///     シーンテキストをドキュメント単位へ分割します。
        /// </summary>
        /// <param name="sceneText"> シーンファイルのテキストです。 </param>
        /// <returns> 分割したドキュメント一覧です。 </returns>
        private static List<SceneDocument> ParseDocuments(string sceneText)
        {
            List<SceneDocument> documents = new();
            MatchCollection matches = DocumentHeaderRegex.Matches(sceneText);
            for (int i = 0; i < matches.Count; i++)
            {
                Match match = matches[i];
                int typeId = int.Parse(match.Groups["type"].Value, CultureInfo.InvariantCulture);
                long anchor = long.Parse(match.Groups["anchor"].Value, CultureInfo.InvariantCulture);
                bool stripped = match.Groups["stripped"].Success;
                string className = match.Groups["class"].Value;
                int bodyStart = match.Index + match.Length;
                int bodyEnd = i + 1 < matches.Count ? matches[i + 1].Index : sceneText.Length;
                string body = sceneText.Substring(bodyStart, bodyEnd - bodyStart);
                documents.Add(new SceneDocument(typeId, anchor, stripped, className, body));
            }

            return documents;
        }

        private static Vector3 GetOverriddenVector3(
            List<ModificationEntry> modifications,
            long targetFileId,
            string propertyPrefix,
            Vector3 defaultValue)
        {
            float x = GetOverriddenFloat(modifications, targetFileId, propertyPrefix + ".x", defaultValue.x);
            float y = GetOverriddenFloat(modifications, targetFileId, propertyPrefix + ".y", defaultValue.y);
            float z = GetOverriddenFloat(modifications, targetFileId, propertyPrefix + ".z", defaultValue.z);
            return new Vector3(x, y, z);
        }

        private static Quaternion GetOverriddenQuaternion(
            List<ModificationEntry> modifications,
            long targetFileId,
            string propertyPrefix,
            Quaternion defaultValue)
        {
            float x = GetOverriddenFloat(modifications, targetFileId, propertyPrefix + ".x", defaultValue.x);
            float y = GetOverriddenFloat(modifications, targetFileId, propertyPrefix + ".y", defaultValue.y);
            float z = GetOverriddenFloat(modifications, targetFileId, propertyPrefix + ".z", defaultValue.z);
            float w = GetOverriddenFloat(modifications, targetFileId, propertyPrefix + ".w", defaultValue.w);
            return new Quaternion(x, y, z, w);
        }

        private static float GetOverriddenFloat(
            List<ModificationEntry> modifications,
            long targetFileId,
            string propertyPath,
            float defaultValue)
        {
            for (int i = 0; i < modifications.Count; i++)
            {
                if (modifications[i].TargetFileId != targetFileId
                    || !string.Equals(modifications[i].PropertyPath, propertyPath, StringComparison.Ordinal))
                {
                    continue;
                }

                if (float.TryParse(
                        modifications[i].Value,
                        NumberStyles.Float,
                        CultureInfo.InvariantCulture,
                        out float parsed))
                {
                    return parsed;
                }
            }

            return defaultValue;
        }

        private static int GetOverriddenInt(
            List<ModificationEntry> modifications,
            long targetFileId,
            string propertyPath,
            int defaultValue)
        {
            for (int i = 0; i < modifications.Count; i++)
            {
                if (modifications[i].TargetFileId != targetFileId
                    || !string.Equals(modifications[i].PropertyPath, propertyPath, StringComparison.Ordinal))
                {
                    continue;
                }

                if (int.TryParse(modifications[i].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int parsed))
                {
                    return parsed;
                }
            }

            return defaultValue;
        }

        private static string GetOverriddenString(
            List<ModificationEntry> modifications,
            long targetFileId,
            string propertyPath,
            string defaultValue)
        {
            for (int i = 0; i < modifications.Count; i++)
            {
                if (modifications[i].TargetFileId == targetFileId
                    && string.Equals(modifications[i].PropertyPath, propertyPath, StringComparison.Ordinal))
                {
                    return modifications[i].Value;
                }
            }

            return defaultValue;
        }

        private static Vector3? ParseVector3(string body, string fieldName)
        {
            Match match = Regex.Match(
                body,
                Regex.Escape(fieldName) + @":\s*\{x:\s*([-\d.eE+]+),\s*y:\s*([-\d.eE+]+),\s*z:\s*([-\d.eE+]+)\}");
            if (!match.Success)
            {
                return null;
            }

            return new Vector3(
                float.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture),
                float.Parse(match.Groups[2].Value, CultureInfo.InvariantCulture),
                float.Parse(match.Groups[3].Value, CultureInfo.InvariantCulture));
        }

        private static Quaternion? ParseQuaternion(string body, string fieldName)
        {
            Match match = Regex.Match(
                body,
                Regex.Escape(fieldName)
                    + @":\s*\{x:\s*([-\d.eE+]+),\s*y:\s*([-\d.eE+]+),\s*z:\s*([-\d.eE+]+),\s*w:\s*([-\d.eE+]+)\}");
            if (!match.Success)
            {
                return null;
            }

            return new Quaternion(
                float.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture),
                float.Parse(match.Groups[2].Value, CultureInfo.InvariantCulture),
                float.Parse(match.Groups[3].Value, CultureInfo.InvariantCulture),
                float.Parse(match.Groups[4].Value, CultureInfo.InvariantCulture));
        }

        private static long? ParseFileId(string body, string fieldName)
        {
            Match match = Regex.Match(body, Regex.Escape(fieldName) + @":\s*\{fileID:\s*(-?\d+)");
            return match.Success ? long.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture) : null;
        }

        private static string ParseGuid(string body, string fieldName)
        {
            Match match = Regex.Match(body, Regex.Escape(fieldName) + @":\s*\{[^}]*guid:\s*([0-9a-fA-F]+)");
            return match.Success ? match.Groups[1].Value : null;
        }

        /// <summary>
        ///     シーンテキストの1ドキュメント分を表します。
        /// </summary>
        private readonly struct SceneDocument
        {
            public readonly int TypeId;
            public readonly long Anchor;
            public readonly bool Stripped;
            public readonly string ClassName;
            public readonly string Body;

            public SceneDocument(int typeId, long anchor, bool stripped, string className, string body)
            {
                TypeId = typeId;
                Anchor = anchor;
                Stripped = stripped;
                ClassName = className;
                Body = body;
            }
        }

        /// <summary>
        ///     プレーンTransformのローカル情報です。
        /// </summary>
        private readonly struct PlainTransform
        {
            public readonly Vector3 LocalPosition;
            public readonly Quaternion LocalRotation;
            public readonly Vector3 LocalScale;
            public readonly long FatherFileId;

            public PlainTransform(
                Vector3 localPosition,
                Quaternion localRotation,
                Vector3 localScale,
                long fatherFileId)
            {
                LocalPosition = localPosition;
                LocalRotation = localRotation;
                LocalScale = localScale;
                FatherFileId = fatherFileId;
            }
        }

        /// <summary>
        ///     PrefabInstanceの上書き1件分です。
        /// </summary>
        private readonly struct ModificationEntry
        {
            public readonly long TargetFileId;
            public readonly string PropertyPath;
            public readonly string Value;

            public ModificationEntry(long targetFileId, string propertyPath, string value)
            {
                TargetFileId = targetFileId;
                PropertyPath = propertyPath;
                Value = value;
            }
        }

        /// <summary>
        ///     シーン内のNavMeshSurface1件分です。
        /// </summary>
        private readonly struct NavMeshSurfaceInfo
        {
            public readonly string NavMeshDataGuid;
            public readonly Vector3 Position;
            public readonly Quaternion Rotation;

            public NavMeshSurfaceInfo(string navMeshDataGuid, Vector3 position, Quaternion rotation)
            {
                NavMeshDataGuid = navMeshDataGuid;
                Position = position;
                Rotation = rotation;
            }
        }

        /// <summary>
        ///     PositionPairプレハブのPrefabInstance1件分です。
        /// </summary>
        private readonly struct PrefabInstanceDoc
        {
            public readonly long Anchor;
            public readonly long TransformParentFileId;
            public readonly List<ModificationEntry> Modifications;

            public PrefabInstanceDoc(long anchor, long transformParentFileId, List<ModificationEntry> modifications)
            {
                Anchor = anchor;
                TransformParentFileId = transformParentFileId;
                Modifications = modifications;
            }
        }

        private static readonly Dictionary<string, BattleSceneMapData> _cache = new(StringComparer.Ordinal);

        private static readonly Regex DocumentHeaderRegex = new(
            @"^--- !u!(?<type>\d+) &(?<anchor>-?\d+)(?<stripped> stripped)?\r?\n(?<class>\w+):",
            RegexOptions.Multiline | RegexOptions.Compiled);

        private static readonly Regex ModificationRegex = new(
            @"-\s*target:\s*\{fileID:\s*(-?\d+)[^}]*\}\r?\n\s*propertyPath:\s*(.*?)\r?\n\s*value:\s*(.*?)\r?\n",
            RegexOptions.Compiled);

        private const int TRANSFORM_TYPE_ID = 4;
        private const int MONO_BEHAVIOUR_TYPE_ID = 114;
        private const int PREFAB_INSTANCE_TYPE_ID = 1001;
        private const int MAX_PARENT_DEPTH = 32;

        private const string POSITION_PAIR_PREFAB_PATH = "Assets/Level/Prefabs/Develop/PositionPair.prefab";
        private const string SPAWN_POINT_ID_PROPERTY_PATH = "_spawnPointId._id";
        private const string SPAWN_POINT_HASH_PROPERTY_PATH = "_spawnPointId._hashId";
        private const string LOCAL_POSITION_PROPERTY_PREFIX = "m_LocalPosition";
        private const string LOCAL_ROTATION_PROPERTY_PREFIX = "m_LocalRotation";
        private const string LOCAL_POSITION_FIELD_NAME = "m_LocalPosition";
        private const string LOCAL_ROTATION_FIELD_NAME = "m_LocalRotation";
        private const string LOCAL_SCALE_FIELD_NAME = "m_LocalScale";
        private const string FATHER_FIELD_NAME = "m_Father";
        private const string TRANSFORM_PARENT_FIELD_NAME = "m_TransformParent";
        private const string SOURCE_PREFAB_FIELD_NAME = "m_SourcePrefab";
        private const string SCRIPT_FIELD_NAME = "m_Script";
        private const string NAV_MESH_DATA_FIELD_NAME = "m_NavMeshData";
        private const string GAME_OBJECT_FIELD_NAME = "m_GameObject";
    }
}
