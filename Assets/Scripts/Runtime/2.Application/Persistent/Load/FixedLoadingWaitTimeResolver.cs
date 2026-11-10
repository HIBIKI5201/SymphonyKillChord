using System;

namespace KillChord.Runtime.Application.Persistent.Load
{
    /// <summary>
    ///     遷移の組み合わせによらず、同じ最低表示時間を返す。
    ///     待機時間の設定アセットが無い場合の既定値に使う。
    /// </summary>
    public sealed class FixedLoadingWaitTimeResolver : ILoadingWaitTimeResolver
    {
        /// <summary>
        ///     最低表示時間を指定して生成する。
        /// </summary>
        /// <param name="minimumDisplayTime"> ロード画面の最低表示時間（秒）。 </param>
        public FixedLoadingWaitTimeResolver(float minimumDisplayTime)
        {
            if (!float.IsFinite(minimumDisplayTime) || minimumDisplayTime < 0f)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(minimumDisplayTime),
                    minimumDisplayTime,
                    "最低表示時間は非負の有限の数でなければなりません。");
            }

            _minimumDisplayTime = minimumDisplayTime;
        }

        /// <summary>
        ///     遷移の組み合わせによらず、生成時の最低表示時間を返す。
        /// </summary>
        /// <param name="fromSceneName"> 遷移元のシーン名。 </param>
        /// <param name="toSceneName"> 遷移先のシーン名。 </param>
        /// <returns> 最低表示時間（秒）。 </returns>
        public float ResolveMinimumDisplayTime(string fromSceneName, string toSceneName)
        {
            return _minimumDisplayTime;
        }

        private readonly float _minimumDisplayTime;
    }
}
