using KillChord.Runtime.Adaptor.InGame.Animation;
using KillChord.Runtime.Adaptor.InGame.Battle;
using KillChord.Runtime.Adaptor.InGame.Mission;
using KillChord.Runtime.Adaptor.InGame.Music;
using KillChord.Runtime.Adaptor.InGame.Player;
using KillChord.Runtime.Adaptor.InGame.Skill;
using KillChord.Runtime.Adaptor.InGame.UI;
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
using KillChord.Runtime.InfraStructure.Player;
using KillChord.Runtime.Domain.InGame.Player;
using KillChord.Runtime.InfraStructure;
using KillChord.Runtime.InfraStructure.InGame.Character;
using KillChord.Runtime.InfraStructure.InGame.Player;
using KillChord.Runtime.InfraStructure.InGame.Skill;
using KillChord.Runtime.Utility.Collections;
using KillChord.Runtime.View;
using KillChord.Runtime.View.InGame.Battle;
using KillChord.Runtime.View.InGame.Player;
using KillChord.Runtime.View.InGame.Skill;
using KillChord.Runtime.View.InGame.UI;
using KillChord.Runtime.View.Persistent.Input;
using SymphonyFrameWork.System.ServiceLocate;
using System;
using UnityEngine;
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
        private SkillInputProgressViewConfigAsset _inputProgressViewConfigAsset;
        [SerializeField, Tooltip("キャラクターアニメーション設定です。")]
        private CharacterAnimationCatalogAsset _characterAnimationCatalogAsset;

        [Space]
        [Header("キャラクターデータ（テスト用）")]
        [SerializeField, Tooltip("プレイヤー定義アセットです。")]
        private CharacterDefinitionAsset _playerData;
        [Header("装備中スキル（テスト用）")]
        [SerializeField, Tooltip("テスト用装備スキル一覧です。")]
        private SkillTemplateAsset[] _equippedSkills;

        private Action _onDodgeEndedHandler;
        private ICharacterAnimationSignal _characterAnimationSignal;
        private CharacterEntity _playerEntity;
        private MissionEventController _missionEventController;
        private InGameHudInitializer _inGameHudInitializer;
        private bool _isModuleRegistered;
        private PlayerModuleContainer _moduleContainer;
        private PlayerView _player;
        private SkillView[] _skillVisuals;
        private CharacterAnimationView _characterAnimationView;

        /// <summary> プレイヤーEntityです。 </summary>
        public CharacterEntity PlayerEntity => _playerEntity;

        /// <summary> プレイヤーViewです。 </summary>
        public PlayerView PlayerView => _player;

        /// <summary> スキル演出View一覧です。 </summary>
        public SkillView[] SkillVisuals => _skillVisuals;

        /// <summary> スキル入力進捗UI設定です。 </summary>
        public SkillInputProgressViewConfigAsset SkillInputProgressViewConfigAsset => _inputProgressViewConfigAsset;

        /// <summary> テスト用装備スキル一覧です。 </summary>
        public SkillTemplateAsset[] EquippedSkillAssets => _equippedSkills;

        /// <summary>
        ///     ServiceLocatorへ自身を登録します。
        /// </summary>
        private void Awake()
        {
            ServiceLocator.RegisterInstance(this, LocateType.Locator);
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

            if (!TryInstantiatePlayerView(out Transform spawnPointTransform))
            {
                return false;
            }

            _playerEntity = CharacterFactory.Create(_playerData);
            _playerEntity.OnDamageAvoided += HandleDamageAvoided;

            _player.transform.SetPositionAndRotation(
                spawnPointTransform.position,
                spawnPointTransform.rotation);
            _moduleContainer = new PlayerModuleContainer(this, _player, _playerEntity);
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
            if (skillModuleContainer == null || skillModuleContainer.SkillController == null)
            {
                Debug.LogError($"[{nameof(PlayerInitializer)}] {nameof(SkillModuleContainer)} が見つかりません。", this);
                return false;
            }

            Initialize(sceneDependencyContainer.InputComposition, skillModuleContainer.SkillController);

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
        public void Initialize(InputComposition inputComposition, SkillController skillController)
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
            AttackIntervalEvaluator attackIntervalEvaluator = new AttackIntervalEvaluator(_playerEntity.AttackIntervalEntity);
            PlayerAttackController playerAttackController = new PlayerAttackController(
                attackResultPresenter,
                playerBattleState,
                skillController,
                targetSystemContainer.TargetSystemController,
                attackIntervalEvaluator,
                musicSyncService,
                musicSyncState,
                (float)parameter.AttackRotationSpeed,
                (float)parameter.AttackCooldown.Value,
                (int)_playerEntity.BaseDamage.Value);
            _moduleContainer.SetPlayerAttackController(playerAttackController);

            IHealthHudViewModel healthHudViewModel = new HealthHudViewModel(_playerEntity.CurrentHealth.Value, _playerEntity.MaxHealth.Value);
            PlayerHealthHudPresenter healthHudPresenter = new PlayerHealthHudPresenter(_playerEntity, healthHudViewModel);

            AnimationComposition animationComposition = new AnimationComposition();
            ICharacterAnimationViewContext animationContext = animationComposition.Init(_characterAnimationView, _characterAnimationCatalogAsset, musicSyncState);

            PlayerDodgeMovementApplication dodge = new PlayerDodgeMovementApplication(parameter);
            dodge.OnDodgeStarted += duration => _playerEntity.SetInvincible(true);
            dodge.OnDodgeEnded += () => _playerEntity.SetInvincible(false);

            _onDodgeEndedHandler = () => playerAttackController.StartAttackCooldown();
            _characterAnimationSignal = animationContext.Signal;
            _characterAnimationSignal.OnDodgeEnded += _onDodgeEndedHandler;

            PlayerMovementApplication move = new PlayerMovementApplication(parameter);
            PlayerApplication application = new PlayerApplication(move, dodge);
            PlayerController playerMovementController = new PlayerController(application, inputComposition.GetBufferedInputBuffer);

            _player.Initialize(
                playerMovementController,
                playerAttackController,
                animationContext,
                musicSyncState,
                cameraTransform.Transform,
                inputView,
                healthHudPresenter);

            _inGameHudInitializer.InitializePlayerHpHud(healthHudViewModel);

#if UNITY_EDITOR
            _player.gameObject
                .AddComponent<PlayerMoveSpecDebug>()
                .SetPlayerMoveSpec(parameter);
#endif
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
        ///     破棄時の購読解除を行います。
        /// </summary>
        private void OnDestroy()
        {
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
            }
        }

        /// <summary>
        ///     登録済みContainerを解除する。
        /// </summary>
        public override void Shutdown()
        {
            if (!_isModuleRegistered)
            {
                return;
            }

            ServiceLocator.UnregisterInstance<PlayerModuleContainer>();
            _moduleContainer = null;
            _isModuleRegistered = false;
        }

        /// <summary>
        ///     Buildフェーズで必要な参照を検証します。
        /// </summary>
        /// <returns> 参照が有効な場合はtrue。 </returns>
        private bool ValidateBuildReferences()
        {
            if (_playerConfig == null
                || _playerViewPrefab == null
                || _playerData == null
                || _characterAnimationCatalogAsset == null)
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
