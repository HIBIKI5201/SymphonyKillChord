using UnityEngine;

namespace KillChord.Runtime.Adaptor.InGame.PostEffect
{
    /// <summary>
    ///     リズムガイドの全画面演出1回分の表示データを保持するDTO。
    /// </summary>
    public readonly ref struct RhythmGuidePostEffectDto
    {
        /// <summary>
        ///     新しい全画面演出DTOを生成する。
        /// </summary>
        /// <param name="isJustTiming"> ジャストタイミングが成立した場合はtrue。 </param>
        /// <param name="color"> 演出へ反映するビート色。 </param>
        public RhythmGuidePostEffectDto(bool isJustTiming, in Color color)
        {
            IsJustTiming = isJustTiming;
            Color = color;
        }

        /// <summary> ジャストタイミングが成立した場合はtrue。 </summary>
        public bool IsJustTiming { get; }

        /// <summary> 演出へ反映するビート色。 </summary>
        public Color Color { get; }
    }
}
