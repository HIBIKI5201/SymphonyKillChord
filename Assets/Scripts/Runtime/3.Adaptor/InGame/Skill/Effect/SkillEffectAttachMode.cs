namespace KillChord.Runtime.Adaptor.InGame.Skill.Effect
{
    /// <summary>
    ///     スキルエフェクトの配置方式を表す列挙型。
    /// </summary>
    public enum SkillEffectAttachMode
    {
        /// <summary> プレイヤーに追従します。 </summary>
        PlayerFollow = 0,

        /// <summary> 再生時点のプレイヤー位置へ設置します。 </summary>
        PlayerPoint = 1,

        /// <summary> 対象に追従します。 </summary>
        TargetFollow = 2,

        /// <summary> 再生時点の対象位置へ設置します。 </summary>
        TargetPoint = 3,

        /// <summary> Contextで指定されたワールド座標へ設置します。 </summary>
        WorldPoint = 4,

        /// <summary> プレイヤーと対象を結ぶ線上へ配置します。ビームや軌跡に使用します。 </summary>
        BetweenPlayerAndTarget = 5,
    }
}
