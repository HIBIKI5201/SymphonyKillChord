using KillChord.Runtime.Adaptor;
using KillChord.Runtime.Adaptor.InGame.Enemy;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.Enemy
{
    public class EnemyBattleAIFacade : MonoBehaviour, IEnemyBattleAIFacade
    {
        public void Initialize(EnemyAIController aIController)
        {
            _aiController = aIController;
        }

        public void StartAttack()
        {
            _aiController.ReserveAttack();
        }

        public void StartStunAnimation()
        {
            Debug.Log("被弾アニメーション実装待ち");
        }

        public void CancelAttack()
        {
            _aiController.CanelAttack();
        }

        private EnemyAIController _aiController;
    }
}
