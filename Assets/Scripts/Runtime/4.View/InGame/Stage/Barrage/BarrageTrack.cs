using UnityEngine.Timeline;

namespace KillChord.Runtime.View.InGame.Stage.Barrage
{
    /// <summary>
    ///     弾幕発射クリップを並べるTimelineトラックです。
    /// </summary>
    /// <remarks> 宛先はタレットIDで解決するため、トラックへのバインドは不要です。 </remarks>
    [TrackClipType(typeof(BarrageFireClip))]
    [TrackColor(0.85f, 0.35f, 0.2f)]
    public sealed class BarrageTrack : TrackAsset
    {
    }
}
