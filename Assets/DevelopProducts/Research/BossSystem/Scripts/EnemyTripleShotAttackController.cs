using KillChord.Runtime.Adaptor.InGame.Enemy;
using KillChord.Runtime.Application.InGame.Enemy;

namespace DevelopProducts.Boss
{
    /// <summary>
    ///     特殊攻撃1用の攻撃コントローラー。
    ///     正面・右斜め30度・左斜め30度の三方向に直線攻撃を行う。
    /// </summary>
    /// <remarks>
    ///     現状は EnemyAttackUsecase.ExecuteAttack を三方向ぶん呼び出す枠組みのみ。
    ///     射線判定（EnemyRaycastDetectView）と警告表示は単一方向前提のため、
    ///     方向ごとの射線判定・警告ラインに対応する拡張が別途必要。TODO参照。
    /// </remarks>
    public sealed class EnemyTripleShotAttackController : IEnemyAttackController
    {
        /// <summary> 中央からの左右の振り角（度）。 </summary>
        public const float SpreadAngleDegrees = 30f;

        public EnemyTripleShotAttackController(
            EnemyAttackUsecase enemyAttackUsecase,
            EnemyBattleState enemyBattleState)
        {
            _enemyAttackUsecase = enemyAttackUsecase;
            _enemyBattleState = enemyBattleState;
        }

        /// <summary>
        ///     三方向に攻撃を実行する。
        /// </summary>
        public void ExecuteAttack()
        {
            // TODO: 方向ごとの射線判定が必要。
            // 現状の EnemyAttackUsecase は内部の単一方向 raycast でヒット判定するため、
            // 角度オフセット（0, +30, -30）を渡せるオーバーロード or
            // 三方向対応の RaycastDetectService 拡張を追加してから差し替える。
            _enemyAttackUsecase.ExecuteAttack(
                _enemyBattleState.CurrentAttack,
                _enemyBattleState.Attacker,
                _enemyBattleState.Target);
        }

        private readonly EnemyAttackUsecase _enemyAttackUsecase;
        private readonly EnemyBattleState _enemyBattleState;
    }
}
