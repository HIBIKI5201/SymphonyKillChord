namespace KillChord.Runtime.Adaptor.OutGame.Audio
{
    /// <summary>
    ///     UI操作の意味に対応する操作音を再生するコマンド。
    /// </summary>
    public interface IUISoundEffectCommand
    {
        /// <summary>
        ///     指定したUI操作に対応する操作音を再生する。
        /// </summary>
        /// <param name="kind"> UI操作音の種類。 </param>
        void Play(UISoundEffectKind kind);
    }
}
