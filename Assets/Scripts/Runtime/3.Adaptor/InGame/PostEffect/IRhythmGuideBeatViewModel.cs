using UnityEngine;

namespace KillChord.Runtime.Adaptor.InGame.PostEffect
{
    /// <summary>
    ///     リズムガイドの拍種に対応する表示色を公開するViewModelインターフェース。
    /// </summary>
    public interface IRhythmGuideBeatViewModel
    {
        /// <summary>
        ///     指定した拍種のビート色を取得する。
        /// </summary>
        /// <param name="beatCount"> 入力時に確定した拍種の整数値。 </param>
        /// <param name="color"> ビートブロックの色。 </param>
        /// <returns> 取得できた場合はtrue。 </returns>
        bool TryGetBeatColor(int beatCount, out Color color);
    }
}
