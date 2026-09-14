using KillChord.Runtime.InfraStructure.OutGame.StageSelect;
using UnityEditor;

namespace KillChord.Editor.Inspectors.SourceData
{
    /// <summary>
    ///     StageTreeAssetの既定項目とステージグラフをInspectorへ表示する。
    /// </summary>
    [CustomEditor(typeof(StageTreeAsset))]
    internal sealed class StageTreeAssetEditor : UnityEditor.Editor
    {
        /// <summary>
        ///     StageTreeAssetのInspectorを描画する。
        /// </summary>
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            EditorGUILayout.Space();
            StageTreeGraphView.Draw((StageTreeAsset)target);
        }
    }
}
