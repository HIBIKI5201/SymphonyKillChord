namespace KillChord.Runtime.Domain.InGame.Enemy
{
    /// <summary>
    ///     砲弾の着弾演出に関する音楽同期の定数。
    /// </summary>
    public static class ShellMusicConstants
    {
        /// <summary>
        ///     着弾（爆発）タイミングの何拍前から、着弾予告デカールの進捗（0→1）を
        ///     変化させ始めるかを表す拍数。
        ///     着弾予告SEの再生タイミングもこの値に合わせて算出することで、
        ///     デカールの見た目の変化開始と完全に同じ拍でSEが鳴るようにする。
        /// </summary>
        public const double DETONATE_LEAD_BEAT_COUNT = 2d;
    }
}
