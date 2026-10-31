namespace KillChord.Runtime.Domain.InGame.Battle
{
    /// <summary>
    ///     攻撃のインターバルを管理するエンティティクラス。
    ///     攻撃硬直中かどうかを保持する。硬直の長さは攻撃ごとに拍数から決めるため、ここでは持たない。
    /// </summary>
    public class AttackIntervalEntity
    {
        /// <summary>
        ///     攻撃状態を更新するメソッド。攻撃が開始された場合はtrue、終了された場合はfalseを引数に渡す。
        /// </summary>
        /// <param name="isAttacking"></param>
        public void UpdateAttackState(bool isAttacking)
        {
            IsAttacking = isAttacking;
        }

        /// <summary> 現在攻撃中かどうかを表すプロパティ。 </summary>
        public bool IsAttacking { get; private set; }
    }
}
