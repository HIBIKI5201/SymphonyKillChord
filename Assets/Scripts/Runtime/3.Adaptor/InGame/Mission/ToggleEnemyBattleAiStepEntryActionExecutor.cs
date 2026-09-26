using KillChord.Runtime.Adaptor.InGame.Enemy;
using KillChord.Runtime.Domain.InGame.Mission.StepEntryAction;
using System;

namespace KillChord.Runtime.Adaptor.InGame.Mission
{
    /// <summary>
    ///     目標ステップ進入時アクションの実行クラス：<br/>
    ///     敵の戦闘AIを切り替える。
    /// </summary>
    public class ToggleEnemyBattleAiStepEntryActionExecutor : IMissionStepEntryActionExecutor
    {
        public ToggleEnemyBattleAiStepEntryActionExecutor(EnemyAIControllerRegistry battleAIRegistry)
        {
            _enemyBattleAIRegistry = battleAIRegistry ?? 
                throw new ArgumentNullException(nameof(battleAIRegistry), "[ToggleEnemyBattleAiStepEntryActionExecutor] EnemyBattleAIRegistryがNULL。");
        }
        /// <inheritdoc />
        public Type EntryActionType => typeof(ToggleEnemyBattleAIStepEntryAction);

        /// <inheritdoc />
        public void Execute(IMissionStepEntryAction entryAction)
        {
            if(entryAction is not ToggleEnemyBattleAIStepEntryAction)
            {
                throw new ArgumentException(
                    $"{nameof(entryAction)}の型が{nameof(ToggleEnemyBattleAIStepEntryAction)}ではない。", nameof(entryAction));
            }
            _enemyBattleAIRegistry.SetBattleAiActivated(
                ((ToggleEnemyBattleAIStepEntryAction)entryAction).IsBattleAiActivated);
        }
        
        private EnemyAIControllerRegistry _enemyBattleAIRegistry;
    }
}
