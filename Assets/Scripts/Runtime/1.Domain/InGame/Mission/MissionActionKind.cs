namespace KillChord.Runtime.Domain.InGame.Mission
{
    /// <summary>
    ///     ミッションの目標として計測できるプレイヤー行動の種別。
    /// </summary>
    public enum MissionActionKind
    {
        /// <summary> 回避。 </summary>
        Evade,
        /// <summary> 攻撃。 </summary>
        Attack,
        /// <summary> スキル。 </summary>
        Skill,
    }
}
