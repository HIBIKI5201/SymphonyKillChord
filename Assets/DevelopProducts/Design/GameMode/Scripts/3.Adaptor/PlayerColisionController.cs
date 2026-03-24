using DevelopProducts.Design.GameMode.Application;
using DevelopProducts.Design.GameMode.Domain;
using DevelopProducts.Design.GameMode.InfraStructure;
using UnityEngine;

namespace DevelopProducts.Design.GameMode.Adaptor
{
    /// <summary>
    ///     プレイヤーが敵に当たった時の処理を行うクラス。
    ///     プレイヤーと敵の両方にダメージを与え、ゲームの状態を更新する役割を持つ。
    /// </summary>
    public class PlayerColisionController
    {
        public PlayerColisionController(
            DamagePlayerUsecase damagePlayerUsecase,
            DamageEnemyUsecase damageEnemyUsecase,
            GameModeRuntime gameModeRuntime,
            StageHudPresenter stageHudPresenter)
        {
            _damagePlayerUsecase = damagePlayerUsecase;
            _damageEnemyUsecase = damageEnemyUsecase;
            _gameModeRuntime = gameModeRuntime;
            _stageHudPresenter = stageHudPresenter;
        }

        public void HitEnemy(EnemyRuntimeState enemy, EnemyDefinition definition)
        {
            if (_gameModeRuntime.IsFinished)
            {
                Debug.Log("Game is already finished. No further actions allowed.");
                return;
            }

            _damagePlayerUsecase.Execute(1);
            _damageEnemyUsecase.Execute(enemy, definition, 1);

            _gameModeRuntime.Tick();
            _stageHudPresenter.Present();
        }

        private readonly DamagePlayerUsecase _damagePlayerUsecase;
        private readonly DamageEnemyUsecase _damageEnemyUsecase;
        private readonly GameModeRuntime _gameModeRuntime;
        private readonly StageHudPresenter _stageHudPresenter;
    }
}
