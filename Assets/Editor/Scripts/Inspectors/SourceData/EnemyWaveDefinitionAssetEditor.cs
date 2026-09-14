using KillChord.Runtime.InfraStructure.InGame.Enemy;
using UnityEditor;

namespace KillChord.Editor.Inspectors.SourceData
{
    /// <summary>
    ///     敵Wave定義アセットの標準項目とWave概要をInspectorへ表示します。
    /// </summary>
    [CustomEditor(typeof(EnemyWaveDefinitionAsset))]
    internal sealed class EnemyWaveDefinitionAssetEditor : UnityEditor.Editor
    {
        /// <summary>
        ///     敵Wave定義アセットのInspectorを描画します。
        /// </summary>
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            serializedObject.Update();
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Wave Summary", EditorStyles.boldLabel);
            EnemyWaveSummaryDrawer.DrawWaveSummary(serializedObject, target);
        }
    }
}
