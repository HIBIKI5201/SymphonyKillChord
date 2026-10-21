namespace KillChord.Runtime.View.InGame.Enemy.AIFacade
{
    /// <summary>
    ///     敵AI用ファサード：移動系。
    ///     共通の移動指示に加えて、攻撃待機中の横移動を扱う。
    /// </summary>
    public class EnemyMovementAIFacade : MovementAIFacadeBase<EnemyMoveView>
    {
        /// <summary>
        ///     攻撃待機中、プレイヤーを向いたまま指定方向へ横移動する。
        /// </summary>
        /// <param name="direction"> 左右の向きを表す符号。 </param>
        public void StrafeWhileWaiting(int direction)
        {
            MoveView?.StrafeWhileWaiting(direction);
        }

        /// <summary>
        ///     横移動だけを終了し、他の移動指示には干渉しない。
        /// </summary>
        public void StopStrafing()
        {
            MoveView?.StopStrafing();
        }
    }
}
