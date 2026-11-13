using KillChord.Runtime.Adaptor.InGame.Enemy;
using KillChord.Runtime.Domain.InGame.Mission.StepEntryAction;
using System;

namespace KillChord.Runtime.Adaptor.InGame.Mission
{
    /// <summary>
    ///     目標ステップ進入時アクションの実行クラス：<br/>
    ///     敵の無敵状態を切り替える。
    /// </summary>
    public class SetEnemyInvincibleStepEntryActionExecutor : IMissionStepEntryActionExecutor
    {
        /// <summary>
        ///     敵の無敵状態の登録簿を指定して生成する。
        /// </summary>
        public SetEnemyInvincibleStepEntryActionExecutor(EnemyInvincibilityRegistry invincibilityRegistry)
        {
            _invincibilityRegistry = invincibilityRegistry ??
                throw new ArgumentNullException(nameof(invincibilityRegistry), "[SetEnemyInvincibleStepEntryActionExecutor] EnemyInvincibilityRegistryがNULL。");
        }

        /// <inheritdoc />
        public Type EntryActionType => typeof(SetEnemyInvincibleStepEntryAction);

        /// <inheritdoc />
        public void Execute(IMissionStepEntryAction entryAction)
        {
            if (entryAction is not SetEnemyInvincibleStepEntryAction action)
            {
                throw new ArgumentException(
                    $"{nameof(entryAction)}の型が{nameof(SetEnemyInvincibleStepEntryAction)}ではない。", nameof(entryAction));
            }
            _invincibilityRegistry.SetInvincible(action.IsInvincible);
        }

        private readonly EnemyInvincibilityRegistry _invincibilityRegistry;
    }
}
