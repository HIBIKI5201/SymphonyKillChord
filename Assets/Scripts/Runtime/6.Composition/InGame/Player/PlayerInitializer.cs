using KillChord.Runtime.Adaptor.InGame.Player;
using KillChord.Runtime.Application.InGame.Player;
using KillChord.Runtime.Composition.InGame.Enemy;
using KillChord.Runtime.Domain.InGame.Character;
using KillChord.Runtime.Domain.InGame.Player;
using KillChord.Runtime.InfraStructure.InGame.Battle;
using KillChord.Runtime.InfraStructure.InGame.Character;
using KillChord.Runtime.InfraStructure.InGame.Player;
using KillChord.Runtime.Utility;
using KillChord.Runtime.View.InGame.Player;
using SymphonyFrameWork.System.ServiceLocate;
using UnityEngine;
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

        [Space] [Header("キャラクターデータ（テスト用）")] [SerializeField]
        private CharacterData _playerData;

        [SerializeField] private CharacterData _enemyData;

        private EnemyTestSpawner _enemyTestSpawner;

        private void Awake()
        {
            ServiceLocator.RegisterInstance(this);
        }

        public void Initialize()
        {
            if (_player == null)
                Debug.LogError($"{nameof(PlayerView)}がNullです", this);
            _enemyTestSpawner = ServiceLocator.GetInstance<EnemyTestSpawner>();

            CharacterEntity player = CharacterFactory.Create(_playerData);
            _enemyTestSpawner.SetTargetEntity(player);

            PlayerMoveParameter parameter = _playerConfig.ToDomain();

            //BattleApplication battleApplication = new(player, attackExecutor);
            //BattleController battleController = new(battleApplication, new(), null);

            PlayerDodgeMovementApplication dodge = new(parameter);
            PlayerMovement move = new(parameter);
            PlayerApplication application = new(move, dodge);

            PlayerController playerMovementController = new(application);
            var ct = ServiceLocator.GetInstance<ICameraTransform>().transform;
            
            _player.Init(playerMovementController, null, ct);


#if UNITY_EDITOR
            _player.gameObject
                .AddComponent<PlayerMoveParameterDebug>()
                .SetPlayerMoveParameter(parameter);
#endif
        }
    }
}