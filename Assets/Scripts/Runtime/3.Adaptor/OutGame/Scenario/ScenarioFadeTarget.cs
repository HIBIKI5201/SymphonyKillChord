namespace KillChord.Runtime.Adaptor.OutGame.Scenario
{
    /// <summary>
    /// View が解釈するシナリオフェード対象。
    /// </summary>
    public enum ScenarioFadeTarget
    {
        /// <summary> 画面全体。 </summary>
        Screen,
        /// <summary> 背景。 </summary>
        Background,
        /// <summary> 左の立ち絵。 </summary>
        PortraitLeft,
        /// <summary> 中央の立ち絵。 </summary>
        PortraitCenter,
        /// <summary> 右の立ち絵。 </summary>
        PortraitRight,
        /// <summary> 会話枠、話者名、本文。 </summary>
        Text,
        /// <summary> 全画面黒オーバーレイ。 </summary>
        Black,
    }
}
