using UnityEngine;

namespace KillChord.Runtime.Adaptor.InGame.Enemy
{
    /// <summary>
    ///     敵・ボス共通で、AIファサードから攻撃の予約と状態を扱うためのコントローラー。
    /// </summary>
    public interface IEnemyAIController
    {
        /// <summary> 攻撃中か。 </summary>
        public bool IsAttacking { get; }

        /// <summary>
        ///     攻撃を予約する。
        /// </summary>
        public void ReserveAttack();

        /// <summary>
        ///     進行中の攻撃をキャンセルする。
        /// </summary>
        public void CancelAttack();

        /// <summary>
        ///     プレイヤーが攻撃範囲内か取得する。
        /// </summary>
        /// <param name="enemyPosition"> 自分の位置。 </param>
        /// <param name="targetPosition"> 攻撃対象の位置。 </param>
        /// <returns> 攻撃範囲内なら true。 </returns>
        public bool IsPlayerInAttackRange(Vector3 enemyPosition, Vector3 targetPosition);
    }
}
