using KillChord.Runtime.Adaptor;
using UnityEngine;

namespace KillChord.Runtime.View.InGame
{
    /// <summary>
    ///     毎フレーム敵移動を更新するビュー。
    ///     敵の移動ロジックはEnemyMoveControllerに委譲される。
    /// </summary>
    public class EnemyMoveView : MonoBehaviour
    {
        public void Initialize(EnemyMoveController enemyMoveController)
        {
            _enemyMoveController = enemyMoveController;
        }

        private void Update()
        {
            if(_enemyMoveController != null && _target != null)
            {
                _enemyMoveController.Move(transform.position, _target.position);
            }
        }

        [SerializeField] private Transform _target;

        private EnemyMoveController _enemyMoveController;
    }
}
