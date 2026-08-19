using KillChord.Runtime.View.InGame.Enemy;
using System;
using System.Collections.Generic;
using UnityEditor;

namespace KillChord.Editor.SourceDataProvider
{
    /// <summary>
    ///     SourceDataProviderへ登録しない、シーン内で完結するCollectionKeyのDataID候補を解決します。
    /// </summary>
    internal static class SceneScopedDataIDOptionResolver
    {
        /// <summary>
        ///     指定カテゴリのシーン内DataID候補を取得します。
        /// </summary>
        /// <param name="collectionKey"> 取得するCollectionKeyです。 </param>
        /// <param name="serializedObject"> DataIDを保持するSerializedObjectです。 </param>
        /// <returns> 解決できた候補一覧です。解決できない場合は空です。 </returns>
        public static IReadOnlyList<SourceDataIDOption> GetOptions(
            string collectionKey,
            SerializedObject serializedObject)
        {
            if (serializedObject == null
                || !string.Equals(
                    collectionKey,
                    SpawnPositionPair.SPAWN_POINT_COLLECTION_KEY,
                    StringComparison.Ordinal))
            {
                return Array.Empty<SourceDataIDOption>();
            }

            // スポーン地点はWave定義が対象とするシーンから列挙する。
            if (!TryGetBattleSceneName(serializedObject, out string battleSceneName))
            {
                return Array.Empty<SourceDataIDOption>();
            }

            if (!BattleSceneDataReader.TryRead(
                    battleSceneName,
                    out BattleSceneDataReader.BattleSceneMapData mapData,
                    out _))
            {
                return Array.Empty<SourceDataIDOption>();
            }

            IReadOnlyList<BattleSceneDataReader.SpawnPointInfo> spawnPoints = mapData.SpawnPoints;
            List<SourceDataIDOption> options = new(spawnPoints.Count);
            for (int i = 0; i < spawnPoints.Count; i++)
            {
                options.Add(new SourceDataIDOption(
                    spawnPoints[i].Id,
                    spawnPoints[i].HashId,
                    serializedObject.targetObject));
            }

            return options;
        }

        /// <summary>
        ///     SerializedObjectから対象のバトルステージシーン名を取得します。
        /// </summary>
        /// <param name="serializedObject"> 読み取り対象です。 </param>
        /// <param name="battleSceneName"> 取得したシーン名です。 </param>
        /// <returns> 取得できた場合はtrueです。 </returns>
        private static bool TryGetBattleSceneName(
            SerializedObject serializedObject,
            out string battleSceneName)
        {
            battleSceneName = null;
            SerializedProperty sceneNameProperty =
                serializedObject.FindProperty(BATTLE_SCENE_NAME_PROPERTY_NAME);
            if (sceneNameProperty == null
                || sceneNameProperty.propertyType != SerializedPropertyType.String
                || string.IsNullOrWhiteSpace(sceneNameProperty.stringValue))
            {
                return false;
            }

            battleSceneName = sceneNameProperty.stringValue;
            return true;
        }

        private const string BATTLE_SCENE_NAME_PROPERTY_NAME = "_battleSceneName";
    }
}
