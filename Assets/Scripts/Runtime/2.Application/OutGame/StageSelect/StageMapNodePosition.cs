using System;

namespace KillChord.Runtime.Application.OutGame.StageSelect
{
    /// <summary>
    ///     作戦画面上のステージノードのグリッド位置を表す値オブジェクト。
    /// </summary>
    public readonly struct StageMapNodePosition : IEquatable<StageMapNodePosition>
    {
        /// <summary>
        ///     グリッド位置を初期化する。
        /// </summary>
        /// <param name="column"> 左端を0とする列番号。</param>
        /// <param name="row"> 列内の上端を0とする行番号。</param>
        public StageMapNodePosition(int column, int row)
        {
            if (column < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(column));
            }

            if (row < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(row));
            }

            _column = column;
            _row = row;
        }

        /// <summary> 左端を0とする列番号。 </summary>
        public int Column => _column;

        /// <summary> 列内の上端を0とする行番号。 </summary>
        public int Row => _row;

        /// <summary>
        ///     他のグリッド位置と比較する。
        /// </summary>
        /// <param name="other"> 比較対象。</param>
        /// <returns> 等しい場合はtrue。</returns>
        public bool Equals(StageMapNodePosition other)
        {
            return _column == other._column && _row == other._row;
        }

        /// <summary>
        ///     オブジェクトと比較する。
        /// </summary>
        /// <param name="obj"> 比較対象。</param>
        /// <returns> 等しい場合はtrue。</returns>
        public override bool Equals(object obj)
        {
            return obj is StageMapNodePosition other && Equals(other);
        }

        /// <summary>
        ///     ハッシュコードを取得する。
        /// </summary>
        /// <returns> ハッシュコード。</returns>
        public override int GetHashCode()
        {
            return HashCode.Combine(_column, _row);
        }

        private readonly int _column;
        private readonly int _row;
    }
}
