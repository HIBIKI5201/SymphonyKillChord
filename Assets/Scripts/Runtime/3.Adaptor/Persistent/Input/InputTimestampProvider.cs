using UnityEngine;

namespace KillChord.Runtime.Adaptor
{
    /// <summary>
    ///     入力履歴用の時刻取得クラス。
    /// </summary>
    public class InputTimestampProvider
    {
        public float GetCurrentTimestamp()
        {
            return Time.unscaledTime;
            //後で、CRIの時間取得に変更する予定。
        }
    }
}
