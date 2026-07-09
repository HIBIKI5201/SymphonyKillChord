using KillChord.Runtime.Adaptor.InGame.Target;
using KillChord.Runtime.Adaptor.InGame.Enemy;
using KillChord.Runtime.Adaptor.InGame.Music;
using KillChord.Runtime.Application.InGame.Music;
using KillChord.Runtime.Composition.InGame.Music;
using KillChord.Runtime.Composition.InGame.Player;
using KillChord.Runtime.View.InGame.Music;
using SymphonyFrameWork.System.ServiceLocate;
using System;
using UnityEngine;

namespace KillChord.Runtime.Composition.InGame.Enemy
{
    /// <summary>
    ///     敵インスタンスを初期化するクラス。
    /// </summary>
    public class EnemyInitializer : MonoBehaviour
    {
        /// <summary>
        ///     初期化処理。
        /// </summary>
        /// <param name="enemyPools"></param>
        public void Initialize(TargetSystemController targetingSystem, EnemyPools enemyPools, EnemyWaveSpawnerState waveSpawnerState)
        {
            MusicSyncInitializer initializer = FindFirstObjectByType<MusicSyncInitializer>();
            if (initializer == null || initializer.MusicSyncService == null)
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
            _targetingSystem = targetingSystem;
            _playerInitializer = ServiceLocator.GetInstance<PlayerInitializer>();
            if (_playerInitializer == null)
            {
                Debug.LogError("PlayerInitializerの取得に失敗しました。", this);
                return;
            }
            _enemyPools = enemyPools;
            _waveSpawnState = waveSpawnerState;
            _initialized = true;
        }

        /// <summary>
        ///     歩兵の初期化処理。
        /// </summary>
        /// <param name="lifeCycle"></param>
        /// <param name="releaseCallback"></param>
        public void InitializeInfantry(EnemyLifeCycle lifeCycle, Action<EnemyLifeCycle> releaseCallback)
        {
            if (!_initialized)
            {
                Debug.LogError("[EnemyInitializer] 初期化が行われていません。", this);
                return;
            }
            EnemyInfantryAttackControllerGenerator attackControllerGenerator = new EnemyInfantryAttackControllerGenerator();
            lifeCycle.Initialize(_playerInitializer.transform, _playerInitializer.PlayerEntity,
            _musicSyncState, _musicSyncService, _targetingSystem, attackControllerGenerator, null, _waveSpawnState, releaseCallback);
        }

        /// <summary>
        ///     砲兵の初期化処理。
        /// </summary>
        /// <param name="lifeCycle"></param>
        /// <param name="releaseCallback"></param>
        public void InitializeArtillery(EnemyLifeCycle lifeCycle, Action<EnemyLifeCycle> releaseCallback)
        {
            if (!_initialized)
            {
                Debug.LogError("[EnemyInitializer] 初期化が行われていません。", this);
                return;
            }
            EnemyArtilleryAttackControllerGenerator attackControllerGenerator = new EnemyArtilleryAttackControllerGenerator();
            lifeCycle.Initialize(_playerInitializer.transform, _playerInitializer.PlayerEntity,
            _musicSyncState, _musicSyncService, _targetingSystem, attackControllerGenerator, _enemyPools, _waveSpawnState, releaseCallback);
        }

        private PlayerInitializer _playerInitializer;
        private MusicSyncState _musicSyncState;
        private IMusicSyncService _musicSyncService;
        private TargetSystemController _targetingSystem;
        private EnemyPools _enemyPools;
        private EnemyWaveSpawnerState _waveSpawnState;
        private bool _initialized = false;
    }
}
