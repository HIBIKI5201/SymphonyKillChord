using KillChord.Runtime.Adaptor.InGame.Animation;
using KillChord.Runtime.Adaptor.InGame.Enemy;
using KillChord.Runtime.Adaptor.InGame.Mission;
using KillChord.Runtime.Adaptor.InGame.Music;
using KillChord.Runtime.Adaptor.InGame.Target;
using KillChord.Runtime.Adaptor.InGame.UI;
using KillChord.Runtime.Application.InGame.Enemy;
using KillChord.Runtime.Application.InGame.Music;
using KillChord.Runtime.Composition.InGame.Mission;
using KillChord.Runtime.Domain.InGame.Battle;
using KillChord.Runtime.Domain.InGame.Character;
using KillChord.Runtime.Domain.InGame.Enemy;
using KillChord.Runtime.Domain.InGame.Mission;
using KillChord.Runtime.InfraStructure.InGame.Character;
using KillChord.Runtime.InfraStructure.InGame.Enemy;
using KillChord.Runtime.InfraStructure.InGame.Mission;
using KillChord.Runtime.Utility.Persistent;
using KillChord.Runtime.Utility.Rendering;
using KillChord.Runtime.View;
using KillChord.Runtime.View.InGame.Character;
using KillChord.Runtime.View.InGame.Enemy;
using KillChord.Runtime.View.InGame.Enemy.AIFacade;
using KillChord.Runtime.View.InGame.Sequence;
using KillChord.Runtime.View.InGame.Target;
using KillChord.Runtime.View.InGame.UI;
using LitMotion;
using LitMotion.Extensions;
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
        ///     個別の敵定義が保持するゲームデータを設定します。
        /// </summary>
        /// <param name="definition"> このインスタンスへ適用する個別の敵定義です。 </param>
        /// <returns> 必要なデータがすべて設定されている場合はtrueです。 </returns>
        public bool Configure(EnemyDefinitionAsset definition)
        {
            if (definition == null)
            {
                return false;
            }

            _loadedEnemyData = definition.CharacterDefinition;
            _loadedMoveData = definition.MoveSpec;
            _loadedEncounterMusicData = definition.EncounterMusicSpec;
            _loadedBattleMusicData = definition.BattleMusicSpec;
            _loadedMissionKeyAsset = definition.MissionKey;
            _attackIndex = definition.AttackIndex;

            return _loadedEnemyData != null
                && _loadedMoveData != null
                && _loadedEncounterMusicData != null
                && _loadedBattleMusicData != null
                && _loadedMissionKeyAsset != null
                && _characterAnimationConfig != null;
        }

        /// <summary>
        ///     初期化処理。
        /// </summary>
        /// <param name="target"></param>
        /// <param name="targetEntity"></param>
        /// <param name="musicSyncState"></param>
        /// <param name="musicSyncService"></param>
        /// <param name="targetingSystem"></param>
        /// <param name="attackControllerGenerator"></param>
        public void Initialize(
            Transform target,
            CharacterEntity targetEntity,
            MusicSyncState musicSyncState,
            IMusicSyncService musicSyncService,
            TargetSystemController targetingSystem,
            IEnemyAttackControllerGenerator attackControllerGenerator,
            IShellPool shellPool,
            DamageNumberPoolView damageNumberPoolView,
            ReusableParticleSystemView damageEffectView,
            EnemyWaveSpawnerState waveSpawnerState,
            Action<EnemyLifeCycle> releaseCallback,
            EnemyType enemyType,
            EnemyAIControllerRegistry battleAiRegistry
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
            if (_targetTransform == null)
            {
                Debug.LogError("_targetTransformの参照がありません", this);
                return;
            }

            _targetingSystem = targetingSystem;
            _enemyEntity = CharacterFactory.Create(_loadedEnemyData);
            _waveSpawnerState = waveSpawnerState;
            _battleAIRegistry = battleAiRegistry;

            MissionModuleContainer missionModuleContainer = ServiceLocator.GetInstance<MissionModuleContainer>();
            _missionEventController = missionModuleContainer?.MissionEventController;
            _attackControllerGenerator = attackControllerGenerator;
            _releaseCallback = releaseCallback;

            // 敵射線判定
            EnemyRaycastDetectController raycastController = new EnemyRaycastDetectController(_raycastView);
            EnemyRaycastDetectService raycastDetectService = new EnemyRaycastDetectService(raycastController);

            // 攻撃位置探索
            NearestAttackPositionSearchController attackPositionSearchController = new NearestAttackPositionSearchController(_attackPositionSearchView);
            NearestAttackPositionSearchService attackPositionSearchService = new NearestAttackPositionSearchService(attackPositionSearchController);

            // Domain生成
            EnemyMoveSpec spec = EnemyFactory.CreateEnemyMoveSpec(_loadedMoveData);
            EnemyAttackMusicSpec attackMusicSpec = EnemyFactory.CreateEnemyAttackMusicSpec(_loadedEncounterMusicData, _loadedBattleMusicData);

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
            EnemyAttackControllerContext attackControllerContext = new EnemyAttackControllerContext(attackUsecase, null, battleState, _shellSpawner);

            // Controller
            IEnemyAttackController attackController = _attackControllerGenerator.Generate(attackControllerContext);
            EnemyAIController aiController = new EnemyAIController(useCase, attackReservationUsecase, battleState, _enemyStateFacade, attackController);
            _aiController = aiController;

            IHealthHudViewModel viewModel = new HealthHudViewModel(_enemyEntity.CurrentHealth.Value, _enemyEntity.MaxHealth.Value);
            // HP Presenter
            var healthHudPresenter = new EnemyHealthHudPresenter(_enemyEntity, _enemyEntity.Id, viewModel, _healthView);
            _healthHudPresenter = healthHudPresenter;
            _enemyHealthHudPresenter = healthHudPresenter;

            _enemyHealthHudPresenter.OnDamaged += _view.PlayDamageFeedback;

            _targetable = new TransformTargetable(_enemyEntity.Id, _targetTransform, GetComponent<Collider>());

            // View接続
            var animationComposition = new AnimationComposition();
            ICharacterAnimationViewContext animationContext =
                animationComposition.Init(_characterAnimationView, _characterAnimationConfig, musicSyncState);
            _characterAnimationContext = animationContext;
            _view.Initialize(aiController, target, animationContext, musicSyncState, damageEffectView);
            _healthView.Bind(viewModel);
            _healthView.Initialize(healthHudPresenter, damageNumberPoolView);
            // 警告デカールへ、攻撃タイミングまでの進捗を0〜1で供給する。
            _musicSyncState = musicSyncState;
            _raycastView.Initialize(
                target,
                spec.AttackRangeMax.Value,
                GetAttackApproach);

            if (enemyType == EnemyType.Infantry)
            {
                _aiController.On1BeatBefore += _raycastView.LockWarningDirection;
                _aiController.On2BeatBefore += _raycastView.StartTrackingWarning;
                _aiController.OnAttack += _raycastView.HideWarning;
            }
            _aiController.OnAttack += HandleEnemyAttackExecuted;
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

            ResetDeathEffect();
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
            ResetDeathEffect();

            _enemyEntity.OnDied += HandleEnemyDied;
            _targetingSystem?.RegisterTarget(_targetable, _enemyEntity);
            _battleAIRegistry?.Register(_aiController);

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

            _enemyEntity.OnDied -= HandleEnemyDied;
            if (_missionEventController != null && _loadedMissionKeyAsset != null)
            {
                _missionEventController.NotifyEnemyKilled(_loadedMissionKeyAsset.Id);
            }
            _targetingSystem?.UnregisterTarget(_targetable);
            _battleAIRegistry?.Unregister(_aiController);

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

        /// <summary>
        ///     予約済みの攻撃時刻までの残り時間から、警告デカール用の0〜1の接近進捗を算出します。
        /// </summary>
        /// <returns> 0〜1の進捗。予約が無い場合は0。 </returns>
        private float GetAttackApproach()
        {
            if (_musicSyncState == null
                || _attackReservationUsecase == null
                || !_attackReservationUsecase.HasReservation)
            {
                return 0f;
            }

            return _musicSyncState.GetNormalizedApproach(
                _attackReservationUsecase.AttackExecutionTime,
                WARNING_LEAD_BEAT_COUNT);
        }

        /// <summary> 警告デカールの進捗を0から1へ変化させる区間の長さ（拍）。 </summary>
        private const double WARNING_LEAD_BEAT_COUNT = 2d;

        private MusicSyncState _musicSyncState;
        private System.Action _spawnerCallback;
        private Action<EnemyLifeCycle> _releaseCallback;
        private ICharacterAnimationViewContext _characterAnimationContext;

        [SerializeField] private EnemyMoveView _view;
        [SerializeField] private EnemyHealthView _healthView;
        [SerializeField] private EnemyRaycastDetectView _raycastView;
        [SerializeField] private NearestAttackPositionSearchView _attackPositionSearchView;
        [SerializeField] private EnemyMovementAIFacade _enemyMovementAIFacade;
        [SerializeField] private EnemyBattleAIFacade _enemyBattleAIFacade;
        [SerializeField] private EnemyStateFacade _enemyStateFacade;
        [SerializeField] private EnemySharedFacade _enemySharedFacade;
        [SerializeField] private BehaviorGraphAgent _behaviorGraphAgent;
        [SerializeField] private NavMeshAgent _navMeshAgent;
        [SerializeField] private CharacterAnimationView _characterAnimationView;
        [SerializeField, Tooltip("敵キャラクターのアニメーション設定です。")]
        private CharacterAnimationCatalogConfig _characterAnimationConfig;
        [SerializeField, Tooltip("死亡時に再生するワンショットアニメーションキー。")]
        private string _deathAnimationKey = "Enemy_Death";
        [SerializeField, Tooltip("死体消滅時のワンショットアニメーションキー。")]
        private string _destroyAniamtionKey = "Enemy_Destroy";
        [SerializeField, Min(0f), Tooltip("死亡アニメーションキーが見つからない場合の待機秒数。")]
        private float _deathAnimationFallbackSeconds = 0.5f;
        [SerializeField, Tooltip("死亡アニメーション開始前に無効化する判定。未設定の場合は何もしません。")]
        private Collider[] _disableOnDyingColliders;

        [SerializeField, Tooltip("死亡演出でMaterialプロパティを変化させる対象のRendererです。未設定の場合は何もしません。")]
        private Renderer[] _deathEffectRenderers;
        [SerializeField, Tooltip("死亡演出用の沼のGameObjectです。")]
        private GameObject _deathSwampGameObject;

        /// <summary>
        ///     死亡演出で変化させるMaterialのfloatプロパティID（仮に"_DeathEffectAmount"）です。
        /// </summary>
        private static readonly int DeathEffectPropertyId = Shader.PropertyToID("_DeathEffectAmount");
        [SerializeField, Min(0f), Tooltip("死亡演出のMaterialプロパティが変化する時間（秒）です。")]
        private float _deathEffectDuration = 1f;
        [SerializeField, Min(0f), Tooltip("死亡演出後、沼が沈み込むまでの時間（秒）です。")]
        private float _deathSwampSinkDuration = 3f;

        [SerializeField, Tooltip("敵ロックオン時の中心となるTransform")]
        private Transform _targetTransform;

        [Header("砲兵の場合のみ必要")]
        [SerializeField] private ShellSpawner _shellSpawner;

        private TargetSystemController _targetingSystem;
        private TransformTargetable _targetable;
        private MissionEventController _missionEventController;
        private CharacterEntity _enemyEntity;
        private IEnemyAttackControllerGenerator _attackControllerGenerator;
        private EnemyAIController _aiController;
        private EnemyAttackReservationUsecase _attackReservationUsecase;
        private IHealthHudPresenter _healthHudPresenter;
        private EnemyHealthHudPresenter _enemyHealthHudPresenter;
        private EnemyBattleState _battleState;
        private EnemyAIControllerRegistry _battleAIRegistry;
        private EnemyWaveSpawnerState _waveSpawnerState;
        private bool _isDying;
        private int _attackIndex;

        private CharacterDefinitionAsset _loadedEnemyData;
        private EnemyMoveSpecAsset _loadedMoveData;
        private EnemyMusicSpecAsset _loadedEncounterMusicData;
        private EnemyMusicSpecAsset _loadedBattleMusicData;
        private EnemyMissionKeyAsset _loadedMissionKeyAsset;

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
            _raycastView?.HideWarning();
            _aiController?.Deactivate();
            _enemyBattleAIFacade?.StopGameplay();
            _view?.StopGameplay();

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

            _targetingSystem?.UnregisterTarget(_targetable);
            _battleAIRegistry?.Unregister(_aiController);
            SetDyingCollidersEnabled(false);
        }

        /// <summary>
        ///     死亡アニメーションを再生し、再生時間分待機する。
        /// </summary>
        private async ValueTask PlayDeathAnimationAsync()
        {
            //TODO 拡張性をもたせた設計にする。
            float waitSeconds = _deathAnimationFallbackSeconds;

            if (_characterAnimationContext != null
                && _characterAnimationContext.Signal.TryRequestOneShot(_deathAnimationKey, out float deathDuration))
            {
                waitSeconds = deathDuration;
                _characterAnimationContext.ViewModel.SetVelocity(Vector2.zero);
            }

            if (waitSeconds > 0f)
            {
                //TODO ボイス処理を追加する。
                await Awaitable.WaitForSecondsAsync(waitSeconds, destroyCancellationToken);//floatで時間を渡すためにAwatable
            }


            waitSeconds = 0f;
            if (_characterAnimationContext != null
                && _characterAnimationContext.Signal.TryRequestOneShot(_destroyAniamtionKey, out float destroyDuration))
            {
                waitSeconds = destroyDuration;
            }


            await PlayDeathMaterialEffectAsync();
        }

        /// <summary>
        ///     死亡演出として、対象RendererのMaterialプロパティをLMotionで変化させる。
        /// </summary>
        private ValueTask PlayDeathMaterialEffectAsync()
        {
            if (_deathEffectRenderers == null || _deathEffectRenderers.Length == 0)
            {
                return default;
            }

            // DestroyFadeシェーダーは_DeathEffectAmountのデフォルトが1(通常表示)で、0に近づくほど消滅する。
            MotionHandle handle = LMotion.Create(1f, 0f, _deathEffectDuration)
                .WithEase(Ease.OutQuad)
                .BindToMaterialPropertyBlockFloat(_deathEffectRenderers, DeathEffectPropertyId);

            if (_deathSwampGameObject != null)
            {
                _deathSwampGameObject.SetActive(true);

                handle = LSequence.Create()
                    .Join(handle)
                    .Join(LMotion.Create(Vector3.up * -0.5f, Vector3.up * 0.1f, _deathEffectDuration)
                        .WithEase(Ease.OutQuad)
                        .BindToLocalPosition(_deathSwampGameObject.transform))
                    .Join(LMotion.Create(Vector3.up * 0.1f, Vector3.up * -0.5f, _deathSwampSinkDuration)
                        .WithDelay(_deathEffectDuration)
                        .BindToLocalPosition(_deathSwampGameObject.transform))
                    .Run();
            }

            return handle.ToValueTask(destroyCancellationToken);
        }

        /// <summary>
        ///     プールから再利用した際に、前回の死亡演出で変化したMaterialPropertyBlockを既定値へ戻す。
        /// </summary>
        private void ResetDeathEffect()
        {
            if (_deathEffectRenderers == null || _deathEffectRenderers.Length == 0)
            {
                return;
            }

            foreach (Renderer renderer in _deathEffectRenderers)
            {
                if (renderer == null)
                {
                    continue;
                }

                renderer.SetPropertyBlock(null);
            }
            if (_deathSwampGameObject != null)
            {
                _deathSwampGameObject.SetActive(false);
            }
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
        /// <param name="diedEnemy"> 死亡した敵のEntity。</param>
        private void HandleEnemyDied(CharacterEntity diedEnemy)
        {
            // 撃破演出用に、敵の撃破を通知する。
            EventBus<EOnEnemyDefeated>.Raise(new EOnEnemyDefeated(diedEnemy.Id));

            DieAsync();
        }

        /// <summary>
        ///     敵が攻撃を実行したことをMissionへ通知します。
        /// </summary>
        private void HandleEnemyAttackExecuted()
        {
            _missionEventController?.NotifyActionPerformed(MissionActionKind.EnemyAttack);
        }

        /// <summary>
        ///     ロード済みアセットを解放します。
        /// </summary>
        private void OnDestroy()
        {
            if (_enemyEntity != null)
            {
                _enemyEntity.OnDied -= HandleEnemyDied;
            }

            if (_enemyHealthHudPresenter != null)
            {
                _enemyHealthHudPresenter.OnDamaged -= _view.PlayDamageFeedback;
            }

            _healthHudPresenter?.Dispose();
            _targetingSystem?.UnregisterTarget(_targetable);
            _battleAIRegistry?.Unregister(_aiController);
            _targetable?.Dispose();
            _loadedEnemyData = null;
            _loadedMoveData = null;
            _loadedEncounterMusicData = null;
            _loadedBattleMusicData = null;
            _loadedMissionKeyAsset = null;
        }
    }
}

