using KillChord.Runtime.Domain.OutGame.StageSelect;
using System;

namespace KillChord.Runtime.Adaptor.InGame.StageSelect
{
    /// <summary>
    ///     選択されているバトルステージの状態を管理するクラス。
    /// </summary>
    public class SelectedBattleStageState
    {
        /// <summary> 現在選択されているステージ定義。 </summary>
        public StageDefinition CurrentStageDefinition
        {
            get
            {
                if (!HasSelectedBattleStage)
                {
                    throw new InvalidOperationException("バトルシーンが選択されていません");
                }

                return _currentBattleStageDefinition;
            }
        }

        /// <summary> 現在選択されているステージ名。 </summary>
        public string StageName =>
            CurrentStageDefinition.StageName;

        /// <summary> 現在選択されているステージのターゲットシーン名。 </summary>
        public string InGameSceneName =>
            CurrentStageDefinition.TargetSceneName;

        /// <summary> 現在選択されているステージのバトルシーン名。 </summary>
        public string BattleSceneName =>
            CurrentStageDefinition.BattleSceneName;

        /// <summary> 戦闘終了後に戻るシーン名。 </summary>
        public string ReturnSceneName
        {
            get
            {
                if (!HasSelectedBattleStage)
                {
                    throw new InvalidOperationException("バトルシーンが選択されていません");
                }

                return _returnSceneName;
            }
        }

        /// <summary> バトルステージが選択されているか。 </summary>
        public bool HasSelectedBattleStage 
            => _currentBattleStageDefinition != null;


        /// <summary>
        ///     バトルステージを選択する。
        /// </summary>
        /// <param name="stageDefinition"> 選択するステージ定義。 </param>
        /// <param name="returnSceneName"> 戦闘終了後に戻るOutGameシーン名。 </param>
        public void SelectBattleStage(StageDefinition stageDefinition, string returnSceneName)
        {
            if (stageDefinition == null)
            {
                throw new ArgumentNullException(nameof(stageDefinition));
            }

            if (stageDefinition.StageType != StageType.Battle)
            {
                throw new ArgumentException($"ステージタイプがBattleではありません。StageType={stageDefinition.StageType}", nameof(stageDefinition));
            }

            if (string.IsNullOrWhiteSpace(stageDefinition.BattleSceneName))
            {
                throw new ArgumentException("バトルシーン名が設定されていません。", nameof(stageDefinition));
            }

            if (string.IsNullOrWhiteSpace(returnSceneName))
            {
                throw new ArgumentException("帰還先シーン名が設定されていません", nameof(returnSceneName));
            }

            _currentBattleStageDefinition = stageDefinition;
            _returnSceneName = returnSceneName;
        }

        /// <summary>
        ///     選択状態をクリアする。
        /// </summary>
        public void Clear()
        {
            _currentBattleStageDefinition = null;
            _returnSceneName = string.Empty;
        }

        private StageDefinition _currentBattleStageDefinition = null;
        private string _returnSceneName = string.Empty;
    }
}
