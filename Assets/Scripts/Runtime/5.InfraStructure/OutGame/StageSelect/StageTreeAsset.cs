using KillChord.Runtime.Domain.OutGame.StageSelect;
using KillChord.Runtime.Utility.Constant;
using KillChord.Runtime.Utility.Identity;
using System.Collections.Generic;
using UnityEngine;

namespace KillChord.Runtime.InfraStructure.OutGame.StageSelect
{
    /// <summary>
    ///     ステージツリー全体の定義を保持するアセットクラス。
    /// </summary>
    [CreateAssetMenu(
        fileName = nameof(StageTreeAsset),
        menuName = PathConst.CREATE_ASSET_MENU_PATH + "StageSelect/" + nameof(StageTreeAsset))]
    public class StageTreeAsset : ScriptableObject
    {
        /// <summary>
        ///     ステージツリーを生成します。
        /// </summary>
        /// <returns> 生成されたステージツリー。</returns>
        public StageTree Create()
        {
            var nodes = new List<StageNode>(_nodeAssets.Count);
            for (var i = 0; i < _nodeAssets.Count; i++)
            {
                if (_nodeAssets[i] == null)
                {
#if UNITY_EDITOR
                    Debug.LogWarning($"[{nameof(StageTreeAsset)}] インデックス {i} のノードアセットが null です。", this);
#endif
                    continue;
                }
                StageNode node = _nodeAssets[i].Create();
                if (node != null)
                {
                    nodes.Add(node);
                }
            }

            var connections = new List<StageNodeConnection>(_connections.Count);
            for (var i = 0; i < _connections.Count; i++)
            {
                if (_connections[i].FromStageId == 0 ||
                    _connections[i].ToStageId == 0)
                {
#if UNITY_EDITOR
                    Debug.LogWarning(
                        $"[{nameof(StageTreeAsset)}] インデックス {i} の接続データに空の StageId があります。スキップします。", this);
#endif
                    continue;
                }

                connections.Add(new StageNodeConnection(
                    new StageId(_connections[i].FromStageId),
                    new StageId(_connections[i].ToStageId)));
            }

            return new StageTree(nodes, connections);
        }

        [Header("ノード一覧")]
        [SerializeField, Tooltip("このツリーに含まれるステージノードアセットの一覧。")]
        private List<StageNodeAsset> _nodeAssets = new();

        [Header("接続情報")]
        [SerializeField, Tooltip("ノード間の接続情報。")]
        private List<StageNodeConnectionData> _connections = new();

#if UNITY_EDITOR
        /// <summary>
        ///     ステージIDの重複をインスペクター入力時に検証します。
        /// </summary>
        private void OnValidate()
        {
            HashSet<int> stageIds = new();
            int tutorialCount = 0;
            for (int i = 0; i < _nodeAssets.Count; i++)
            {
                StageNodeAsset nodeAsset = _nodeAssets[i];
                if (nodeAsset == null)
                {
                    continue;
                }

                if (nodeAsset.StageIdValue == 0)
                {
                    Debug.LogError(
                        $"[{nameof(StageTreeAsset)}] StageIdが未設定です。Index: {i}",
                        this);
                    continue;
                }

                if (nodeAsset.IsTutorial)
                {
                    tutorialCount++;
                }

                if (stageIds.Add(nodeAsset.StageIdValue))
                {
                    continue;
                }

                Debug.LogError(
                    $"[{nameof(StageTreeAsset)}] StageIdが重複しています。StageId: {nodeAsset.StageIdValue}",
                    this);
            }

            if (tutorialCount > 1)
            {
                Debug.LogError(
                    $"[{nameof(StageTreeAsset)}] チュートリアルステージが複数設定されています。",
                    this);
            }
        }
#endif

        /// <summary>
        ///     インスペクター入力用の接続データ。
        /// </summary>
        [System.Serializable]
        private class StageNodeConnectionData
        {
            public int FromStageId => _fromStageId.Id;
            public int ToStageId => _toStageId.Id;

            [Tooltip("接続元のステージID。")]
            [SerializeField, DataCategory("Stage")]
            private DataID _fromStageId;

            [Tooltip("接続先のステージID。")]
            [SerializeField, DataCategory("Stage")]
            private DataID _toStageId;
        }
    }
}
