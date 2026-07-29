using KillChord.Editor.Utility;
using KillChord.Runtime.Utility.Identity;
using KillChord.Runtime.View.InGame.Enemy;
using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace KillChord.Editor.SourceDataProvider
{
    /// <summary>
    ///     ステージシーン内のSpawnPositionPairへ、GameObject名に基づいたIDを一括付与します。
    /// </summary>
    public static class SpawnPositionPairIndexMigrationMenu
    {
        /// <summary>
        ///     対象シーン配下の全SpawnPositionPairへIDを付与します。
        /// </summary>
        [MenuItem(ToolConst.TOOLS_PATH + "Source Data Provider/Migrate SpawnPositionPair Ids")]
        public static void MigrateSpawnPositionPairIds()
        {
            if (!Application.isBatchMode
                && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                return;
            }

            SceneSetup[] originalSceneSetup = EditorSceneManager.GetSceneManagerSetup();
            int migratedCount = 0;
            int skippedCount = 0;
            int errorCount = 0;

            try
            {
                string[] guids = AssetDatabase.FindAssets(
                    "t:Scene",
                    new[] { STAGE_SCENES_DIRECTORY });
                for (int i = 0; i < guids.Length; i++)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                    MigrateScene(path, ref migratedCount, ref skippedCount, ref errorCount);
                }
            }
            finally
            {
                if (originalSceneSetup.Length > 0)
                {
                    EditorSceneManager.RestoreSceneManagerSetup(originalSceneSetup);
                }
            }

            string message =
                $"SpawnPositionPairのID移行が完了しました。付与: {migratedCount}, スキップ: {skippedCount}, エラー: {errorCount}";
            Debug.Log($"[{nameof(SpawnPositionPairIndexMigrationMenu)}] {message}");

            if (!Application.isBatchMode)
            {
                EditorUtility.DisplayDialog("Migrate SpawnPositionPair Ids", message, "OK");
            }
        }

        /// <summary>
        ///     1シーン分のSpawnPositionPairへIDを付与します。
        /// </summary>
        /// <param name="scenePath"> 対象シーンのアセットパスです。 </param>
        /// <param name="migratedCount"> 付与件数です。 </param>
        /// <param name="skippedCount"> スキップ件数です。 </param>
        /// <param name="errorCount"> エラー件数です。 </param>
        private static void MigrateScene(
            string scenePath,
            ref int migratedCount,
            ref int skippedCount,
            ref int errorCount)
        {
            Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            SpawnPositionPair[] pairs = UnityEngine.Object.FindObjectsByType<SpawnPositionPair>(
                FindObjectsSortMode.None);
            if (pairs.Length == 0)
            {
                return;
            }

            bool changed = false;
            foreach (SpawnPositionPair pair in pairs)
            {
                if (pair.SpawnPointId.Id != 0)
                {
                    skippedCount++;
                    continue;
                }

                string candidateId = pair.gameObject.name;
                if (!IsUniqueInScene(pairs, pair, candidateId))
                {
                    Debug.LogError(
                        $"[{nameof(SpawnPositionPairIndexMigrationMenu)}] "
                        + $"GameObject名が重複しているため移行できません。Scene: {scenePath}, Name: {candidateId}",
                        pair);
                    errorCount++;
                    continue;
                }

                SerializedObject serializedObject = new(pair);
                SerializedProperty spawnPointIdProperty = serializedObject.FindProperty(SPAWN_POINT_ID_PROPERTY_NAME);
                spawnPointIdProperty.FindPropertyRelative(ID_PROPERTY_NAME).stringValue = candidateId;
                spawnPointIdProperty.FindPropertyRelative(HASH_PROPERTY_NAME).intValue =
                    DataIDHasher.Compute(SpawnPositionPair.SPAWN_POINT_COLLECTION_KEY, candidateId);
                serializedObject.ApplyModifiedPropertiesWithoutUndo();

                migratedCount++;
                changed = true;
            }

            if (changed)
            {
                EditorSceneManager.SaveScene(scene);
            }
        }

        /// <summary>
        ///     GameObject名がシーン内で一意かどうかを判定します。
        /// </summary>
        /// <param name="pairs"> シーン内の全SpawnPositionPairです。 </param>
        /// <param name="self"> 検証対象自身です。 </param>
        /// <param name="candidateId"> 検証する名前です。 </param>
        /// <returns> 一意な場合はtrueです。 </returns>
        private static bool IsUniqueInScene(
            SpawnPositionPair[] pairs,
            SpawnPositionPair self,
            string candidateId)
        {
            int matchCount = 0;
            foreach (SpawnPositionPair pair in pairs)
            {
                if (string.Equals(pair.gameObject.name, candidateId, StringComparison.Ordinal))
                {
                    matchCount++;
                }
            }

            return matchCount == 1;
        }

        private const string STAGE_SCENES_DIRECTORY = "Assets/Level/Scenes/Master/Stages";
        private const string SPAWN_POINT_ID_PROPERTY_NAME = "_spawnPointId";
        private const string ID_PROPERTY_NAME = "_id";
        private const string HASH_PROPERTY_NAME = "_hashId";
    }
}
