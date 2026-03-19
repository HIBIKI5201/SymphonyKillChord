using UnityEngine;

namespace DevelopProducts.Persistent.Domain.Input
{
    /// <summary>
    ///     入力のタイムスタンプを提供するクラス。
    ///     タイムスタンプは、入力が発生した時間を表す。
    /// </summary>
    public class InputTimestampProvider
    {
        public float GetTimestamp()
        {
            return Time.unscaledTime;
        }
    }
}
