namespace KillChord.Runtime.Adaptor.OutGame.SkillTree
{
    /// <summary>
    ///     スキルプレビュー画面のViewModel。
    /// </summary>
    /// <param name="nodeId"></param>
    public interface IPreviewVideoScreenViewModel
    {
        /// <summary>
        ///     プレイビュー動画を再生する。
        /// </summary>
        /// <param name="nodeId"></param>
        public void PlayPreviewVideo(int nodeId);
        /// <summary>
        ///     プレイビュー動画の再生を停止する。
        /// </summary>
        public void StopPreviewVideo();
    }
}
