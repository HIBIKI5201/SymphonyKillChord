namespace KillChord.Runtime.Adaptor.Persistent.Music
{
    /// <summary>
    ///     BGM Cueの切り替えをViewへ要求する出力ポート。
    /// </summary>
    public interface IBgmCuePlayer
    {
        /// <summary>
        ///     再生するBGM Cueを設定する。
        /// </summary>
        /// <param name="cueName"> 再生するCue名。空の場合は停止する。 </param>
        void SetCue(string cueName);
    }
}
