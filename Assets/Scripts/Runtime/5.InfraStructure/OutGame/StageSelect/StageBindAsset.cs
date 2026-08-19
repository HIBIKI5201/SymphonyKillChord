using KillChord.Runtime.Domain.OutGame.StageSelect;
using KillChord.Runtime.Utility.Constant;
using KillChord.Runtime.Utility.Identity;
using UnityEngine;

namespace KillChord.Runtime.InfraStructure.OutGame.StageSelect
{
    /// <summary>
    ///     2つのステージと接続元完了後の進行方法を保持するアセットクラス。
    /// </summary>
    [CreateAssetMenu(
        fileName = nameof(StageBindAsset),
        menuName = PathConst.CREATE_ASSET_MENU_PATH + "StageSelect/" + nameof(StageBindAsset))]
    public sealed class StageBindAsset : ScriptableObject
    {
        /// <summary> 接続元ステージIDの入力値。 </summary>
        public int FromStageIdValue => _fromStageId.Id;
        /// <summary> 接続先ステージIDの入力値。 </summary>
        public int ToStageIdValue => _toStageId.Id;
        /// <summary> 接続元ステージ完了後の進行方法。 </summary>
        public StageAdvanceMode AdvanceMode => _advanceMode;

        /// <summary>
        ///     Domainの接続情報を生成する。
        /// </summary>
        /// <returns> 生成した接続情報。</returns>
        public StageNodeConnection Create()
        {
            if (_fromStageId.Id == 0 || _toStageId.Id == 0)
            {
                throw new System.InvalidOperationException(
                    $"[{nameof(StageBindAsset)}] FromStageまたはToStageが未設定です。Asset: {name}");
            }

            return new StageNodeConnection(
                new StageId(_fromStageId.Id),
                new StageId(_toStageId.Id),
                _advanceMode);
        }

        [SerializeField, Tooltip("接続元のステージID。")]
        [SourceDataCollection("StageAsset")]
        private DataID _fromStageId;

        [SerializeField, Tooltip("接続先のステージID。")]
        [SourceDataCollection("StageAsset")]
        private DataID _toStageId;

        [SerializeField, Tooltip("接続元ステージ完了後の進行方法。")]
        private StageAdvanceMode _advanceMode = StageAdvanceMode.ManualSelection;
    }
}
