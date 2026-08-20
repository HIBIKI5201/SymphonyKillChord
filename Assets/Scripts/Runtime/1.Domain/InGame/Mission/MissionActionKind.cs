namespace KillChord.Runtime.Domain.InGame.Mission
{
    /// <summary>
    ///     ミッションの目標として計測できるプレイヤー行動の種別。
    /// </summary>
    public enum MissionActionKind
    {
        /// <summary> 回避。 </summary>
        Dodge,
        /// <summary> 攻撃。 </summary>
        Attack,
        /// <summary> スキル。 </summary>
        Skill,
        /// <summary> 移動。 </summary>
        Move,
        /// <summary> カメラのロックオン。 </summary>
        LockOn,
        /// <summary> 1拍子攻撃。 </summary>
        AttackOneBeat,
        /// <summary> 2拍子攻撃。 </summary>
        AttackTwoBeat,
        /// <summary> 4拍子攻撃。 </summary>
        AttackFourBeat,
        /// <summary> 8拍子攻撃。 </summary>
        AttackEightBeat,
        /// <summary> 3拍子攻撃。 </summary>
        AttackThreeBeat,
        /// <summary> 6拍子攻撃。 </summary>
        AttackSixBeat,
        /// <summary> 敵の攻撃実行。 </summary>
        EnemyAttack,
    }
}
