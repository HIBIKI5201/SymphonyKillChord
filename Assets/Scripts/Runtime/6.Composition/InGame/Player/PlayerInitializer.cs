using KillChord.Runtime.Adaptor;
using KillChord.Runtime.Adaptor.InGame.Battle;
using KillChord.Runtime.Adaptor.InGame.Player;
using KillChord.Runtime.Adaptor.InGame.Skill;
using KillChord.Runtime.Application;
using KillChord.Runtime.Application.InGame;
using KillChord.Runtime.Application.InGame.Music;
using KillChord.Runtime.Application.InGame.Player;
using KillChord.Runtime.Composition.InGame.Enemy;
using KillChord.Runtime.Domain.InGame.Character;
using KillChord.Runtime.Domain.InGame.Player;
using KillChord.Runtime.InfraStructure.InGame.Character;
using KillChord.Runtime.InfraStructure.InGame.Player;
using KillChord.Runtime.InfraStructure.Player;
using KillChord.Runtime.Utility;
using KillChord.Runtime.View;
using KillChord.Runtime.View.InGame.Player;
using KillChord.Runtime.View.Persistent.Input;
using SymphonyFrameWork.System.ServiceLocate;
using UnityEngine;
using KillChord.Runtime.Adaptor.InGame.Mission;


#if UNITY_EDITOR
using KillChord.Runtime.Composition.InGame.Debugger;
#endif

namespace KillChord.Runtime.Composition
{
    /// <summary>
    ///     プレイヤーに関するクラスの生成と依存関係の解決を行う初期化クラス。
    /// </summary>
    [DefaultExecutionOrder(ExecutionOrderConst.INITIALIZATION)]
    public sealed class PlayerInitializer : MonoBehaviour
    {
        [SerializeField] private PlayerConfig _playerConfig;
        [SerializeField] private PlayerView _player;
        [SerializeField] private SkillRepository _skillRepository;
        [SerializeField] private int _bpm;

        [Space]
        [Header("キャラクターデータ（テスト用）")]
        [SerializeField]
        private CharacterData _playerData;

        private EnemyInfantryTestSpawner _enemyInfantryTestSpawner;
        private EnemyArtilleryTestSpawner _enemyArtilleryTestSpawner;
        private CharacterEntity _playerEntity;
        private MissionEventController _missionEventController;

        private void Awake()
        {
            ServiceLocator.RegisterInstance(this);
        }

        public void Initialize(
            TargetManager targetManager,
            TargetEntityRegistry targetEntityRegistry,
            InputComposition inputComposition)
        {
            if (_player == null)
                Debug.LogError($"{nameof(PlayerView)}がNullです", this);
            _enemyInfantryTestSpawner = ServiceLocator.GetInstance<EnemyInfantryTestSpawner>();
            if (_enemyInfantryTestSpawner == null)
            {
                Debug.LogError($"{nameof(EnemyInfantryTestSpawner)}が見つかりません。シーン内に配置されていることを確認してください。", this);
                return;
            }
            _enemyArtilleryTestSpawner = ServiceLocator.GetInstance<EnemyArtilleryTestSpawner>();
            if (_enemyArtilleryTestSpawner == null)
            {
                Debug.LogError($"{nameof(EnemyArtilleryTestSpawner)}が見つかりません。シーン内に配置されていることを確認してください。", this);
                return; 
            }


            _playerEntity = CharacterFactory.Create(_playerData);

            _missionEventController = ServiceLocator.GetInstance<MissionEventController>();
            if (_missionEventController != null)
            {
                _playerEntity.OnDied += HandlePlayerDied;
            }

            _enemyInfantryTestSpawner.SetTargetEntity(_playerEntity);
            _enemyInfantryTestSpawner.SetTargetManager(targetManager, targetEntityRegistry);
            _enemyArtilleryTestSpawner.SetTargetEntity(_playerEntity);
            _enemyArtilleryTestSpawner.SetTargetManager(targetManager, targetEntityRegistry);

            PlayerMoveParameter parameter = _playerConfig.ToDomain();

            PlayerDodgeMovementApplication dodge = new(parameter);
            dodge.OnDodgeStarted += (float duration) => _playerEntity.SetInvincible(true);
            dodge.OnDodgeEnded += () => _playerEntity.SetInvincible(false);

            PlayerMovement move = new(parameter);
            PlayerApplication application = new(move, dodge);

            PlayerController playerMovementController = new(application, inputComposition.GetBufferedInputBuffer);
            var ct = ServiceLocator.GetInstance<ICameraTransform>().transform;
            var inputView = ServiceLocator.GetInstance<PlayerInputView>();


            TargetSelectorController targetSelectorController = ServiceLocator.GetInstance<TargetSelectorController>();
            if (targetSelectorController == null)
            {
                Debug.LogError($"{nameof(TargetSelectorController)}が見つかりません。シーン内に配置されていることを確認してください。", this);
                return;
            }

            IMusicSyncService musicSyncService = ServiceLocator.GetInstance<IMusicSyncService>();
            if (musicSyncService == null)
            {
                Debug.LogError($"{nameof(IMusicSyncService)}が見つかりません。MusicSyncInitializerが先に実行されているか確認してください。");
                return;
            }

            SkillResultViewModel skillResultViewModel = new SkillResultViewModel();
            Debug.Log($"{skillResultViewModel}作成。");
            SkillResultPresenter skillResultPresenter = new SkillResultPresenter(skillResultViewModel);
            Debug.Log($"{skillResultPresenter}作成。");
            // 仮でシーン内のSkillResultViewを見つけて、ViewModelをバインド
            SkillResultView skillResultView = FindAnyObjectByType<SkillResultView>();
            skillResultView?.Bind(skillResultViewModel);
            SkillController skillController = new SkillController(_skillRepository, musicSyncService, null, skillResultPresenter);


            AttackResultViewModel attackResultViewModel = new AttackResultViewModel();
            AttackResultPresenter attackResultPresenter = new AttackResultPresenter(attackResultViewModel);

            PlayerBattleState playerBattleState = new PlayerBattleState(_playerEntity);

            PlayerAttackController playerAttackController = new PlayerAttackController(attackResultPresenter,
                playerBattleState, skillController, targetSelectorController, musicSyncService);

            _player.Initialize(playerMovementController, playerAttackController, ct, inputView);


#if UNITY_EDITOR
            _player.gameObject
                .AddComponent<PlayerMoveParameterDebug>()
                .SetPlayerMoveParameter(parameter);
#endif
        }

        private void HandlePlayerDied(CharacterEntity _)
        {
            _missionEventController?.NotifyPlayerDead();
        }

        private void OnDestroy()
        {
            if (_playerEntity != null)
            {
                _playerEntity.OnDied -= HandlePlayerDied;
            }
        }
    }
}