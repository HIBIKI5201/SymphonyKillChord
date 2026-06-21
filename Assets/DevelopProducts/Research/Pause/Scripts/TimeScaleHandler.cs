using System.Collections.Generic;

namespace DevelopProducts.Pause
{
    /// <summary>
    ///     オブジェクト一つ分のスケール管理を行うクラス
    /// </summary>
    public class TimeScaleHandler
    {
        /// <summary>
        ///     コンストラクタ
        /// </summary>
        /// <param name="baseScale"></param>
        public TimeScaleHandler(float baseScale)
        {
            _baseScale = baseScale;
        }
        public float CurrentScale => _pauseCount > 0 ? 0f : _baseScale;
        /// <summary>
        ///     ポーズ
        /// </summary>
        /// <returns></returns>
        public PauseToken Pause()
        {
            _pauseCount++;
            return new PauseToken(() => _pauseCount--);
        }
        /// <summary>
        ///     タイムスケールを上書きする
        /// </summary>
        /// <param name="scale"></param>
        public void SetScale(float scale)
        {
            _baseScale = scale;
        }
        private float _baseScale;
        private int _pauseCount;
    }
}
