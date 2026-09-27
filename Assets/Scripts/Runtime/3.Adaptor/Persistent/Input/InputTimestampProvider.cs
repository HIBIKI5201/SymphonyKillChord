using UnityEngine;

namespace KillChord.Runtime.Adaptor.Persistent.Input
{
    /// <summary>
    ///     入力履歴用の時刻取得クラス。
    /// </summary>
    public class InputTimestampProvider
    {
        /// <summary>
        ///     タイムスケールの影響を受けない現在時刻を返す。
        /// </summary>
        public float GetCurrentTimestamp()
        {
            return Time.unscaledTime;
        }
    }
}
