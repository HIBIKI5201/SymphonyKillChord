using KillChord.Runtime.Adaptor.InGame.Camera.Target;
using KillChord.Runtime.Adaptor.InGame.Music;
using KillChord.Runtime.Application.InGame.Camera.Target;
using KillChord.Runtime.Application.InGame.Music;
using KillChord.Runtime.Composition.InGame.Music;
using KillChord.Runtime.Domain.InGame.Battle;
using KillChord.Runtime.InfraStructure.InGame.Character;
using KillChord.Runtime.View.InGame.Music;
using SymphonyFrameWork.System.ServiceLocate;
using UnityEngine;

namespace KillChord.Runtime.Composition.InGame.Enemy
{
    /// <summary>
    ///     テスト用の歩兵スポナー。
    /// </summary>
    public class EnemyInfantryTestSpawner : MonoBehaviour
    {
        private void Awake()
        {
            ServiceLocator.RegisterInstance(this);
        }

        /// <summary>
        ///     攻撃対象のエンティティを設定する。
        /// </summary>
        /// <param name="targetEntity"></param>
        public void SetTargetEntity(IDefender targetEntity)
        {
            _targetEntity = targetEntity;
        }

        /// <summary>
        ///     目標選択関連のインスタンスを設定する。
        /// </summary>
        /// <param name="targetManager"></param>
        /// <param name="targetEntityRegistry"></param>
        public void SetTargetManager(TargetManager targetManager, TargetEntityRegistry targetEntityRegistry)
        {
            _targetManager = targetManager;
            _targetManagerController = new(targetManager);
            _targetEntityRegistryController = new(targetEntityRegistry);
        }

        [SerializeField, Tooltip("敵Prefab")] private EnemyMoveDebugInitializer _enemyPrefab;
        [SerializeField, Tooltip("生成位置")] private Transform _spawnPoint;
        [SerializeField, Tooltip("生成間隔")] private float _spawnInterval;
        [SerializeField, Tooltip("最大生成数")] private int _maxSpawnCount;

        [SerializeField, Tooltip("敵のキャラクター基盤データ")] private CharacterData _enemyData;

        private MusicSyncState _musicSyncState;
        private IMusicSyncService _musicSyncService;
        private IDefender _targetEntity;
        private TargetManager _targetManager;
        private TargetManagerController _targetManagerController;
        private TargetEntityRegistryController _targetEntityRegistryController;
        private Transform _target;

        private float _timer;
        private int _spawnCount;

        /// <summary>
        ///     初期化処理。
        /// </summary>
        public void Init()
        {
            MusicSyncInitializer initializer = FindFirstObjectByType<MusicSyncInitializer>();
            if (initializer?.MusicSyncService == null)
            {
                Debug.LogError("MusicSyncInitializerが見つかりません。", this);
                return;
            }
            _musicSyncService = initializer.MusicSyncService;

            MusicSyncView view = FindAnyObjectByType<MusicSyncView>();
            if (view?.MusicSyncState == null)
            {
                Debug.LogError("MusicSyncViewが見つかりません。", this);
                return;
            }
            _musicSyncState = view.MusicSyncState;
        }

        private void Update()
        {
            if (_enemyPrefab == null || _spawnPoint == null) return;
            if (_spawnCount >= _maxSpawnCount) return;

            _timer += Time.deltaTime;

            if (_timer >= _spawnInterval)
            {
                _timer = 0f;
                SpawnEnemy();
            }
        }

        /// <summary>
        ///     敵生成処理。
        /// </summary>
        private void SpawnEnemy()
        {
            PlayerInitializer playerInitializer = ServiceLocator.GetInstance<PlayerInitializer>();
            if (playerInitializer == null)
            {
                Debug.LogError("ターゲットのTransformが見つかりません。", this);
                return;
            }
            _target = playerInitializer.transform;

            if (_targetEntity == null || _musicSyncState == null ||
                _musicSyncService == null || _targetEntityRegistryController == null)
            {
                if (_targetEntity == null) Debug.LogError("ターゲットエンティティが設定されていません。", this);
                if (_musicSyncState == null) Debug.LogError("MusicSyncStateが見つかりません。", this);
                if (_musicSyncService == null) Debug.LogError("MusicSyncServiceが見つかりません。", this);
                if (_targetEntityRegistryController == null) Debug.LogError("TargetEntityRegistryControllerが見つかりません。", this);
                return;
            }

            EnemyMoveDebugInitializer enemyInstance =
                Instantiate(_enemyPrefab, _spawnPoint.position, _spawnPoint.rotation);

            EnemyInfantryAttackControllerGenerator attackControllerGenerator = new EnemyInfantryAttackControllerGenerator();

            enemyInstance.Initialize(_target, (Domain.InGame.Character.CharacterEntity)_targetEntity,
            _musicSyncState, _musicSyncService, _targetManagerController, _targetEntityRegistryController, attackControllerGenerator);

            _spawnCount++;
        }
    }
}