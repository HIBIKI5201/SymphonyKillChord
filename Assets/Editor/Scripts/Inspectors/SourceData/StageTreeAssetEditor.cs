using KillChord.Runtime.InfraStructure.OutGame.StageSelect;
using UnityEditor;

namespace KillChord.Editor.Inspectors.SourceData
{
    /// <summary>
    ///     StageTreeAssetの既定項目とステージグラフをInspectorへ表示する。
    /// </summary>
    [CustomEditor(typeof(StageTreeAsset))]
    [CanEditMultipleObjects]
    internal sealed class StageTreeAssetEditor : UnityEditor.Editor
    {
        /// <summary>
        ///     StageTreeAssetのInspectorを描画する。
        ///     グラフプレビューはtarget/SerializedObjectの単数系idiomに依存するため、単一選択時のみ描画する。
        /// </summary>
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            if (targets.Length != 1)
            {
                return;
            }

            EditorGUILayout.Space();
            StageTreeGraphView.Draw((StageTreeAsset)target);
        }
    }
}
