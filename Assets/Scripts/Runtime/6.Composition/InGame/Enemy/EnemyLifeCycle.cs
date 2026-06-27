using KillChord.Runtime.Adaptor;
using KillChord.Runtime.Adaptor.InGame.Camera.Target;
using KillChord.Runtime.Adaptor.InGame.Enemy;
using KillChord.Runtime.Adaptor.InGame.Mission;
using KillChord.Runtime.Adaptor.InGame.Music;
using KillChord.Runtime.Adaptor.InGame.UI;
using KillChord.Runtime.Application.InGame.Enemy;
using KillChord.Runtime.Application.InGame.Music;
using KillChord.Runtime.Domain.InGame.Battle;
using KillChord.Runtime.Domain.InGame.Character;
using KillChord.Runtime.Domain.InGame.Enemy;
using KillChord.Runtime.InfraStructure;
using KillChord.Runtime.InfraStructure.InGame.Character;
using KillChord.Runtime.InfraStructure.InGame.Enemy;
using KillChord.Runtime.InfraStructure.InGame.Mission;
using KillChord.Runtime.View;
using KillChord.Runtime.View.InGame.Enemy;
using KillChord.Runtime.View.InGame.Enemy.AIFacade;
using KillChord.Runtime.View.InGame.Sequence;
using KillChord.Runtime.View.InGame.UI;
using SymphonyFrameWork.System.ServiceLocate;
using System;
using System.Threading;
using System.Threading.Tasks;
using Unity.Behavior;
using UnityEngine;
using UnityEngine.AI;

namespace KillChord.Runtime.Composition.InGame.Enemy
{
    /// <summary>
    ///     敵の依存関係を構築する。
    /// </summary>
    public class EnemyLifeCycle : MonoBehaviour, IGameplayControllable
    {

        /// <summary>
        ///     初期化処理。
        /// </summary>
        /// <param name="target"></param>
        /// <param name="targetEntity"></param>
        /// <param name="musicSyncState"></param>
        /// <param name="musicSyncService"></param>
        /// <param name="targetManagerController"></param>
        /// <param name="targetEntityRegistryController"></param>
        /// <param name="attackControllerGenerator"></param>
        public void Initialize(
            Transform target,
            CharacterEntity targetEntity,
            MusicSyncState musicSyncState,
            IMusicSyncService musicSyncService,
            TargetManagerController targetManagerController,
            TargetEntityRegistryController targetEntityRegistryController,
            IEnemyAttackControllerGenerator attackControllerGenerator,
            IShellPool shellPool,
            EnemyWaveSpawnerState waveSpawnerState,
            Action<EnemyLifeCycle> releaseCallback
            )
        {
            if (_view == null)
                Debug.LogError($"{nameof(EnemyMoveView)}の参照がありません。");
            if (_healthView == null)
                Debug.LogError($"{nameof(EnemyHealthView)}の参照がありません。");
            if (_raycastView == null)
                Debug.LogError($"{nameof(EnemyRaycastDetectView)}の参照がありません。");
            if (_attackPositionSearchView == null)
                Debug.LogError($"{nameof(NearestAttackPositionSearchView)}の参照がありません。");

            _targetManagerController = targetManagerController;
            _targetEntityRegistryController = targetEntityRegistryController;
            _enemyEntity = CharacterFactory.Create(_enemyData);
            _waveSpawnerState = waveSpawnerState;

            _missionEventController = ServiceLocator.GetInstance<MissionEventController>();
            _attackControllerGenerator = attackControllerGenerator;
            _releaseCallback = releaseCallback;

            // 敵射線判定
            EnemyRaycastDetectController raycastController = new EnemyRaycastDetectController(_raycastView);
            EnemyRaycastDetectService raycastDetectService = new EnemyRaycastDetectService(raycastController);

            // 攻撃位置探索
            NearestAttackPositionSearchController attackPositionSearchController = new NearestAttackPositionSearchController(_attackPositionSearchView);
            NearestAttackPositionSearchService attackPositionSearchService = new NearestAttackPositionSearchService(attackPositionSearchController);

            // Domain生成
            EnemyMoveSpec spec = EnemyFactory.CreateEnemyMoveSpec(_moveData);
            EnemyAttackMusicSpec attackMusicSpec = EnemyFactory.CreateEnemyAttackMusicSpec(_encounterMusicData, _battleMusicData);

            AttackDefinition attackDefinition = _enemyEntity.CombatSpec.GetAttackDifinition(_attackIndex);

            // Adaptor
            IMusicActionScheduler musicActionScheduler = new MusicSchedulerAdaptor(musicSyncState, musicSyncService);

            // UseCase
            EnemyMoveUsecase useCase = new EnemyMoveUsecase(spec, raycastDetectService, attackPositionSearchService);
            EnemyAttackReservationUsecase attackReservationUsecase = new EnemyAttackReservationUsecase(attackMusicSpec, musicActionScheduler);
            EnemyAttackUsecase attackUsecase = new EnemyAttackUsecase(raycastDetectService);
            _attackReservationUsecase = attackReservationUsecase;


            EnemyBattleState battleState = new EnemyBattleState(_enemyEntity, targetEntity, attackDefinition);
            _battleState = battleState;

            // AttackController生成用コンテキスト
            EnemyAttackControllerContext attackControllerContext = new EnemyAttackControllerContext(attackUsecase, battleState, _shellSpawner);

            // Controller
            IEnemyAttackController attackController = _attackControllerGenerator.Generate(attackControllerContext);
            EnemyAIController aiController = new EnemyAIController(useCase, attackReservationUsecase, battleState, _enemyStateFacade, attackController);
            _aiController = aiController;

            IHealthHudViewModel viewModel = new HealthHudViewModel(_enemyEntity.CurrentHealth.Value, _enemyEntity.MaxHealth.Value);
            // HP Presenter
            IHealthHudPresenter healthHudPresenter = new EnemyHealthHudPresenter(_enemyEntity, viewModel, _healthView);
            _healthHudPresenter = healthHudPresenter;

            _lockOnTargetGateway = new LockOnTargetGateway(transform);

            // View接続
            var animationComposition = new AnimationComposition();
            var animationController = animationComposition.Init(_characterAnimationView, _characterAnimationCatalogAsset, musicSyncState, out CharacterAnimationIndices animationIndices);
            _characterAnimationController = animationController;
            _characterAnimationIndices = animationIndices;
            _view.Initialize(aiController, target, animationController,animationIndices);
            _healthView.Bind(viewModel);
            _healthView.Initialize(healthHudPresenter);
            _raycastView.Initialize(target, spec.AttackRangeMax.Value);
            _aiController.On1BeatBefore += _raycastView.LockWarningDirection;
            _aiController.On2BeatBefore += _raycastView.StartTrackingWarning;
            _aiController.OnAttack += _raycastView.HideWarning;
            _attackPositionSearchView.Initialize();
            if (_shellSpawner != null && shellPool != null)
            {
                _shellSpawner.Initialize(shellPool);
            }

            // ファサード初期化
            _enemyMovementAIFacade.Initialize(_view);
            _enemyBattleAIFacade.Initialize(aiController);
            _enemyStateFacade.Initialize(aiController, target, _raycastView, battleState);
            //_enemySharedFacade.Initialize(target);
        }

        /// <summary>
        ///     有効化処理。
        ///     コンポーネント有効化、インスタンスのリセット、依存や購読の再構築を行う。
        /// </summary>
        public void Activate(Vector3 position, System.Action spawnerCallback)
        {
            _isDying = false;
            _spawnerCallback = spawnerCallback;
            _enemyEntity.Reset();
            _battleState.Reset();
            _aiController.Activate();
            _healthHudPresenter.Activate();

            if (_missionEventController != null && _missionKeyAsset != null)
            {
                _enemyEntity.OnDied += HandleEnemyDied;
            }
            _targetManagerController?.Register(_lockOnTargetGateway);
            _targetEntityRegistryController?.RegisterTargetEntity(_lockOnTargetGateway, _enemyEntity);

            // コンポーネント有効化
            SetDyingCollidersEnabled(true);
            _view.Activate();
            _attackPositionSearchView.enabled = true;
            _navMeshAgent.enabled = true;
            _navMeshAgent.Warp(position);
            _behaviorGraphAgent.enabled = true;
            _behaviorGraphAgent.Restart();
            gameObject.SetActive(true);
        }

        /// <summary>
        ///     マップ外側の実生成地点から、戦闘開始地点まで移動してから有効化する。
        /// </summary>
        /// <param name="entryPosition">マップ外側の実生成地点。</param>
        /// <param name="activePosition">到着後に戦闘を開始する地点。</param>
        /// <param name="spawnerCallback">無効化時にスポナーへ通知するcallback。</param>
        public async ValueTask<bool> EnterFromOutsideAsync(
            SpawnPositionPair positionPair,
            System.Action spawnerCallback,
            CancellationToken ct)
        {
            bool hasPreparedEntrance = false;
			positionPair.SetInUse(true);
            try
            {
                ct.ThrowIfCancellationRequested();

                PrepareEntrance(positionPair.SpawnPosition.position);
                hasPreparedEntrance = true;

                bool hasArrived =
                    await _view.MoveToTargetAysnc(
                        positionPair.EntryPosition.position,
                        ct);

                ct.ThrowIfCancellationRequested();
				positionPair.SetInUse(false);
                if (!hasArrived || this == null)
                {
                    if (this != null)
                    {
                        CancelEntrance();
                    }
                    positionPair.SetInUse(false);
                    return false;
                }

                Activate(positionPair.EntryPosition.position, spawnerCallback);
                return true;
            }
            catch (OperationCanceledException)
            {
                if (hasPreparedEntrance && this != null)
                {
                    CancelEntrance();
                }

                throw;
            }
            catch
            {
                if (hasPreparedEntrance && this != null)
                {
                    CancelEntrance();
                }

                throw;
            }
        }

        /// <summary>
        ///     無効化処理。
        ///     コンポーネント無効化、依存解除や購読解除を行う。
        /// </summary>
        public void Deactivate()
        {
            // コンポーネント無効化
            _behaviorGraphAgent.enabled = false;
            _navMeshAgent.enabled = false;
            _attackPositionSearchView.enabled = false;
            SetDyingCollidersEnabled(true);
            _view.Deactivate();

            if (_missionEventController != null && _missionKeyAsset != null)
            {
                _enemyEntity.OnDied -= HandleEnemyDied;
                _missionEventController.NotifyEnemyKilled(_missionKeyAsset.Id);
            }
            _targetManagerController?.Unregister(_lockOnTargetGateway);
            _targetEntityRegistryController?.UnregisterTargetEntity(_lockOnTargetGateway);

            _attackReservationUsecase.Deactivate();
            _aiController.Deactivate();
            _healthHudPresenter.Deactivate();

            _spawnerCallback?.Invoke();
            _spawnerCallback = null;
            gameObject.SetActive(false);
            _releaseCallback?.Invoke(this);
        }

        /// <summary>
        ///    ゲームプレイ開始処理。
        /// </summary>
        public void StartGameplay()
        {
            if (_behaviorGraphAgent != null)
            {
                _behaviorGraphAgent.enabled = true;
                _behaviorGraphAgent.Restart();
            }

            if (_navMeshAgent != null && _navMeshAgent.enabled)
            {
                _navMeshAgent.isStopped = false;
            }

            _enemyBattleAIFacade?.StartGameplay();
            _view?.StartGameplay();
        }

        /// <summary>
        ///    ゲームプレイ停止処理。
        /// </summary>
        public void StopGameplay()
        {
            _attackReservationUsecase?.Deactivate();
            _aiController?.CancelAttack();

            if (_behaviorGraphAgent != null)
            {
                _behaviorGraphAgent.enabled = false;
            }

            if (_navMeshAgent != null && _navMeshAgent.enabled && _navMeshAgent.isOnNavMesh)
            {
                _navMeshAgent.isStopped = true;
                _navMeshAgent.ResetPath();
                _navMeshAgent.velocity = Vector3.zero;
                _navMeshAgent.updateRotation = false;
            }

            _enemyBattleAIFacade?.StopGameplay();
            _view?.StopGameplay();
        }

        private System.Action _spawnerCallback;
        private Action<EnemyLifeCycle> _releaseCallback;
        private ICharacterAnimationController _characterAnimationController;
        private CharacterAnimationIndices _characterAnimationIndices;

        [SerializeField] private CharacterData _enemyData;
        [SerializeField] private EnemyMoveData _moveData;
        [SerializeField] private EnemyMusicData _encounterMusicData;
        [SerializeField] private EnemyMusicData _battleMusicData;

        [SerializeField] private int _attackIndex;

        [SerializeField] private EnemyMoveView _view;
        [SerializeField] private EnemyHealthView _healthView;
        [SerializeField] private EnemyRaycastDetectView _raycastView;
        [SerializeField] private NearestAttackPositionSearchView _attackPositionSearchView;
        [SerializeField] private EnemyMissionKeyAsset _missionKeyAsset;
        [SerializeField] private EnemyMovementAIFacade _enemyMovementAIFacade;
        [SerializeField] private EnemyBattleAIFacade _enemyBattleAIFacade;
        [SerializeField] private EnemyStateFacade _enemyStateFacade;
        [SerializeField] private EnemySharedFacade _enemySharedFacade;
        [SerializeField] private BehaviorGraphAgent _behaviorGraphAgent;
        [SerializeField] private NavMeshAgent _navMeshAgent;
        [SerializeField] private CharacterAnimationView _characterAnimationView;
        [SerializeField] private CharacterAnimationCatalogAsset _characterAnimationCatalogAsset;
        [SerializeField, Tooltip("死亡時に再生するワンショットアニメーションキー。")]
        private string _deathAnimationKey = "Enemy_Death";
        [SerializeField,Tooltip("死体消滅時のワンショットアニメーションキー。")]
        private string _destroyAniamtionKey = "Enemy_Destroy";
        [SerializeField, Min(0f), Tooltip("死亡アニメーションキーが見つからない場合の待機秒数。")]
        private float _deathAnimationFallbackSeconds = 0.5f;
        [SerializeField, Tooltip("死亡アニメーション開始前に無効化する判定。未設定の場合は何もしません。")]
        private Collider[] _disableOnDyingColliders;


        [Header("砲兵の場合のみ必要")]
        [SerializeField] private ShellSpawner _shellSpawner;

        private TargetEntityRegistryController _targetEntityRegistryController;
        private TargetManagerController _targetManagerController;
        private LockOnTargetGateway _lockOnTargetGateway;
        private MissionEventController _missionEventController;
        private CharacterEntity _enemyEntity;
        private IEnemyAttackControllerGenerator _attackControllerGenerator;
        private EnemyAIController _aiController;
        private EnemyAttackReservationUsecase _attackReservationUsecase;
        private IHealthHudPresenter _healthHudPresenter;
        private EnemyBattleState _battleState;
        private EnemyWaveSpawnerState _waveSpawnerState;
        private bool _isDying;

        /// <summary>
        ///     入場移動に必要な表示とNavMeshAgentのみ有効化する。
        /// </summary>
        /// <param name="position">入場開始地点。</param>
        private void PrepareEntrance(Vector3 position)
        {
            if (_behaviorGraphAgent != null)
            {
                _behaviorGraphAgent.enabled = false;
            }

            if (_attackPositionSearchView != null)
            {
                _attackPositionSearchView.enabled = false;
            }

            gameObject.SetActive(true);

            if (_navMeshAgent != null)
            {
                _navMeshAgent.enabled = true;
                _navMeshAgent.Warp(position);
                _navMeshAgent.isStopped = false;
            }
        }

        /// <summary>
        ///     死亡演出を挟んでから完全な無効化処理を行う。
        /// </summary>
        private async void DieAsync()
        {
            if (_isDying)
            {
                return;
            }

            _isDying = true;
            BeginDying();
            try
            {
                await PlayDeathAnimationAsync();
                Deactivate();
                _waveSpawnerState.OnEnemyDeath();
            }
            catch (OperationCanceledException)
            {
            }
        }

        /// <summary>
        ///     死亡アニメーション前に、戦闘処理や判定を停止する。
        /// </summary>
        private void BeginDying()
        {
            if (_enemyEntity != null)
            {
                _enemyEntity.OnDied -= HandleEnemyDied;
            }

            _attackReservationUsecase?.Deactivate();
            _aiController?.CancelAttack();
            _aiController?.Deactivate();
            _enemyBattleAIFacade?.StopGameplay();
            _view?.StopGameplay();
            _healthHudPresenter?.Deactivate();

            if (_behaviorGraphAgent != null)
            {
                _behaviorGraphAgent.enabled = false;
            }

            if (_navMeshAgent != null && _navMeshAgent.enabled && _navMeshAgent.isOnNavMesh)
            {
                _navMeshAgent.isStopped = true;
                _navMeshAgent.ResetPath();
                _navMeshAgent.velocity = Vector3.zero;
                _navMeshAgent.updateRotation = false;
            }

            if (_attackPositionSearchView != null)
            {
                _attackPositionSearchView.enabled = false;
            }

            _targetManagerController?.Unregister(_lockOnTargetGateway);
            _targetEntityRegistryController?.UnregisterTargetEntity(_lockOnTargetGateway);
            SetDyingCollidersEnabled(false);
        }

        /// <summary>
        ///     死亡アニメーションを再生し、再生時間分待機する。
        /// </summary>
        private async ValueTask PlayDeathAnimationAsync()
        {
            //TODO 拡張性をもたせた設計にする。
            float waitSeconds = _deathAnimationFallbackSeconds;

            if (_characterAnimationController != null
                && _characterAnimationIndices != null
                && _characterAnimationIndices.TryGetOneShotIndex(_deathAnimationKey, out int deathIndex))
            {
                _characterAnimationController.SetVelocity(Vector2.zero);
                _characterAnimationController.TriggerOneShot(deathIndex);
                waitSeconds = _characterAnimationController.GetOneShotAnimationLength(deathIndex);
            }

            if (waitSeconds <= 0f)
            {
                return;
            }
            //TODO ボイス処理を追加する。
            await Awaitable.WaitForSecondsAsync(waitSeconds,destroyCancellationToken);//floatで時間を渡すためにAwatable


           if( _characterAnimationIndices.TryGetOneShotIndex(_destroyAniamtionKey,out int destoryIndex)){
            _characterAnimationController.TriggerOneShot(destoryIndex);
            waitSeconds = _characterAnimationController.GetOneShotAnimationLength(destoryIndex);
            }

            if(waitSeconds <= 0f)
            {
                return;
            }

            //TODO エフェクト処理を追加する。
            await Awaitable.WaitForSecondsAsync(waitSeconds,destroyCancellationToken);
        }

        /// <summary>
        ///     入場移動を中断して敵をプールへ戻す。
        /// </summary>
        private void CancelEntrance()
        {
            _view?.StopGameplay();

            if (_behaviorGraphAgent != null)
            {
                _behaviorGraphAgent.enabled = false;
            }

            if (_attackPositionSearchView != null)
            {
                _attackPositionSearchView.enabled = false;
            }

            if (_navMeshAgent != null
                && _navMeshAgent.enabled)
            {
                if (_navMeshAgent.isOnNavMesh)
                {
                    _navMeshAgent.isStopped = true;
                    _navMeshAgent.ResetPath();
                    _navMeshAgent.velocity = Vector3.zero;
                }

                _navMeshAgent.enabled = false;
            }

            gameObject.SetActive(false);
            _releaseCallback?.Invoke(this);
        }

        /// <summary>
        ///     死亡時に止める判定の有効状態を切り替える。
        /// </summary>
        /// <param name="enabled">有効にする場合はtrue。</param>
        private void SetDyingCollidersEnabled(bool enabled)
        {
            if (_disableOnDyingColliders == null)
            {
                return;
            }

            for (int i = 0; i < _disableOnDyingColliders.Length; i++)
            {
                if (_disableOnDyingColliders[i] == null)
                {
                    continue;
                }

                _disableOnDyingColliders[i].enabled = enabled;
            }
        }

        /// <summary>
        ///     敵死亡時に実行する処理。
        /// </summary>
        /// <param name="_"></param>
        private void HandleEnemyDied(CharacterEntity _)
        {
            DieAsync();
        }

        private void OnDestroy()
        {
            if (_enemyEntity != null)
            {
                _enemyEntity.OnDied -= HandleEnemyDied;
            }

            _targetManagerController?.Unregister(_lockOnTargetGateway);
            _targetEntityRegistryController?.UnregisterTargetEntity(_lockOnTargetGateway);
            _lockOnTargetGateway?.Dispose();
        }
    }
}
