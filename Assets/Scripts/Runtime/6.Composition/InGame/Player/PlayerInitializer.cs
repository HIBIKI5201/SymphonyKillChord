using KillChord.Runtime.Adaptor;
using KillChord.Runtime.Adaptor.InGame.Battle;
using KillChord.Runtime.Adaptor.InGame.Player;
using KillChord.Runtime.Adaptor.InGame.Skill;
using KillChord.Runtime.Application;
using KillChord.Runtime.Application.InGame;
using KillChord.Runtime.Application.InGame.Music;
using KillChord.Runtime.Application.InGame.Player;
using KillChord.Runtime.Composition.InGame.Enemy;
using KillChord.Runtime.Composition.InGame.Player;
using KillChord.Runtime.Domain.InGame.Character;
using KillChord.Runtime.Domain.InGame.Player;
using KillChord.Runtime.InfraStructure.InGame.Battle;
using KillChord.Runtime.InfraStructure.InGame.Character;
using KillChord.Runtime.InfraStructure.InGame.Player;
using KillChord.Runtime.InfraStructure.Player;
using KillChord.Runtime.Utility;
using KillChord.Runtime.View;
using KillChord.Runtime.View.InGame.Player;
using SymphonyFrameWork.System.ServiceLocate;
using UnityEngine;

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
        [SerializeField] private MusicSyncService _musicSyncService;
        [SerializeField] private int _bpm;

        [Space]
        [Header("キャラクターデータ（テスト用）")]
        [SerializeField]
        private CharacterData _playerData;

        [SerializeField] private CharacterData _enemyData;

        private EnemyTestSpawner _enemyTestSpawner;

        private void Awake()
        {
            ServiceLocator.RegisterInstance(this);
        }

        public void Initialize(TargetManager targetManager, TargetEntityRegistry targetEntityRegistry)
        {
            if (_player == null)
                Debug.LogError($"{nameof(PlayerView)}がNullです", this);
            _enemyTestSpawner = ServiceLocator.GetInstance<EnemyTestSpawner>();

            CharacterEntity player = CharacterFactory.Create(_playerData);
            _enemyTestSpawner.SetTargetEntity(player);
            _enemyTestSpawner.SetTargetManager(targetManager, targetEntityRegistry);


            PlayerMoveParameter parameter = _playerConfig.ToDomain();

            PlayerDodgeMovementApplication dodge = new(parameter);
            PlayerMovement move = new(parameter);
            PlayerApplication application = new(move, dodge);

            PlayerController playerMovementController = new(application);
            var ct = ServiceLocator.GetInstance<CameraTransform>().transform;


            TargetSelectorController targetSelectorController = ServiceLocator.GetInstance<TargetSelectorController>();
            IMusicSyncService _musicSyncService = new MusicSyncService(new(_bpm));
            SkillController skillController = new SkillController(_skillRepository, _musicSyncService);
            AttackResultViewModel attackResultViewModel = new AttackResultViewModel();
            AttackResultPresenter attackResultPresenter = new AttackResultPresenter(attackResultViewModel);

            PlayerBattleState playerBattleState = new PlayerBattleState(player);

            PlayerAttackController playerAttackController = new PlayerAttackController(attackResultPresenter,playerBattleState,skillController,targetSelectorController);

            _player.Init(playerMovementController, playerAttackController, ct);


#if UNITY_EDITOR
            _player.gameObject
                .AddComponent<PlayerMoveParameterDebug>()
                .SetPlayerMoveParameter(parameter);
#endif
        }
    }
}