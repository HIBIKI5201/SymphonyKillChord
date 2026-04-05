using UnityEngine;

namespace DevelopProducts.Design.GameMode.Domain
{
    /// <summary>
    ///     Domain層で使用される、ステージの実行時コンテキストを表すクラス。
    /// </summary>
    public class StageRuntimeContext
    {
        public StageRuntimeContext(PlayerRuntimeState playerRuntimeState, EnemyKillStatics enemyKillStatics, StageTimeState stageTimeState)
        {
            _playerRuntimeState = playerRuntimeState;
            _enemyKillStatics = enemyKillStatics;
            _stageTimeState = stageTimeState;
        }

        public PlayerRuntimeState PlayerRuntimeState => _playerRuntimeState;
        public EnemyKillStatics EnemyKillStatics => _enemyKillStatics;
        public StageTimeState StageTimeState => _stageTimeState;

        private readonly PlayerRuntimeState _playerRuntimeState;
        private readonly EnemyKillStatics _enemyKillStatics;
        private readonly StageTimeState _stageTimeState;
    }
}
