namespace KillChord.Runtime.Utility.Persistent
{
    /// <summary>
    ///     イベント定義：プレイヤーが攻撃を実行した時。
    ///     命中の有無に関わらず、攻撃が成立した時点で発火する。
    /// </summary>
    public readonly struct EOnPlayerAttackExecuted : IEvent
    {
    }
}
