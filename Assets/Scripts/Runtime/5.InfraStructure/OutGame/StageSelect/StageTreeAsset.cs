using KillChord.Runtime.Domain.OutGame.StageSelect;
using KillChord.Runtime.Utility.Constant;
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
                nodes.Add(_nodeAssets[i].Create());
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

        /// <summary>
        ///     インスペクター入力用の接続データ。
        /// </summary>
        [System.Serializable]
        private class StageNodeConnectionData
        {
            public int FromStageId => _fromStageId;
            public int ToStageId => _toStageId;

            [Tooltip("接続元のステージID。")]
            [SerializeField]
            private int _fromStageId;

            [Tooltip("接続先のステージID。")]
            [SerializeField]
            private int _toStageId;
        }
    }
}