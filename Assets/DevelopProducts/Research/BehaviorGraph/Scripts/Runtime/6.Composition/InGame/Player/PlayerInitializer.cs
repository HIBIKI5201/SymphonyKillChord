using DevelopProducts.BehaviorGraph.Runtime.Adaptor;
using DevelopProducts.BehaviorGraph.Runtime.Adaptor.InGame.Battle;
using DevelopProducts.BehaviorGraph.Runtime.Adaptor.InGame.Player;
using DevelopProducts.BehaviorGraph.Runtime.Adaptor.InGame.Skill;
using DevelopProducts.BehaviorGraph.Runtime.Application;
using DevelopProducts.BehaviorGraph.Runtime.Application.InGame;
using DevelopProducts.BehaviorGraph.Runtime.Application.InGame.Music;
using DevelopProducts.BehaviorGraph.Runtime.Application.InGame.Player;
using DevelopProducts.BehaviorGraph.Runtime.Composition.InGame.Enemy;
using DevelopProducts.BehaviorGraph.Runtime.Domain.InGame.Character;
using DevelopProducts.BehaviorGraph.Runtime.Domain.InGame.Player;
using DevelopProducts.BehaviorGraph.Runtime.InfraStructure.InGame.Character;
using DevelopProducts.BehaviorGraph.Runtime.Utility;
using DevelopProducts.BehaviorGraph.Runtime.View;
using DevelopProducts.BehaviorGraph.Runtime.View.InGame.Player;
using SymphonyFrameWork.System.ServiceLocate;
using UnityEngine;
using KillChord.Runtime.InfraStructure.InGame.Battle;
using KillChord.Runtime.InfraStructure.InGame.Player;
using KillChord.Runtime.InfraStructure.Player;

#if UNITY_EDITOR
using DevelopProducts.BehaviorGraph.Runtime.Composition.InGame.Debugger;
#endif

namespace DevelopProducts.BehaviorGraph.Runtime.Composition
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

        [SerializeField] private CharacterData _enemyData;

        private EnemyTestSpawnerBG _enemyTestSpawner;

        private void Awake()
        {
            ServiceLocator.RegisterInstance(this);
        }

        public void Initialize(TargetManager targetManager, TargetEntityRegistry targetEntityRegistry)
        {
            if (_player == null)
                Debug.LogError($"{nameof(PlayerView)}がNullです", this);
            _enemyTestSpawner = ServiceLocator.GetInstance<EnemyTestSpawnerBG>();
            if (_enemyTestSpawner == null)
            {
                Debug.LogError($"{nameof(EnemyTestSpawner)}が見つかりません。シーン内に配置されていることを確認してください。", this);
                return;
            }


            CharacterEntity player = CharacterFactory.Create(_playerData);
            Debug.Log(player.ToString());
            _enemyTestSpawner.SetTargetEntity(player);
            _enemyTestSpawner.SetTargetManager(targetManager, targetEntityRegistry);


            //PlayerMoveParameter parameter = _playerConfig.ToDomain();
            PlayerMoveParameter parameter = new(10,20,0.2f,0.3f);

            PlayerDodgeMovementApplication dodge = new(parameter);
            PlayerMovement move = new(parameter);
            PlayerApplication application = new(move, dodge);

            PlayerController playerMovementController = new(application);
            var ct = ServiceLocator.GetInstance<ICameraTransform>().transform;


            TargetSelectorController targetSelectorController = ServiceLocator.GetInstance<TargetSelectorController>();
            if(targetSelectorController == null)
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

            SkillController skillController = new SkillController(musicSyncService);
            AttackResultViewModel attackResultViewModel = new AttackResultViewModel();
            AttackResultPresenter attackResultPresenter = new AttackResultPresenter(attackResultViewModel);

            PlayerBattleState playerBattleState = new PlayerBattleState(player);

            PlayerAttackController playerAttackController = new PlayerAttackController(attackResultPresenter, playerBattleState, skillController, targetSelectorController, musicSyncService);

            _player.Init(playerMovementController, playerAttackController, ct);

            Debug.Log("【PlayerInitializer】Initialize End");
#if UNITY_EDITOR
            _player.gameObject
                .AddComponent<PlayerMoveParameterDebug>()
                .SetPlayerMoveParameter(parameter);
#endif
        }
    }
}