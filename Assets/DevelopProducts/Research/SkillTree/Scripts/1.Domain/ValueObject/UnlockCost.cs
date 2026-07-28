using System;

namespace DevelopProducts.SkillTree
{
    /// <summary>
    /// スキルのアンロックに必要なコストを表します。
    /// </summary>
    public readonly struct UnlockCost : IEquatable<UnlockCost>
    {
        private readonly int _cost;

        /// <summary>
        /// アンロックコストを生成します。
        /// </summary>
        /// <param name="cost">コスト値</param>
        /// <exception cref="ArgumentException">
        /// cost が 0 未満の場合に発生します。
        /// </exception>
        public UnlockCost(int cost)
        {
            if (cost < 0)
            {
                throw new ArgumentException("アンロックコストは正の数である必要があります");
            }

            _cost = cost;
        }

        /// <summary>
        /// コスト値を取得します。
        /// </summary>
        public int Cost => _cost;

        /// <summary>
        /// 指定したアンロックコストと現在の値が等しいか判定します。
        /// </summary>
        /// <param name="other">比較対象のアンロックコスト</param>
        /// <returns>値が等しい場合は true</returns>
        public bool Equals(UnlockCost other)
        {
            return _cost == other._cost;
        }

        /// <summary>
        /// 指定したオブジェクトと現在の値が等しいか判定します。
        /// </summary>
        /// <param name="obj">比較対象のオブジェクト</param>
        /// <returns>値が等しい場合は true</returns>
        public override bool Equals(object obj)
        {
            return obj is UnlockCost other && Equals(other);
        }

        /// <summary>
        /// ハッシュコードを取得します。
        /// </summary>
        /// <returns>ハッシュコード</returns>
        public override int GetHashCode()
        {
            return _cost.GetHashCode();
        }

        /// <summary>
        /// 2つのアンロックコストが等しいか判定します。
        /// </summary>
        public static bool operator ==(UnlockCost left, UnlockCost right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// 2つのアンロックコストが等しくないか判定します。
        /// </summary>
        public static bool operator !=(UnlockCost left, UnlockCost right)
        {
            return !(left == right);
        }

        /// <summary>
        /// コスト値の文字列表現を返します。
        /// </summary>
        /// <returns>コスト値の文字列</returns>
        public override string ToString()
        {
            return _cost.ToString();
        }
    }
}