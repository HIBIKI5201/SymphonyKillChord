using KillChord.Runtime.Domain.OutGame.StageSelect;
using KillChord.Runtime.InfraStructure.InGame.Mission;
using KillChord.Runtime.Utility.Constant;
using SymphonyFrameWork.Attribute;
using UnityEngine;

namespace KillChord.Runtime.InfraStructure.OutGame.StageSelect
{
    /// <summary>
    ///     バトルステージ固有の入力情報を保持するアセットクラス。
    /// </summary>
    [CreateAssetMenu(
        fileName = nameof(BattleStageAsset),
        menuName = PathConst.CREATE_ASSET_MENU_PATH + "StageSelect/" + nameof(BattleStageAsset))]
    public sealed class BattleStageAsset : StageAssetBase
    {
        /// <summary> チュートリアルステージとして設定されている場合はtrue。 </summary>
        public override bool IsTutorial => _isTutorial;

        [Header("バトル情報")]
        [SerializeField, Tooltip("初回起動時に自動出撃するチュートリアルステージの場合はオンにする。")]
        private bool _isTutorial;

        [SerializeField, SceneNameSelector, Tooltip("バトルパートで追加ロードするステージシーン名。")]
        private string _battleSceneName = "Stage_1";

        [SerializeField, Tooltip("バトルパートで使用する敵Wave定義のAddressablesキー。")]
        private string _enemyWaveDefinitionAssetKey;

        [SerializeField, Tooltip("バトルパートのミッション定義アセット。")]
        private MissionDefinitionAsset _missionDefinitionAsset;

        /// <summary>
        ///     バトルステージ定義を生成する。
        /// </summary>
        /// <param name="stageId"> ステージID。</param>
        /// <param name="stageName"> ステージ名。</param>
        /// <param name="flavorText"> フレーバーテキスト。</param>
        /// <param name="reward"> クリア報酬。</param>
        /// <param name="targetSceneName"> 遷移先シーン名。</param>
        /// <returns> 生成したバトルステージ定義。</returns>
        protected override StageDefinition CreateDefinition(
            StageId stageId,
            string stageName,
            string flavorText,
            StageReward reward,
            string targetSceneName)
        {
            if (_missionDefinitionAsset == null)
            {
                throw new System.InvalidOperationException(
                    $"[{nameof(BattleStageAsset)}] ミッション定義が未設定です。StageId: {stageId.Value}");
            }

            return new BattleStageDefinition(
                stageId,
                stageName,
                flavorText,
                reward,
                targetSceneName,
                _battleSceneName,
                _missionDefinitionAsset.Create(),
                _isTutorial,
                _enemyWaveDefinitionAssetKey);
        }
    }
}
