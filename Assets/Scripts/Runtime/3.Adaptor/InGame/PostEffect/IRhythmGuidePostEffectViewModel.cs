namespace KillChord.Runtime.Adaptor.InGame.PostEffect
{
    /// <summary>
    ///     リズムガイドの全画面演出の再生指示を受け取るViewModelインターフェース。
    /// </summary>
    public interface IRhythmGuidePostEffectViewModel
    {
        /// <summary>
        ///     全画面演出を一度だけ再生する。
        /// </summary>
        /// <param name="dto"> 反映する表示データ。 </param>
        void Play(in RhythmGuidePostEffectDto dto);
    }
}
