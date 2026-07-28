using SymphonyFrameWork.Attribute;
using System;
using UnityEngine;

namespace KillChord.Runtime.Utility.Identity
{
    /// <summary>
    ///     エディタで設定した文字列IDを実行時用の数値IDとして保持します。
    /// </summary>
    [Serializable]
    public struct DataID : IEquatable<DataID>
    {
        /// <summary> 実行時に使用する数値IDです。 </summary>
        public int Id => _hashId;

        /// <summary>
        ///     別のDataIDと等価か判定します。
        /// </summary>
        /// <param name="other"> 比較対象のDataIDです。 </param>
        /// <returns> 数値IDが一致する場合はtrueです。 </returns>
        public bool Equals(DataID other)
        {
            return _hashId == other._hashId;
        }

        /// <summary>
        ///     オブジェクトと等価か判定します。
        /// </summary>
        /// <param name="obj"> 比較対象のオブジェクトです。 </param>
        /// <returns> 数値IDが一致するDataIDの場合はtrueです。 </returns>
        public override bool Equals(object obj)
        {
            return obj is DataID other && Equals(other);
        }

        /// <summary>
        ///     数値IDをハッシュコードとして取得します。
        /// </summary>
        /// <returns> 数値IDです。 </returns>
        public override int GetHashCode()
        {
            return _hashId;
        }

        /// <summary>
        ///     数値IDの文字列表現を取得します。
        /// </summary>
        /// <returns> 数値IDの文字列表現です。 </returns>
        public override string ToString()
        {
            return _hashId.ToString();
        }

        /// <summary>
        ///     2つのDataIDが等しいか判定します。
        /// </summary>
        /// <param name="left"> 左辺のDataIDです。 </param>
        /// <param name="right"> 右辺のDataIDです。 </param>
        /// <returns> 数値IDが一致する場合はtrueです。 </returns>
        public static bool operator ==(DataID left, DataID right)
        {
            return left.Equals(right);
        }

        /// <summary>
        ///     2つのDataIDが異なるか判定します。
        /// </summary>
        /// <param name="left"> 左辺のDataIDです。 </param>
        /// <param name="right"> 右辺のDataIDです。 </param>
        /// <returns> 数値IDが異なる場合はtrueです。 </returns>
        public static bool operator !=(DataID left, DataID right)
        {
            return !left.Equals(right);
        }

#if UNITY_EDITOR
        [SerializeField, Tooltip("プランナーが設定する人間可読なIDです。")]
        private string _id;
#endif

        [SerializeField, ReadOnly, Tooltip("実行時に使用する焼き込み済み数値IDです。")]
        private int _hashId;
    }
}
