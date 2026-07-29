namespace KillChord.Runtime.Domain.OutGame.Scenario
{
    /// <summary>
    /// フェード演出の対象を表す。テキストは対象に含めない（常にフェードしない）。
    /// </summary>
    public enum FadeTarget
    {
        /// <summary> 画面全体（テキストを除く）。 </summary>
        Screen,
        /// <summary> 背景のみ。 </summary>
        Background,
        /// <summary> 左の立ち絵。 </summary>
        PortraitLeft,
        /// <summary> 中央の立ち絵。 </summary>
        PortraitCenter,
        /// <summary> 右の立ち絵。 </summary>
        PortraitRight,
        /// <summary> テキストボックス。 </summary>
        Text,
        /// <summary> 演出用の黒オーバーレイ（暗転/明転）。 </summary>
        Black,
    }
}
