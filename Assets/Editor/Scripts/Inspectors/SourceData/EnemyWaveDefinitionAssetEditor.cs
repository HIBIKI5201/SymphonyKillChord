using KillChord.Editor.SourceDataProvider.Core;
using KillChord.Runtime.InfraStructure.InGame.Enemy;
using UnityEditor;
using UnityEngine;

namespace KillChord.Editor.Inspectors.SourceData
{
    /// <summary>
    ///     敵Wave定義アセットの標準項目とWave概要をInspectorへ表示します。
    /// </summary>
    [CustomEditor(typeof(EnemyWaveDefinitionAsset))]
    [CanEditMultipleObjects]
    internal sealed class EnemyWaveDefinitionAssetEditor : UnityEditor.Editor
    {
        /// <summary>
        ///     Inspectorが対象アセットを表示し始めるタイミングでシーンマップキャッシュを破棄します。
        ///     バトルシーン側でスポーンポイントを編集した後にこのアセットを選び直せば最新状態を反映します。
        /// </summary>
        private void OnEnable()
        {
            BattleSceneDataReader.ClearCache();
        }

        /// <summary>
        ///     敵Wave定義アセットのInspectorを描画します。
        ///     Wave概要/マップはserializedObject(単一target)前提のため、単一選択時のみ描画します。
        /// </summary>
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            if (targets.Length != 1)
            {
                return;
            }

            serializedObject.Update();
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Wave Summary", EditorStyles.boldLabel);
            if (GUILayout.Button("Refresh Map", GUILayout.Width(96f)))
            {
                BattleSceneDataReader.ClearCache();
            }
            EditorGUILayout.EndHorizontal();
            EnemyWaveSummaryDrawer.DrawWaveSummary(serializedObject, target);
        }
    }
}
