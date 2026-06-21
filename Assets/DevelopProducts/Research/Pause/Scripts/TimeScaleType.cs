using System;

namespace DevelopProducts.Pause
{
    /// <summary>
    ///     時間のスケーリングの種類を表すフラグ列挙型。
    /// </summary>
    [Flags]
    public enum TimeScaleType : byte
    {
        None = 0,
        Player = 1 << 0,
        Enemy = 1 << 1,
        Map = 1 << 2,
    }
}
