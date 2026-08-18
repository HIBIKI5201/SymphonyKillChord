using KillChord.Runtime.Adaptor;
using KillChord.Runtime.Adaptor.InGame.Animation;
using KillChord.Runtime.Adaptor.InGame.Battle;
using KillChord.Runtime.Adaptor.InGame.Mission;
using KillChord.Runtime.Adaptor.InGame.Music;
using KillChord.Runtime.Adaptor.InGame.Player;
using KillChord.Runtime.Adaptor.InGame.Skill;
using KillChord.Runtime.Adaptor.InGame.UI;
using KillChord.Runtime.Adaptor.Persistent.Input;
using KillChord.Runtime.Application.InGame.Battle;
using KillChord.Runtime.Application.InGame.Music;
using KillChord.Runtime.Application.InGame.Player;
using KillChord.Runtime.Composition.InGame.Bootstrap;
using KillChord.Runtime.Composition.InGame.Music;
using KillChord.Runtime.Composition.InGame.Sequence;
using KillChord.Runtime.Composition.InGame.Skill;
using KillChord.Runtime.Composition.InGame.Target;
using KillChord.Runtime.Composition.InGame.UI;
using KillChord.Runtime.Composition.Persistent.Camera;
using KillChord.Runtime.Composition.Persistent.Input;
using KillChord.Runtime.Domain.InGame.Battle;
using KillChord.Runtime.Domain.InGame.Character;
using KillChord.Runtime.Domain.InGame.Player;
using KillChord.Runtime.Domain.InGame.Skill;
using KillChord.Runtime.InfraStructure.Addressables;
using KillChord.Runtime.InfraStructure.InGame.Character;
using KillChord.Runtime.InfraStructure.InGame.Player;
using KillChord.Runtime.Utility.Collections;
using KillChord.Runtime.Utility.Identity;
using KillChord.Runtime.Utility.Persistent;
using KillChord.Runtime.View;
using KillChord.Runtime.View.InGame.Battle;
using KillChord.Runtime.View.InGame.Camera;
using KillChord.Runtime.View.InGame.Player;
using KillChord.Runtime.View.InGame.Skill;
using KillChord.Runtime.View.InGame.UI;
using KillChord.Runtime.View.Persistent.Input;
using SymphonyFrameWork.System.ServiceLocate;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace KillChord.Runtime.Composition.InGame.Player
{
    /// <summary>
    ///     プレイヤーに関するクラスの生成と依存関係の解決を行う初期化クラス。
    /// </summary>
    [DefaultExecutionOrder(ExecutionOrderConst.INITIALIZATION)]
    public sealed class PlayerInitializer : InGameInitializationModuleBase
    {
        /// <summary> モジュール名です。 </summary>
        public override string ModuleName => nameof(PlayerInitializer);

        /// <summary> 実行順です。 </summary>
        public override int Order => 500;

        [SerializeField, Tooltip("プレイヤー移動設定です。")]
        private PlayerMoveSpecAsset _playerConfig;
        [SerializeField, Tooltip("プレイヤーViewプレハブです。")]
        private PlayerView _playerViewPrefab;
        [SerializeField, Tooltip("入力進捗UI設定です。")]
        private SkillInputProgressUIConfig _inputProgressUIConfig;
        [SerializeField, Tooltip("プレイヤー共通のアニメーション設定です。")]
        private CharacterAnimationCatalogConfig _characterAnimationConfig;

        [SerializeField, Tooltip("プレイヤーの攻撃種別ごとのアニメーション設定です。")]
        private PlayerAttackAnimationConfig _playerAttackAnimationConfig;

        [SerializeField, Tooltip("モバイルスティックフリック入力です。")]
        private MobileStickFlickInput _mobileStickFlickInput;

        [SerializeField, SourceDataAddress, Tooltip("モバイルスティックのフリック判定設定の Addressables キーです。")]
        private string _mobileStickFlickInputConfigKey;

        [Space]
        [Header("キャラクターデータ（テスト用）")]
        [SerializeField, SourceDataAddress, Tooltip("キャラクター定義リポジトリの Addressables キーです。")]
        private string _characterRepositoryKey;
        [SerializeField, SourceDataCollection("Character"), Tooltip("プレイヤーが使用するキャラクター定義のIDです。")]
        private DataID _playerCharacterId;
        [Header("装備中スキル（テスト用）")]
        [SerializeField, SourceDataCollection("Skill"), Tooltip("テスト用装備スキルID一覧です。")]
        private DataID[] _equippedSkills;

        private CharacterDefinitionAsset _loadedPlayerData;
        private MobileStickFlickInputConfig _loadedMobileStickFlickInputConfig;

        private Action _onDodgeEndedHandler;
        private ICharacterAnimationSignal _characterAnimationSignal;
        private CharacterEntity _playerEntity;
        private MissionEventController _missionEventController;
        private InGameHudInitializer _inGameHudInitializer;
        private bool _isModuleRegistered;
        private PlayerModuleContainer _moduleContainer;
        private PlayerView _player;
        private PlayerInputView _playerInputView;
        private CameraSystemView _cameraSystemView;
        private SkillView[] _skillVisuals;
        private CharacterAnimationView _characterAnimationView;

        /// <summary> プレイヤーEntityです。 </summary>
        public CharacterEntity PlayerEntity => _playerEntity;

        /// <summary> プレイヤーViewです。 </summary>
        public PlayerView PlayerView => _player;

        /// <summary> スキル演出View一覧です。 </summary>
        public SkillView[] SkillVisuals => _skillVisuals;

        /// <summary> スキル入力進捗UI設定です。 </summary>
        public SkillInputProgressUIConfig SkillInputProgressUIConfig => _inputProgressUIConfig;

        /// <summary> テスト用装備スキルID一覧です。 </summary>
        public SkillId[] EquippedSkillIds => ConvertToSkillIds(_equippedSkills);

        /// <summary>
        ///     ServiceLocatorへ自身を登録します。
        /// </summary>
        private void Awake()
        {
            ServiceLocator.RegisterInstance(this, LocateTypeEnum.Locator);
        }

        /// <summary>
        ///     プレイヤーキャラクター定義とモバイル入力設定を非同期でロードします。
        /// </summary>
        /// <param name="cancellationToken"> キャンセルトークンです。 </param>
        /// <returns> 成功した場合はtrue。 </returns>
        public override async Awaitable<bool> ResourceLoadAsync(CancellationToken cancellationToken)
        {
            CharacterDefinitionRepository characterRepository =
                await _characterRepositoryKey.LoadAssetAsync<CharacterDefinitionRepository>(this, cancellationToken);
            if (characterRepository == null
                || !characterRepository.TryGetAsset(new CharacterDefinitionId(_playerCharacterId.Id), out _loadedPlayerData))
            {
                Debug.LogError($"[{nameof(PlayerInitializer)}] プレイヤーキャラクター定義の解決に失敗しました。", this);
                return false;
            }

#if UNITY_ANDROID || UNITY_EDITOR
            if (_mobileStickFlickInput != null)
            {
                _loadedMobileStickFlickInputConfig =
                    await _mobileStickFlickInputConfigKey.LoadAssetAsync<MobileStickFlickInputConfig>(this, cancellationToken);
                if (_loadedMobileStickFlickInputConfig == null)
                {
                    Debug.LogError(
                        $"[{nameof(PlayerInitializer)}] {nameof(MobileStickFlickInputConfig)} のロードに失敗しました。",
                        this);
                    return false;
                }
            }
#endif

            return true;
        }

        /// <summary>
        ///     プレイヤーモジュールのContainerを登録する。
        /// </summary>
        /// <returns> 成功した場合はtrue。 </returns>
        public override bool Build()
        {
            if (!ValidateBuildReferences())
            {
                return false;
            }

            if (!ServiceLocator.TryGetInstance(out PlayerStatusBonusModuleContainer playerStatusBonusContainer))
            {
                Debug.LogError(
                    $"[{nameof(PlayerInitializer)}] {nameof(PlayerStatusBonusModuleContainer)} が見つかりません。",
                    this);
                return false;
            }

            if (!TryInstantiatePlayerView(out Transform spawnPointTransform))
            {
                return false;
            }

            _playerEntity = CharacterFactory.Create(
                _loadedPlayerData,
                playerStatusBonusContainer.PlayerStatusBonus.MaxHealthMultiplier,
                playerStatusBonusContainer.PlayerStatusBonus.AttackPowerMultiplier,
                playerStatusBonusContainer.PlayerStatusBonus.CriticalChanceAddition,
                playerStatusBonusContainer.PlayerStatusBonus.CriticalMultiplierAddition);
            _playerEntity.OnDamageAvoided += HandleDamageAvoided;
            _playerEntity.OnHealthChanged += HandlePlayerHealthChanged;

            _player.transform.SetPositionAndRotation(
                spawnPointTransform.position,
                spawnPointTransform.rotation);
            _moduleContainer = new PlayerModuleContainer(
                this,
                _player,
                _playerEntity,
                playerStatusBonusContainer.PlayerStatusBonus);
            ServiceLocator.RegisterInstance(_moduleContainer);
            _isModuleRegistered = true;
            return _player != null && _playerEntity != null;
        }

        /// <summary>
        ///     他モジュールと結合してプレイヤーを初期化する。
        /// </summary>
        /// <returns> 成功した場合はtrue。 </returns>
        public override bool Ready()
        {
            SceneDependencyModuleContainer sceneDependencyContainer = ServiceLocator.GetInstance<SceneDependencyModuleContainer>();
            if (sceneDependencyContainer == null)
            {
                Debug.LogError($"[{nameof(PlayerInitializer)}] {nameof(SceneDependencyModuleContainer)} が見つかりません。", this);
                return false;
            }

            SkillModuleContainer skillModuleContainer = ServiceLocator.GetInstance<SkillModuleContainer>();
            if (skillModuleContainer == null ||
                skillModuleContainer.SkillController == null ||
                skillModuleContainer.PendingAttackEffectService == null)
            {
                Debug.LogError($"[{nameof(PlayerInitializer)}] {nameof(SkillModuleContainer)} が見つかりません。", this);
                return false;
            }

            Initialize(
                sceneDependencyContainer.InputComposition,
                skillModuleContainer.SkillController,
                skillModuleContainer.PendingAttackEffectService);

            InGamePlayDirector inGamePlayDirector = FindFirstObjectByType<InGamePlayDirector>();
            if (inGamePlayDirector != null && _player != null)
            {
                inGamePlayDirector.AddGamePlayControllable(_player);
            }

            return _playerEntity != null;
        }

        /// <summary>
        ///     他モジュールと結合してPlayerViewを初期化します。
        /// </summary>
        /// <param name="inputComposition"> 入力Compositionです。 </param>
        /// <param name="skillController"> スキルControllerです。 </param>
        /// <param name="pendingAttackEffectService"> スキル攻撃の演出を管理するサービスです。 </param>
        public void Initialize(
            InputComposition inputComposition,
            SkillController skillController,
            PendingAttackEffectService pendingAttackEffectService)
        {
            if (_player == null)
            {
                Debug.LogError($"{nameof(PlayerView)} がNullです", this);
                return;
            }

            _inGameHudInitializer = ServiceLocator.GetInstance<InGameHudInitializer>();
            if (_inGameHudInitializer == null)
            {
                Debug.LogError($"{nameof(InGameHudInitializer)} が見つかりません。シーン内に配置されていることを確認してください。", this);
                return;
            }

            _missionEventController = ServiceLocator.GetInstance<MissionEventController>();
            if (_missionEventController != null)
            {
                _playerEntity.OnDied += HandlePlayerDied;
            }

            MusicSyncModuleContainer musicSyncContainer = ServiceLocator.GetInstance<MusicSyncModuleContainer>();
            if (musicSyncContainer == null)
            {
                Debug.LogError($"{nameof(MusicSyncModuleContainer)} が見つかりません。", this);
                return;
            }

            MusicSyncState musicSyncState = musicSyncContainer.MusicSyncState;
            if (musicSyncState == null)
            {
                Debug.LogError($"{nameof(MusicSyncState)} が見つかりません。ServiceLocatorに登録されているか確認してください。", this);
                return;
            }

            PlayerMoveSpec parameter = _playerConfig.ToDomain();
            ICameraTransform cameraTransform = ServiceLocator.GetInstance<ICameraTransform>();
            PlayerInputView inputView = ServiceLocator.GetInstance<PlayerInputView>();
            if (cameraTransform == null || inputView == null)
            {
                Debug.LogError($"[{nameof(PlayerInitializer)}] カメラまたは入力Viewが見つかりません。", this);
                return;
            }

            // 位置リセット入力を購読する。
            _playerInputView = inputView;
            _playerInputView.OnResetPositionInput += HandleResetPositionInput;

            TargetSystemModuleContainer targetSystemContainer = ServiceLocator.GetInstance<TargetSystemModuleContainer>();
            if (targetSystemContainer == null || targetSystemContainer.TargetSystemController == null)
            {
                Debug.LogError($"{nameof(TargetSystemModuleContainer)} が見つかりません。", this);
                return;
            }

            IMusicSyncService musicSyncService = musicSyncContainer.MusicSyncService;
            if (musicSyncService == null)
            {
                Debug.LogError($"{nameof(IMusicSyncService)} が見つかりません。MusicSyncInitializerが先に実行されているか確認してください。", this);
                return;
            }

            AttackResultViewModel attackResultViewModel = new AttackResultViewModel();
            AttackResultPresenter attackResultPresenter = new AttackResultPresenter(attackResultViewModel);
            PlayerBattleState playerBattleState = new PlayerBattleState(_playerEntity);
            PlayerActionRestrictionState actionRestrictionState = new PlayerActionRestrictionState();
            AttackIntervalEvaluator attackIntervalEvaluator = new AttackIntervalEvaluator(_playerEntity.AttackIntervalEntity);
            PlayerAttackController playerAttackController = new PlayerAttackController(
                attackResultPresenter,
                playerBattleState,
                actionRestrictionState,
                skillController,
                targetSystemContainer.TargetSystemController,
                attackIntervalEvaluator,
                musicSyncService,
                musicSyncState,
                targetSystemContainer.TargetAreaQuery,
                _player.transform,
                pendingAttackEffectService,
                (float)parameter.AttackRotationSpeed,
                (float)parameter.AttackCooldown.Value);
            _moduleContainer.SetActionRestrictionState(actionRestrictionState);
            _moduleContainer.SetPlayerAttackController(playerAttackController);

            IHealthHudViewModel healthHudViewModel = new HealthHudViewModel(_playerEntity.CurrentHealth.Value, _playerEntity.MaxHealth.Value);
            PlayerHealthHudPresenter healthHudPresenter = new PlayerHealthHudPresenter(_playerEntity, healthHudViewModel);

            AnimationComposition animationComposition = new AnimationComposition();
            ICharacterAnimationViewContext animationContext = animationComposition.Init(
                _characterAnimationView,
                _characterAnimationConfig,
                musicSyncState,
                _playerAttackAnimationConfig);

            PlayerDodgeMovementApplication dodge = new PlayerDodgeMovementApplication(parameter);
            dodge.OnDodgeStarted += (duration, direction) =>
            {
                _playerEntity.SetInvincible(true);
                _player.PlayDodgeMaterialEffect(duration, direction);
            };
            dodge.OnDodgeEnded += () =>
            {
                _playerEntity.SetInvincible(false);
                _player.ResetDodgeMaterialEffect();
            };

            _onDodgeEndedHandler = () => playerAttackController.StartAttackCooldown();
            _characterAnimationSignal = animationContext.Signal;
            _characterAnimationSignal.OnDodgeEnded += _onDodgeEndedHandler;

            PlayerMovementApplication move = new PlayerMovementApplication(parameter);
            PlayerApplication application = new PlayerApplication(move, dodge);
            PlayerController playerMovementController = new PlayerController(application, inputComposition.GetBufferedInputBuffer);
            _moduleContainer.SetPlayerController(playerMovementController);

            PlayerInputSuppressionState inputSuppressionState = new PlayerInputSuppressionState();
            _moduleContainer.SetInputSuppressionState(inputSuppressionState);

            _player.Initialize(
                playerMovementController,
                playerAttackController,
                animationContext,
                musicSyncState,
                cameraTransform.Transform,
                inputView,
                healthHudPresenter,
                inputSuppressionState);

            InitializeMobileStickFlickInput(inputView);

            _inGameHudInitializer.InitializePlayerHpHud(healthHudViewModel);

#if UNITY_EDITOR
            _player.gameObject
                .AddComponent<PlayerMoveSpecDebug>()
                .SetPlayerMoveSpec(parameter);
#endif
        }

        /// <summary>
        ///     プレイヤーをステージのスタート地点へ戻します。
        /// </summary>
        public void ResetPlayerToSpawn()
        {
            if (_player == null)
            {
                Debug.LogError($"[{nameof(PlayerInitializer)}] {nameof(PlayerView)} が存在しないため位置リセットできません。", this);
                return;
            }

            if (!TryResolvePlayerSpawnPointTransform(out Transform spawnPointTransform))
            {
                Debug.LogError($"[{nameof(PlayerInitializer)}] スタート地点が見つからないため位置リセットできません。", this);
                return;
            }

            _player.ResetToSpawn(spawnPointTransform.position, spawnPointTransform.rotation);

            // カメラの向きもスタート時の前方へ戻す。
            ResetCameraOrientation(spawnPointTransform.forward);
        }

        /// <summary>
        ///     カメラの向きを指定した前方へ戻します。
        /// </summary>
        /// <param name="forward"> カメラを向ける前方(ワールド空間)です。 </param>
        private void ResetCameraOrientation(Vector3 forward)
        {
            if (_cameraSystemView == null)
            {
                _cameraSystemView = FindFirstObjectByType<CameraSystemView>();
            }

            if (_cameraSystemView == null)
            {
                Debug.LogWarning($"[{nameof(PlayerInitializer)}] {nameof(CameraSystemView)} が見つからないためカメラの向きをリセットできません。", this);
                return;
            }

            _cameraSystemView.ResetOrientation(forward);
        }

        /// <summary>
        ///     位置リセット入力を受け取ってプレイヤーをスタート地点へ戻します。
        /// </summary>
        /// <param name="input"> 位置リセット入力です。 </param>
        private void HandleResetPositionInput(InputContext<float> input)
        {
            // 押下開始時のみ実行する。
            if (input.Phase != InputActionPhase.Started)
            {
                return;
            }

            ResetPlayerToSpawn();
        }

        /// <summary>
        ///     回避成功時の演出を再生します。
        /// </summary>
        /// <param name="damage"> 回避したダメージです。 </param>
        private void HandleDamageAvoided(Damage damage)
        {
            _player?.PlayDodgeSuccessFeedback();
        }

        /// <summary>
        ///     プレイヤー死亡をミッションへ通知します。
        /// </summary>
        /// <param name="_"> 死亡したプレイヤーです。 </param>
        private void HandlePlayerDied(CharacterEntity _)
        {
            _missionEventController?.NotifyPlayerDead();
        }

        /// <summary>
        ///     プレイヤーのHP変化を受け取り、被弾時のみ演出用イベントを通知します。
        /// </summary>
        /// <param name="currentHealth"> 変化後の現在HPです。 </param>
        /// <param name="maxHealth"> 最大HPです。 </param>
        /// <param name="amountChanged"> HPの変化量です。ダメージは負、回復は正になります。 </param>
        private void HandlePlayerHealthChanged(float currentHealth, float maxHealth, float amountChanged)
        {
            // 回復では演出を出さないため、減少時のみ通知する。
            if (amountChanged >= 0f)
            {
                return;
            }

            // 被弾演出用に、プレイヤーの被弾を正の値へ直して通知する。
            EventBus<EOnPlayerTakeDamage>.Raise(new EOnPlayerTakeDamage(-amountChanged));
        }

        /// <summary>
        ///     破棄時の購読解除を行います。
        /// </summary>
        private void OnDestroy()
        {
            UninitializeMobileStickFlickInput();

            if (_playerInputView != null)
            {
                _playerInputView.OnResetPositionInput -= HandleResetPositionInput;
                _playerInputView = null;
            }

            if (_characterAnimationSignal != null && _onDodgeEndedHandler != null)
            {
                _characterAnimationSignal.OnDodgeEnded -= _onDodgeEndedHandler;
                _onDodgeEndedHandler = null;
                _characterAnimationSignal = null;
            }

            ServiceLocator.UnregisterInstance(this);
            if (_isModuleRegistered)
            {
                ServiceLocator.UnregisterInstance<PlayerModuleContainer>();
                _moduleContainer = null;
                _isModuleRegistered = false;
            }

            if (_playerEntity != null)
            {
                _playerEntity.OnDied -= HandlePlayerDied;
                _playerEntity.OnDamageAvoided -= HandleDamageAvoided;
                _playerEntity.OnHealthChanged -= HandlePlayerHealthChanged;
            }
        }

        /// <summary>
        ///     登録済みContainerを解除する。
        /// </summary>
        public override void Shutdown()
        {
            UninitializeMobileStickFlickInput();
#if UNITY_ANDROID || UNITY_EDITOR
            _mobileStickFlickInputConfigKey.ReleaseLoadedAsset(this);
            _loadedMobileStickFlickInputConfig = null;
#endif
            _characterRepositoryKey.ReleaseLoadedAsset(this);
            _loadedPlayerData = null;

            if (!_isModuleRegistered)
            {
                return;
            }

            ServiceLocator.UnregisterInstance<PlayerModuleContainer>();
            _moduleContainer = null;
            _isModuleRegistered = false;
        }

        /// <summary>
        ///     Androidの仮想スティックフリック入力を入力Viewへ接続する。
        /// </summary>
        /// <param name="inputView"> フリック入力の通知先。 </param>
        private void InitializeMobileStickFlickInput(PlayerInputView inputView)
        {
#if UNITY_ANDROID || UNITY_EDITOR
            if (_mobileStickFlickInput == null)
            {
#if UNITY_ANDROID
                Debug.LogWarning(
                    $"[{nameof(PlayerInitializer)}] {nameof(MobileStickFlickInput)} が設定されていません。",
                    this);
#endif
                return;
            }

            _mobileStickFlickInput.Initialize(inputView, _loadedMobileStickFlickInputConfig);
#endif
        }

        /// <summary>
        ///     仮想スティックフリック入力と入力Viewの接続を解除する。
        /// </summary>
        private void UninitializeMobileStickFlickInput()
        {
#if UNITY_ANDROID || UNITY_EDITOR
            _mobileStickFlickInput?.Uninitialize();
#endif
        }

        /// <summary>
        ///     DataID配列をSkillId配列へ変換します。
        /// </summary>
        /// <param name="dataIds"> 変換元のDataID配列です。 </param>
        /// <returns> 変換後のSkillId配列です。 </returns>
        private static SkillId[] ConvertToSkillIds(DataID[] dataIds)
        {
            if (dataIds == null || dataIds.Length == 0)
            {
                return Array.Empty<SkillId>();
            }

            List<SkillId> ids = new List<SkillId>(dataIds.Length);
            for (int i = 0; i < dataIds.Length; i++)
            {
                if (dataIds[i].Id == 0)
                {
                    continue;
                }

                ids.Add(new SkillId(dataIds[i].Id));
            }

            return ids.ToArray();
        }

        /// <summary>
        ///     Buildフェーズで必要な参照を検証します。
        /// </summary>
        /// <returns> 参照が有効な場合はtrue。 </returns>
        private bool ValidateBuildReferences()
        {
            if (_playerConfig == null
                || _playerViewPrefab == null
                || _loadedPlayerData == null
                || _characterAnimationConfig == null
                || _playerAttackAnimationConfig == null)
            {
                Debug.LogError($"[{nameof(PlayerInitializer)}] プレイヤー初期化参照が不足しています。", this);
                return false;
            }

            return true;
        }

        /// <summary>
        ///     ステージ側のスポーン地点へプレイヤーViewを生成します。
        /// </summary>
        /// <param name="spawnPointTransform"> 使用したスポーン地点です。 </param>
        /// <returns> 生成に成功した場合はtrue。 </returns>
        private bool TryInstantiatePlayerView(out Transform spawnPointTransform)
        {
            if (!TryResolvePlayerSpawnPointTransform(out spawnPointTransform))
            {
                Debug.LogError($"[{nameof(PlayerInitializer)}] {nameof(IStageSceneInstance)} または PlayerSpawnPoint が見つかりません。", this);
                return false;
            }

            _player = Instantiate(
                _playerViewPrefab,
                spawnPointTransform.position,
                spawnPointTransform.rotation);
            _player.name = _playerViewPrefab.name;
            SceneManager.MoveGameObjectToScene(_player.gameObject, gameObject.scene);

            _skillVisuals = _player.GetComponentsInChildren<SkillView>(true);
            if (!TryResolveCharacterAnimationView(out _characterAnimationView))
            {
                Destroy(_player.gameObject);
                _player = null;
                Debug.LogError($"[{nameof(PlayerInitializer)}] {nameof(CharacterAnimationView)} の取得に失敗しました。", this);
                return false;
            }

            return true;
        }

        /// <summary>
        ///     プレイヤー生成位置のTransformを解決します。
        /// </summary>
        /// <param name="spawnPointTransform"> 解決結果です。 </param>
        /// <returns> 解決に成功した場合はtrue。 </returns>
        private bool TryResolvePlayerSpawnPointTransform(out Transform spawnPointTransform)
        {
            spawnPointTransform = null;

            IStageSceneInstance stageSceneInstance = ServiceLocator.GetInstance<IStageSceneInstance>();
            if (stageSceneInstance == null || stageSceneInstance.PlayerSpawnPointTransform == null)
            {
                return false;
            }

            spawnPointTransform = stageSceneInstance.PlayerSpawnPointTransform;
            return true;
        }

        /// <summary>
        ///     プレイヤー配下の CharacterAnimationView を解決します。
        /// </summary>
        /// <param name="characterAnimationView"> 解決したViewです。 </param>
        /// <returns> 解決に成功した場合はtrue。 </returns>
        private bool TryResolveCharacterAnimationView(out CharacterAnimationView characterAnimationView)
        {
            characterAnimationView = _player.GetComponentInChildren<CharacterAnimationView>(true);
            if (characterAnimationView != null)
            {
                return true;
            }

            Animator animator = _player.GetComponentInChildren<Animator>(true);
            if (animator == null)
            {
                return false;
            }

            characterAnimationView = animator.GetComponent<CharacterAnimationView>();
            if (characterAnimationView != null)
            {
                return true;
            }

            characterAnimationView = animator.gameObject.AddComponent<CharacterAnimationView>();
            return characterAnimationView != null;
        }
    }
}
