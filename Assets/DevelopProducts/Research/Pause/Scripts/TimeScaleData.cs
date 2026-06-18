using System;
using UnityEngine;

namespace DevelopProducts.Pause
{
    /// <summary>
    ///     時間のスケーリングに関するデータを保持するクラス。
    /// </summary>
    public class TimeScaleData
    {
        public float Scale { get; private set; } = 1.0f;

        /// <summary>
        ///     スケールを適用した値を返す。
        /// </summary>
        /// <param name="delta"></param>
        /// <returns></returns>
        public float ApplayScale(float delta)
        {
            return Scale * delta;
        }

        /// <summary>
        ///     スケールを変更する。
        /// </summary>
        /// <param name="scale"></param>
        public void ChangeScale(float scale)
        {
            Scale = scale;
        }
    }
}