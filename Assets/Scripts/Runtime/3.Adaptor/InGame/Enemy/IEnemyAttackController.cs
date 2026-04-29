using UnityEngine;

namespace KillChord.Runtime.Adaptor.InGame.Enemy
{
    /// <summary>
    ///     敵の攻撃処理コントローラー。
    /// </summary>
    public interface IEnemyAttackController
    {
        /// <summary>
        ///     攻撃実行。
        /// </summary>
        public void ExecuteAttack();
    }
}
