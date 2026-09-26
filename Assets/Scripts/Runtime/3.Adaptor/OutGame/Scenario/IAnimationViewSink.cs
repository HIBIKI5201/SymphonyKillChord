namespace KillChord.Runtime.Adaptor.OutGame.Scenario
{
    /// <summary>
    /// Animation の表示反映契約を定義する。
    /// </summary>
    public interface IAnimationViewSink
    {
        /// <summary>
        ///     指定したアニメーションを表示に反映する。
        /// </summary>
        void SetAnimation(string assetKey);
    }
}