using KillChord.Runtime.Domain.OutGame.StageSelect;
using KillChord.Runtime.Utility.Constant;
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
        /// <summary> 接続元ステージ。 </summary>
        public StageAssetBase FromStage => _fromStage;
        /// <summary> 接続先ステージ。 </summary>
        public StageAssetBase ToStage => _toStage;
        /// <summary> 接続元ステージ完了後の進行方法。 </summary>
        public StageAdvanceMode AdvanceMode => _advanceMode;

        /// <summary>
        ///     Domainの接続情報を生成する。
        /// </summary>
        /// <returns> 生成した接続情報。</returns>
        public StageNodeConnection Create()
        {
            if (_fromStage == null || _toStage == null)
            {
                throw new System.InvalidOperationException(
                    $"[{nameof(StageBindAsset)}] FromStageまたはToStageが未設定です。Asset: {name}");
            }

            return new StageNodeConnection(
                new StageId(_fromStage.StageIdValue),
                new StageId(_toStage.StageIdValue),
                _advanceMode);
        }

        [SerializeField, Tooltip("接続元のステージアセット。")]
        private StageAssetBase _fromStage;

        [SerializeField, Tooltip("接続先のステージアセット。")]
        private StageAssetBase _toStage;

        [SerializeField, Tooltip("接続元ステージ完了後の進行方法。")]
        private StageAdvanceMode _advanceMode = StageAdvanceMode.ManualSelection;
    }
}
