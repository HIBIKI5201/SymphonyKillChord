using KillChord.Runtime.Adaptor;
using KillChord.Runtime.Adaptor.InGame.Enemy;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.Enemy
{
    /// <summary>
    ///     敵AI用ファサード：戦闘系
    /// </summary>
    public class EnemyBattleAIFacade : MonoBehaviour, IEnemyBattleAIFacade
    {
        /// <summary>
        ///     初期化処理。
        /// </summary>
        /// <param name="aIController"></param>
        public void Initialize(EnemyAIController aIController)
        {
            _aiController = aIController;
        }

        /// <summary>
        ///     指示：目標に攻撃行動を開始する。
        /// </summary>
        public void StartAttack()
        {
            _aiController.ReserveAttack();
        }

        /// <summary>
        ///     指示：被弾硬直アニメーションを開始する。
        /// </summary>
        public void StartStunAnimation()
        {
            Debug.Log("被弾アニメーション実装待ち");
        }

        /// <summary>
        ///     指示：進行中の攻撃をキャンセルする。
        /// </summary>
        public void CancelAttack()
        {
            _aiController.CancelAttack();
        }

        private EnemyAIController _aiController;
    }
}
