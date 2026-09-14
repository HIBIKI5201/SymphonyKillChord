using KillChord.Runtime.InfraStructure.InGame.Character;
using UnityEditor;

namespace KillChord.Editor.Inspectors.SourceData
{
    /// <summary>
    ///     CharacterDefinitionAssetの通常Inspectorにレーダーグラフプレビューを追加します。
    /// </summary>
    [CustomEditor(typeof(CharacterDefinitionAsset))]
    internal sealed class CharacterDefinitionAssetEditor : UnityEditor.Editor
    {
        /// <summary>
        ///     通常の編集項目と敵ステータスのレーダーグラフを描画します。
        /// </summary>
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Preview", EditorStyles.boldLabel);
            serializedObject.Update();
            EnemyStatusRadarChart.Draw(serializedObject);
        }
    }
}
