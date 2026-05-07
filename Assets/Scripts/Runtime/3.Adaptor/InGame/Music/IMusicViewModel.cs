namespace KillChord.Runtime.Adaptor.InGame.Music
{
    /// <summary>
    ///     音楽再生の状態を管理するViewModelのインターフェース。
    /// </summary>
    public interface IMusicViewModel
    {
        /// <summary>
        ///     再生する音楽のキューを更新する。
        /// </summary>
        /// <param name="cueName"> 新しいキュー名。 </param>
        public void UpdateMusicCue(string cueName);
    }
}