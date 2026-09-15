using KillChord.Runtime.InfraStructure.InGame.Character;
using UnityEditor;

namespace KillChord.Editor.Inspectors.SourceData
{
    /// <summary>
    ///     CharacterDefinitionAssetの通常Inspectorにレーダーグラフプレビューを追加します。
    /// </summary>
    [CustomEditor(typeof(CharacterDefinitionAsset))]
    [CanEditMultipleObjects]
    internal sealed class CharacterDefinitionAssetEditor : UnityEditor.Editor
    {
        /// <summary>
        ///     通常の編集項目と敵ステータスのレーダーグラフを描画します。
        ///     レーダーグラフはserializedObject(単一target)前提のため、単一選択時のみ描画します。
        ///     複数選択時は通常のInspectorのみを描画し、標準の一括編集を維持します。
        /// </summary>
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            if (targets.Length != 1)
            {
                return;
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Preview", EditorStyles.boldLabel);
            serializedObject.Update();
            EnemyStatusRadarChart.Draw(serializedObject);
        }
    }
}
