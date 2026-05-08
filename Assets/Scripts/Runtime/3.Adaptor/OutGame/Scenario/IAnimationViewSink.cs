namespace KillChord.Runtime.Adaptor
{
    /// <summary>
    /// IAnimationViewSink の契約を定義します。
    /// </summary>
    public interface IAnimationViewSink
    {
        void SetAnimation(string assetKey);
    }
}
