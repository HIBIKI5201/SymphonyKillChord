using System;

namespace DevelopProducts.SkillTree
{
    /// <summary>
    ///     スキルツリーで使用するアンロックポイントを表します。
    /// </summary>
    public readonly struct UnlockPoint : IEquatable<UnlockPoint>
    {
        /// <summary>
        ///     アンロックポイントを生成します。
        /// </summary>
        /// <param name="point">ポイント値</param>
        /// <exception cref="ArgumentException">
        /// point が 0 未満の場合に発生します。
        /// </exception>
        public UnlockPoint(int point)
        {
            if (point < 0)
            {
                throw new ArgumentException("アンロックポイントは正の数である必要があります");
            }

            _point = point;
        }

        /// <summary>
        ///     ポイント値を取得します。
        /// </summary>
        public int Point => _point;

        /// <summary>
        ///     指定したアンロックポイントと現在の値が等しいか判定します。
        /// </summary>
        /// <param name="other">比較対象のアンロックポイント</param>
        /// <returns>値が等しい場合は true</returns>
        public bool Equals(UnlockPoint other)
        {
            return _point == other._point;
        }

        /// <summary>
        ///     指定したオブジェクトと現在の値が等しいか判定します。
        /// </summary>
        /// <param name="obj">比較対象のオブジェクト</param>
        /// <returns>値が等しい場合は true</returns>
        public override bool Equals(object obj)
        {
            return obj is UnlockPoint other && Equals(other);
        }

        /// <summary>
        ///     ハッシュコードを取得します。
        /// </summary>
        /// <returns>ハッシュコード</returns>
        public override int GetHashCode()
        {
            return _point.GetHashCode();
        }

        /// <summary>
        ///     2つのアンロックポイントが等しいか判定します。
        /// </summary>
        public static bool operator ==(UnlockPoint left, UnlockPoint right)
        {
            return left.Equals(right);
        }

        /// <summary>
        ///     2つのアンロックポイントが等しくないか判定します。
        /// </summary>
        public static bool operator !=(UnlockPoint left, UnlockPoint right)
        {
            return !(left == right);
        }

        /// <summary>
        ///     ポイント値の文字列表現を返します。
        /// </summary>
        /// <returns>ポイント値の文字列</returns>
        public override string ToString()
        {
            return _point.ToString();
        }

        private readonly int _point;
    }
}