namespace KillChord.Runtime.Domain.InGame.Battle
{
    /// <summary>
    ///     攻撃のインターバルを管理するエンティティクラス。
    ///     攻撃の開始と終了を管理するためのクラスで、攻撃が現在行われているかどうかを保持するプロパティを持つ。
    /// </summary>
    public class AttackIntervalEntity
    {
        /// <summary>
        ///     攻撃のインターバルを管理するエンティティのコンストラクタ。
        /// </summary>
        /// <param name="interval"></param>
        public AttackIntervalEntity(AttackInterval interval)
        {
            Interval = interval;
        }

        /// <summary>
        ///     攻撃状態を更新するメソッド。攻撃が開始された場合はtrue、終了された場合はfalseを引数に渡す。
        /// </summary>
        /// <param name="isAttacking"></param>
        public void UpdateAttackState(bool isAttacking)
        {
            IsAttacking = isAttacking;
        }

        /// <summary> 攻撃の硬直時間を表すプロパティ。 </summary>
        public AttackInterval Interval { get; }

        /// <summary> 現在攻撃中かどうかを表すプロパティ。 </summary>
        public bool IsAttacking { get; private set; }
    }
}