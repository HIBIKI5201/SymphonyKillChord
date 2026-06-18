namespace DevelopProducts.Pause
{
    /// <summary>
    ///     時間のスケーリングが可能なオブジェクトのインターフェース。
    /// </summary>
    public interface ITimeScalable
    {
        public TimeScaleType TimeScaleType { get; }
        public void Inject(TimeScaleData data);
    }
}