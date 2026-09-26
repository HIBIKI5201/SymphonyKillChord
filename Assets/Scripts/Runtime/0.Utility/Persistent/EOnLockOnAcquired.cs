namespace KillChord.Runtime.Utility.Persistent
{
    /// <summary>
    ///     イベント定義：プレイヤーのロックオン対象が新しい敵に切り替わった時。
    ///     未ロックオン状態からの新規捕捉、および既にロックオン中に別の敵へ切り替わった時の両方で発火する。
    /// </summary>
    public readonly struct EOnLockOnAcquired : IEvent
    {
    }
}
