using UnityEngine;

namespace KillChord.Runtime.Adaptor
{
    /// <summary>
    ///     敵AI用ファサード：戦闘系。
    /// </summary>
    public interface IEnemyBattleAIFacade
    {
        /// <summary>
        ///     指示：目標に攻撃行動を開始する。
        /// </summary>
        public void StartAttack();
        /// <summary>
        ///     指示：被弾硬直アニメーションを開始する。
        /// </summary>
        public void StartStunAnimation();
        /// <summary>
        ///     指示：進行中の攻撃をキャンセルする。
        /// </summary>
        public void CancelAttack();
    }
}
