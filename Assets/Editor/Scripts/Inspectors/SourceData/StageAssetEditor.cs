using KillChord.Editor.SourceDataProvider.Core;
using KillChord.Runtime.InfraStructure.OutGame.StageSelect;
using UnityEditor;
using UnityEngine;

namespace KillChord.Editor.Inspectors.SourceData
{
    /// <summary>
    ///     StageAssetBase派生アセットの既定項目に加えて、所属するStageTreeAssetのグラフを
    ///     選択中ステージがハイライトされた状態で表示する。
    /// </summary>
    [CustomEditor(typeof(StageAssetBase), true)]
    internal sealed class StageAssetEditor : UnityEditor.Editor
    {
        /// <summary>
        ///     StageAssetBase派生アセットのInspectorを描画する。
        /// </summary>
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            EditorGUILayout.Space();

            if (!SourceDataProviderRepositoryResolver.TryResolveAsset(
                    STAGE_TREE_ADDRESSABLE_KEY,
                    out ScriptableObject stageTreeAsset))
            {
                EditorGUILayout.HelpBox(
                    $"所属するStageTreeAsset(「{STAGE_TREE_ADDRESSABLE_KEY}」)を解決できません。",
                    MessageType.None);
                return;
            }

            StageTreeGraphView.Draw(stageTreeAsset);
        }

        private const string STAGE_TREE_ADDRESSABLE_KEY = "StageTreeAsset";
    }
}
