using KillChord.Runtime.Adaptor.InGame.Enemy.EnemyAIFacadeInterface;
using KillChord.Runtime.View.InGame.Sequence;
using UnityEngine;

namespace DevelopProducts.Boss
{
    /// <summary>
    ///     ボスAI用ファサード：戦闘系。EnemyBattleAIFacade のボス専用複製。
    /// </summary>
    public class BossBattleAIFacade : MonoBehaviour, IEnemyBattleAIFacade, IGameplayControllable
    {
        /// <summary>
        ///     初期化処理。
        /// </summary>
        public void Initialize(BossAIController aiController)
        {
            _aiController = aiController;
            _isPlaying = false;
        }

        /// <summary> 指示：目標に攻撃行動を開始する。 </summary>
        public void StartAttack()
        {
            if (!_isPlaying || _aiController == null) return;
            _aiController.ReserveAttack();
        }

        /// <summary> 指示：被弾硬直アニメーションを開始する。 </summary>
        public void StartStunAnimation()
        {
            Debug.Log("被弾アニメーション実装待ち");
        }

        /// <summary> 指示：進行中の攻撃をキャンセルする。 </summary>
        public void CancelAttack()
        {
            if (_aiController == null) return;
            _aiController.CancelAttack();
        }

        /// <summary> ゲームプレイ開始処理。 </summary>
        public void StartGameplay()
        {
            _isPlaying = true;
        }

        /// <summary> ゲームプレイ停止処理。 </summary>
        public void StopGameplay()
        {
            _isPlaying = false;
            CancelAttack();
        }

        private BossAIController _aiController;
        private bool _isPlaying;
    }
}
