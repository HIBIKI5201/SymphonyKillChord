namespace KillChord.Runtime.View.InGame.Enemy
{
    /// <summary>
    ///     敵・ボス共通で、AIファサードから移動を指示するための View。
    /// </summary>
    public interface IEnemyMoveView
    {
        /// <summary>
        ///     攻撃可能な位置まで移動する。
        /// </summary>
        public void MoveToAttack();

        /// <summary>
        ///     移動を停止する。
        /// </summary>
        public void StopMoving();
    }
}
