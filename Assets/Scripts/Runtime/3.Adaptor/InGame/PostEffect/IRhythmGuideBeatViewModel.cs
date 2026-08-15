using UnityEngine;

namespace KillChord.Runtime.Adaptor.InGame.PostEffect
{
    /// <summary>
    ///     リズムガイド上の現在のビート状態を公開するViewModelインターフェース。
    /// </summary>
    public interface IRhythmGuideBeatViewModel
    {
        /// <summary> 現在のビート位置がジャストタイミングのブロック上にある場合はtrue。 </summary>
        bool IsOnJustTiming { get; }

        /// <summary>
        ///     現在カーソルが乗っているビートブロックの色を取得する。
        /// </summary>
        /// <param name="color"> ビートブロックの色。 </param>
        /// <returns> 取得できた場合はtrue。 </returns>
        bool TryGetCurrentBeatColor(out Color color);
    }
}
