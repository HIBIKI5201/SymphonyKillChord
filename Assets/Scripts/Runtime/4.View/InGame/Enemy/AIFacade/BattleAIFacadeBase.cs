using KillChord.Runtime.Adaptor.InGame.Enemy;
using KillChord.Runtime.Adaptor.InGame.Enemy.EnemyAIFacadeInterface;
using KillChord.Runtime.View.InGame.Sequence;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.Enemy.AIFacade
{
    /// <summary>
    ///     敵・ボス共通のAI用ファサード：戦闘系。
    ///     攻撃の予約・キャンセルとゲームプレイの開始・停止を扱い、被弾時の演出だけを派生クラスに任せる。
    /// </summary>
    public abstract class BattleAIFacadeBase : MonoBehaviour, IEnemyBattleAIFacade, IGameplayControllable
    {
        /// <summary>
        ///     初期化処理。
        /// </summary>
        /// <param name="aiController"> 攻撃を予約するAIコントローラー。 </param>
        public void Initialize(IEnemyAIController aiController)
        {
            _aiController = aiController;
            _isPlaying = false;
        }

        /// <summary>
        ///     指示：目標に攻撃行動を開始する。
        /// </summary>
        public void StartAttack()
        {
            if (!_isPlaying || _aiController == null)
            {
                return;
            }

            _aiController.ReserveAttack();
        }

        /// <summary>
        ///     指示：被弾硬直アニメーションを開始する。
        /// </summary>
        public abstract void StartStunAnimation();

        /// <summary>
        ///     指示：進行中の攻撃をキャンセルする。
        /// </summary>
        public void CancelAttack()
        {
            if (_aiController == null)
            {
                return;
            }

            _aiController.CancelAttack();
        }

        /// <summary>
        ///     ゲームプレイの開始処理を行う。
        /// </summary>
        public void StartGameplay()
        {
            _isPlaying = true;
        }

        /// <summary>
        ///     ゲームプレイの停止処理を行う。進行中の攻撃もキャンセルする。
        /// </summary>
        public void StopGameplay()
        {
            _isPlaying = false;
            CancelAttack();
        }

        private IEnemyAIController _aiController;
        private bool _isPlaying;
    }
}
