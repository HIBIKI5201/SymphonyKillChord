using KillChord.Runtime.Utility.Identity;
using KillChord.Runtime.View.InGame.Enemy;
using UnityEditor;
using UnityEngine;

namespace KillChord.Editor.SourceDataProvider
{
    /// <summary>
    ///     SpawnPositionPairのIDをシーン内で一意に保つための専用Inspectorです。
    /// </summary>
    [CustomEditor(typeof(SpawnPositionPair))]
    internal sealed class SpawnPositionPairEditor : UnityEditor.Editor
    {
        /// <summary>
        ///     Inspectorを描画します。
        /// </summary>
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(serializedObject.FindProperty(SPAWN_POSITION_PROPERTY_NAME));
            EditorGUILayout.PropertyField(serializedObject.FindProperty(ENTRY_POSITION_PROPERTY_NAME));

            SerializedProperty spawnPointIdProperty = serializedObject.FindProperty(SPAWN_POINT_ID_PROPERTY_NAME);
            SerializedProperty idProperty = spawnPointIdProperty.FindPropertyRelative(ID_PROPERTY_NAME);
            SerializedProperty hashProperty = spawnPointIdProperty.FindPropertyRelative(HASH_PROPERTY_NAME);

            EditorGUI.BeginChangeCheck();
            string nextId = EditorGUILayout.TextField("Spawn Point Id", idProperty.stringValue);
            if (EditorGUI.EndChangeCheck())
            {
                idProperty.stringValue = nextId;
                hashProperty.intValue = DataIDHasher.Compute(SpawnPositionPair.SPAWN_POINT_COLLECTION_KEY, nextId);
            }

            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.IntField("Hash", hashProperty.intValue);
            }

            string warning = FindSceneLocalWarning(hashProperty.intValue);
            if (!string.IsNullOrEmpty(warning))
            {
                EditorGUILayout.HelpBox(warning, MessageType.Warning);
            }

            serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        ///     同一シーン内で使用中の他のSpawnPositionPairとIDが衝突していないか検証します。
        /// </summary>
        /// <param name="hashId"> 検証対象の数値IDです。 </param>
        /// <returns> 衝突がある場合は警告文、それ以外は空文字列です。 </returns>
        private string FindSceneLocalWarning(int hashId)
        {
            if (hashId == 0)
            {
                return "IDが未設定です。";
            }

            SpawnPositionPair[] allPairs = FindObjectsByType<SpawnPositionPair>(FindObjectsSortMode.None);
            int duplicateCount = 0;
            for (int i = 0; i < allPairs.Length; i++)
            {
                if (allPairs[i] == target)
                {
                    continue;
                }

                if (allPairs[i].SpawnPointId.Id == hashId)
                {
                    duplicateCount++;
                }
            }

            return duplicateCount > 0
                ? $"同一シーン内に同じIDのSpawnPositionPairが{duplicateCount}件あります。"
                : string.Empty;
        }

        private const string SPAWN_POSITION_PROPERTY_NAME = "_spawnPosition";
        private const string ENTRY_POSITION_PROPERTY_NAME = "_entryPosition";
        private const string SPAWN_POINT_ID_PROPERTY_NAME = "_spawnPointId";
        private const string ID_PROPERTY_NAME = "_id";
        private const string HASH_PROPERTY_NAME = "_hashId";
    }
}
