using KillChord.Runtime.Adaptor.InGame.Enemy;
using KillChord.Runtime.Adaptor.InGame.Enemy.EnemyAIFacadeInterface;
using KillChord.Runtime.View.InGame.Sequence;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.Enemy.AIFacade
{
    /// <summary>
    ///     敵AI用ファサード：戦闘系
    /// </summary>
    public class EnemyBattleAIFacade : MonoBehaviour, IEnemyBattleAIFacade, IGameplayControllable
    {
        /// <summary>
        ///     初期化処理。
        /// </summary>
        /// <param name="aIController"></param>
        public void Initialize(EnemyAIController aIController)
        {
            _aiController = aIController;
            _isPlaying = false;
        }

        /// <summary>
        ///     指示：目標に攻撃行動を開始する。
        /// </summary>
        public void StartAttack()
        {
            if (!_isPlaying) return;

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

        /// <summary>
        ///    ゲームプレイの開始処理を行います。
        /// </summary>
        public void StartGameplay()
        {
            _isPlaying = true;
        }

        /// <summary>
        ///     ゲームプレイの停止処理を行います。
        /// </summary>
        public void StopGameplay()
        {
            _isPlaying = false;
            CancelAttack();
        }

        private EnemyAIController _aiController;
        private bool _isPlaying;
    }
}
