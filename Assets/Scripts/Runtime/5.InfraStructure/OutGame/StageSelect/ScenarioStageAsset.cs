using KillChord.Runtime.Domain.OutGame.StageSelect;
using KillChord.Runtime.Utility.Constant;
using UnityEngine;

namespace KillChord.Runtime.InfraStructure.OutGame.StageSelect
{
    /// <summary>
    ///     シナリオステージ固有の入力情報を保持するアセットクラス。
    /// </summary>
    [CreateAssetMenu(
        fileName = nameof(ScenarioStageAsset),
        menuName = PathConst.CREATE_ASSET_MENU_PATH + "StageSelect/" + nameof(ScenarioStageAsset))]
    public sealed class ScenarioStageAsset : StageAssetBase
    {
        [Header("シナリオ情報")]
        [SerializeField, Tooltip("シナリオパートで再生するシナリオID。")]
        private string _scenarioId;

        /// <summary>
        ///     シナリオステージ定義を生成する。
        /// </summary>
        /// <param name="stageId"> ステージID。</param>
        /// <param name="stageName"> ステージ名。</param>
        /// <param name="flavorText"> フレーバーテキスト。</param>
        /// <param name="reward"> クリア報酬。</param>
        /// <param name="targetSceneName"> 遷移先シーン名。</param>
        /// <returns> 生成したシナリオステージ定義。</returns>
        protected override StageDefinition CreateDefinition(
            StageId stageId,
            string stageName,
            string flavorText,
            StageReward reward,
            string targetSceneName)
        {
            return new ScenarioStageDefinition(
                stageId,
                stageName,
                flavorText,
                reward,
                targetSceneName,
                _scenarioId);
        }
    }
}
