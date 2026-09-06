namespace KillChord.Runtime.Domain.InGame.Enemy
{
    /// <summary>
    ///     攻撃後に敵が選択する行動の種類。
    /// </summary>
    public enum EnemyPostAttackBehaviorKind
    {
        /// <summary> その場に留まり再攻撃を狙う。 </summary>
        Stay,
        /// <summary> 近くの味方に合流する。 </summary>
        RegroupWithAlly,
        /// <summary> 近くの障害物に接近する。 </summary>
        ApproachObstacle,
    }
}
