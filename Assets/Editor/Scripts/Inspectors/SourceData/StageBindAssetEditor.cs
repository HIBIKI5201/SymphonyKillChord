using KillChord.Runtime.InfraStructure.OutGame.StageSelect;
using UnityEditor;
using UnityEngine;

namespace KillChord.Editor.Inspectors.SourceData
{
    /// <summary>
    ///     StageBindAssetの既定項目に加えて、所属するStageTreeAssetのグラフを
    ///     選択中Bindがどの接続かわかる状態で表示する。
    /// </summary>
    [CustomEditor(typeof(StageBindAsset))]
    [CanEditMultipleObjects]
    internal sealed class StageBindAssetEditor : UnityEditor.Editor
    {
        /// <summary>
        ///     StageBindAssetのInspectorを描画する。
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

            if (!StageTreeGraphView.TryFindContainingStageTree(
                    STAGE_TREE_ADDRESSABLE_KEY,
                    (ScriptableObject)target,
                    out ScriptableObject stageTreeAsset,
                    out string message))
            {
                EditorGUILayout.HelpBox(message, MessageType.None);
                return;
            }

            StageTreeGraphView.Draw(stageTreeAsset, (ScriptableObject)target);
        }

        private const string STAGE_TREE_ADDRESSABLE_KEY = "StageTreeAsset";
    }
}
