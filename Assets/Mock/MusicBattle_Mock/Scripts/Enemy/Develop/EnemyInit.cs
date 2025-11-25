using UnityEngine;

namespace Mock.MusicBattle.Enemy
{
    /// <summary> エネミー全体の初期化テストクラス。 </summary>
    [DefaultExecutionOrder(-800)]
    public class EnemyInit : MonoBehaviour
    {
        [SerializeField] private EnemyManager _enemyManager;
        [SerializeField] private EnemyStatus _enemystatus;
        [SerializeField] private Transform _player;
        private EnemyContainer _enemyContainer;
        private EnemyFactory _factory;

        private void Awake()
        {
            _enemyContainer = new EnemyContainer();
            _factory = new EnemyFactory(_enemyContainer, _player, _enemyManager);
        }

        private void Start()
        {
            _factory.Spawn(_enemystatus, transform.position);
        }
    }
}