using UnityEngine;

namespace KillChord.Runtime.Adaptor.Persistent.Input
{
    /// <summary>
    ///     入力履歴用の時刻取得クラス。
    /// </summary>
    public class InputTimestampProvider
    {
        public float GetCurrentTimestamp()
        {
            return Time.unscaledTime;
        }
    }
}
