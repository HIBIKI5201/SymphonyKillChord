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
using KillChord.Runtime.InfraStructure.Addressables;
using KillChord.Runtime.InfraStructure.InGame.Character;
using KillChord.Runtime.InfraStructure.InGame.Enemy;
using KillChord.Runtime.InfraStructure.InGame.Mission;
using KillChord.Runtime.Utility.Identity;
using KillChord.Runtime.Utility.Rendering;
using KillChord.Runtime.View;
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
        ///     敵用 Addressables アセットをロードします。
        /// </summary>
        /// <param name="cancellationToken"> キャンセルトークンです。 </param>
        /// <returns> 成功した場合はtrue。 </returns>
        public async Task<bool> LoadAddressableAssetsAsync(CancellationToken cancellationToken)
        {
            try
            {
                _loadedEnemyData = await _enemyDataKey.LoadAssetAsync<CharacterDefinitionAsset>(this, cancellationToken);
                _loadedMoveData = await _moveDataKey.LoadAssetAsync<EnemyMoveSpecAsset>(this, cancellationToken);
                _loadedEncounterMusicData = await _encounterMusicDataKey.LoadAssetAsync<EnemyMusicSpecAsset>(this, cancellationToken);
                _loadedBattleMusicData = await _battleMusicDataKey.LoadAssetAsync<EnemyMusicSpecAsset>(this, cancellationToken);
                EnemyMissionKeyRepository missionKeyRepository =
                    await _missionKeyRepositoryKey.LoadAssetAsync<EnemyMissionKeyRepository>(this, cancellationToken);
                missionKeyRepository?.TryGetAsset(new EnemyMissionKey(_missionKeyId.Id), out _loadedMissionKeyAsset);
            }
            catch (Exception ex) { Debug.LogException(ex, this); }

            return _loadedEnemyData != null
                && _loadedMoveData != null
                && _loadedEncounterMusicData != null
                && _loadedBattleMusicData != null
                && _loadedMissionKeyAsset != null
                && _characterAnimationConfig != null;
        }

        /// <summary>
        ///     ロード済みアセット参照を別インスタンスへコピーします。
        /// </summary>
        /// <param name="source"> コピー元です。 </param>
        public void CopyLoadedAssetsFrom(EnemyLifeCycle source)
        {
            _loadedEnemyData = source._loadedEnemyData;
            _loadedMoveData = source._loadedMoveData;
            _loadedEncounterMusicData = source._loadedEncounterMusicData;
            _loadedBattleMusicData = source._loadedBattleMusicData;
            _loadedMissionKeyAsset = source._loadedMissionKeyAsset;
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

            _targetingSystem = targetingSystem;
            _enemyEntity = CharacterFactory.Create(_loadedEnemyData);
            _waveSpawnerState = waveSpawnerState;

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
            IHealthHudPresenter healthHudPresenter = new EnemyHealthHudPresenter(_enemyEntity, viewModel, _healthView);
            _healthHudPresenter = healthHudPresenter;

            _targetable = new TransformTargetable(_enemyEntity.Id, transform);

            // View接続
            var animationComposition = new AnimationComposition();
            ICharacterAnimationViewContext animationContext =
                animationComposition.Init(_characterAnimationView, _characterAnimationConfig, musicSyncState);
            _characterAnimationContext = animationContext;
            _view.Initialize(aiController, target, animationContext, musicSyncState);
            _healthView.Bind(viewModel);
            _healthView.Initialize(healthHudPresenter);
            _raycastView.Initialize(target, spec.AttackRangeMax.Value);
            _aiController.On1BeatBefore += _raycastView.LockWarningDirection;
            _aiController.On2BeatBefore += _raycastView.StartTrackingWarning;
            _aiController.OnAttack += _raycastView.HideWarning;
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
        private ICharacterAnimationViewContext _characterAnimationContext;

        [SerializeField, SourceDataAddress, Tooltip("敵定義の Addressables キーです。")] private string _enemyDataKey;
        [SerializeField, SourceDataAddress, Tooltip("敵移動仕様の Addressables キーです。")] private string _moveDataKey;
        [SerializeField, SourceDataAddress, Tooltip("遭遇演出音楽仕様の Addressables キーです。")] private string _encounterMusicDataKey;
        [SerializeField, SourceDataAddress, Tooltip("戦闘音楽仕様の Addressables キーです。")] private string _battleMusicDataKey;

        [SerializeField] private int _attackIndex;

        [SerializeField] private EnemyMoveView _view;
        [SerializeField] private EnemyHealthView _healthView;
        [SerializeField] private EnemyRaycastDetectView _raycastView;
        [SerializeField] private NearestAttackPositionSearchView _attackPositionSearchView;
        [SerializeField, SourceDataAddress, Tooltip("敵ミッションキーリポジトリの Addressables キーです。")] private string _missionKeyRepositoryKey;
        [SerializeField, SourceDataCollection("EnemyMissionKey"), Tooltip("この敵が対応する敵ミッションキーのIDです。")] private DataID _missionKeyId;
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
        private EnemyBattleState _battleState;
        private EnemyWaveSpawnerState _waveSpawnerState;
        private bool _isDying;

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

            _targetingSystem?.UnregisterTarget(_targetable);
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

            if (waitSeconds > 0f)
            {
                await Awaitable.WaitForSecondsAsync(waitSeconds, destroyCancellationToken);
            }
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
        /// <param name="_"></param>
        private void HandleEnemyDied(CharacterEntity _)
        {
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

            _targetingSystem?.UnregisterTarget(_targetable);
            _targetable?.Dispose();
            _enemyDataKey.ReleaseLoadedAsset(this);
            _moveDataKey.ReleaseLoadedAsset(this);
            _encounterMusicDataKey.ReleaseLoadedAsset(this);
            _battleMusicDataKey.ReleaseLoadedAsset(this);
            _missionKeyRepositoryKey.ReleaseLoadedAsset(this);
            _loadedEnemyData = null;
            _loadedMoveData = null;
            _loadedEncounterMusicData = null;
            _loadedBattleMusicData = null;
            _loadedMissionKeyAsset = null;
        }
    }
}

