namespace DevelopProducts.Pause
{
    /// <summary>
    ///     時間のスケーリングが可能なオブジェクトのインターフェース。
    /// </summary>
    public interface ITimeScalable
    {
        public int InstanceId { get; }
        public TimeScaleType TimeScaleType { get; }
        public void Inject(TimeScaleData data);
        public void Tick();
    }
}