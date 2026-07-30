using KillChord.Runtime.Application.InGame.Enemy;
using KillChord.Runtime.Domain.InGame.Enemy;
using KillChord.Runtime.Domain.InGame.Mission;
using KillChord.Runtime.Domain.OutGame.StageSelect;
using KillChord.Runtime.Utility.Constant;
using KillChord.Runtime.Utility.Identity;
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

        [SerializeField, SourceDataCollection("Wave"), Tooltip("バトルパートで使用する敵Wave定義ID。スポーン候補地の都合により、ステージシーン名もこの定義が保持する。")]
        private DataID _enemyWaveDefinitionId;

        [SerializeField, SourceDataCollection("Mission"), Tooltip("バトルパートのミッションID。")]
        private DataID _missionDefinitionId;

        /// <summary>
        ///     バトルステージ定義を生成する。
        /// </summary>
        /// <param name="stageId"> ステージID。</param>
        /// <param name="stageName"> ステージ名。</param>
        /// <param name="flavorText"> フレーバーテキスト。</param>
        /// <param name="reward"> クリア報酬。</param>
        /// <param name="targetSceneName"> 遷移先シーン名。</param>
        /// <param name="waveDefinitionRepository"> バトルシーン名の解決に使う敵Wave定義リポジトリ。</param>
        /// <returns> 生成したバトルステージ定義。</returns>
        protected override StageDefinition CreateDefinition(
            StageId stageId,
            string stageName,
            string flavorText,
            StageReward reward,
            string targetSceneName,
            IEnemyWaveDefinitionRepository waveDefinitionRepository)
        {
            if (_missionDefinitionId.Id == 0)
            {
                throw new System.InvalidOperationException(
                    $"[{nameof(BattleStageAsset)}] ミッション定義が未設定です。StageId: {stageId.Value}");
            }

            if (waveDefinitionRepository == null)
            {
                throw new System.InvalidOperationException(
                    $"[{nameof(BattleStageAsset)}] 敵Wave定義リポジトリが未指定です。StageId: {stageId.Value}");
            }

            EnemyWaveDefinitionId enemyWaveDefinitionId = new(_enemyWaveDefinitionId.Id);
            if (!waveDefinitionRepository.TryGetBattleSceneName(
                    enemyWaveDefinitionId,
                    out string battleSceneName))
            {
                throw new System.InvalidOperationException(
                    $"[{nameof(BattleStageAsset)}] 敵Wave定義からバトルシーン名を解決できません。"
                        + $" StageId: {stageId.Value}, EnemyWaveDefinitionId: {_enemyWaveDefinitionId.Id}");
            }

            return new BattleStageDefinition(
                stageId,
                stageName,
                flavorText,
                reward,
                targetSceneName,
                battleSceneName,
                new MissionId(_missionDefinitionId.Id),
                _isTutorial,
                enemyWaveDefinitionId);
        }
    }
}
