using DevelopProducts.Design.GameMode.Adaptor;
using UnityEngine;

namespace DevelopProducts.Design.GameMode.View
{
    /// <summary>
    ///     プレイヤーの当たり判定のビュークラス。
    ///     プレイヤーが敵と衝突した際の処理を担当する。
    /// </summary>
    public class PlayerCollisionView : MonoBehaviour
    {
        public void Initialize(PlayerColisionController controller)
        {
            _controller = controller;
        }

        private void OnTriggerEnter(Collider other)
        {
            EnemyView enemyView = other.GetComponent<EnemyView>();
            if (enemyView != null)
            {
                // プレイヤーと敵が衝突した場合の処理
                _controller.HitEnemy(enemyView.RuntimeState, enemyView.EnemyDefinition);
            }
        }

        private PlayerColisionController _controller;
    }
}
