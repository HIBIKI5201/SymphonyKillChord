using KillChord.Runtime.Adaptor.InGame.Player;
using KillChord.Runtime.Application.InGame.Player;
using KillChord.Runtime.Composition.InGame.Enemy;
using KillChord.Runtime.Composition.InGame.Player;
using KillChord.Runtime.Domain.InGame.Character;
using KillChord.Runtime.Domain.InGame.Player;
using KillChord.Runtime.InfraStructure.InGame.Battle;
using KillChord.Runtime.InfraStructure.InGame.Character;
using KillChord.Runtime.InfraStructure.InGame.Player;
using KillChord.Runtime.Utility;
using KillChord.Runtime.View.InGame.Player;
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

        [Space] [Header("キャラクターデータ（テスト用）")] [SerializeField]
        private CharacterData _playerData;

        [SerializeField] private CharacterData _enemyData;

        [SerializeField] private EnemyTestSpawner _enemyTestSpawner;

        public void Initialize()
        {
            if (_player == null)
                Debug.LogError($"{nameof(PlayerView)}がNullです", this);

            CharacterEntity player = CharacterFactory.Create(_playerData);
            _enemyTestSpawner.SetTargetEntity(player);

            PlayerMoveParameter parameter = _playerConfig.ToDomain();

            //BattleApplication battleApplication = new(player, attackExecutor);
            //BattleController battleController = new(battleApplication, new(), null);

            PlayerDodgeMovementApplication dodge = new(parameter);
            PlayerMovement move = new(parameter);
            PlayerApplication application = new(move, dodge);

            PlayerController playerMovementController = new(application);
            _player.Init(playerMovementController, null);


#if UNITY_EDITOR
            _player.gameObject
                .AddComponent<PlayerMoveParameterDebug>()
                .SetPlayerMoveParameter(parameter);
#endif
        }
    }
}