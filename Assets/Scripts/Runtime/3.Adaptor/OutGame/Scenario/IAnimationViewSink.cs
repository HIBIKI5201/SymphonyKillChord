namespace KillChord.Runtime.Adaptor.OutGame.Scenario
{
    /// <summary>
    /// Animation の表示反映契約を定義する。
    /// </summary>
    public interface IAnimationViewSink
    {
        void SetAnimation(string assetKey);
    }
}