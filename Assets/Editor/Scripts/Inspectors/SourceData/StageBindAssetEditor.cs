using KillChord.Editor.SourceDataProvider.Core;
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
    internal sealed class StageBindAssetEditor : UnityEditor.Editor
    {
        /// <summary>
        ///     StageBindAssetのInspectorを描画する。
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
