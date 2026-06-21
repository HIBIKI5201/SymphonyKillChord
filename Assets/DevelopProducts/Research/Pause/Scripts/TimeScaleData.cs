namespace DevelopProducts.Pause
{
    /// <summary>
    ///     時間のスケーリングに関するデータを保持するクラス。
    /// </summary>
    public class TimeScaleData
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="handler"></param>
        public TimeScaleData(TimeScaleHandler handler)
        {
            _handler = handler;
        }
        public float Scale => _handler.CurrentScale;
        /// <summary>
        ///     オブジェクト側がスケールにDeltaTimeを乗算した値を返す
        /// </summary>
        /// <param name="delta"></param>
        /// <returns></returns>
        public float ApplyScale(float delta) => _handler.CurrentScale * delta;

        private readonly TimeScaleHandler _handler;
    }
}