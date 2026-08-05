namespace KillChord.Runtime.Domain.InGame.Battle
{
    /// <summary>
    ///     1回の攻撃で狙う対象1件と、その対象に固有の命中条件を表す構造体。
    /// </summary>
    public readonly struct AttackTarget
    {
        /// <summary>
        ///     攻撃対象を生成します。
        /// </summary>
        /// <param name="defender"> 攻撃対象。 </param>
        /// <param name="isOutOfRange"> 射程外の対象である場合はtrue。 </param>
        public AttackTarget(IDefender defender, bool isOutOfRange)
        {
            Defender = defender;
            IsOutOfRange = isOutOfRange;
        }

        /// <summary> 攻撃対象。 </summary>
        public IDefender Defender { get; }

        /// <summary> 射程外の対象である場合はtrue。ダメージ減衰の判定に使う。 </summary>
        public bool IsOutOfRange { get; }
    }
}
