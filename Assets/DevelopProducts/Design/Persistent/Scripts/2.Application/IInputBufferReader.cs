using DevelopProducts.Persistent.Domain.Input;
using UnityEngine;

namespace DevelopProducts.Persistent.Application
{
    /// <summary>
    ///     入力履歴を読み取るためのインターフェース。
    /// </summary>
    public interface IInputBufferReader
    {
        /// <summary> 現在の履歴件数 </summary>
        int Count { get; }

        /// <summary>
        ///     古い順に入力履歴を取得する。
        ///     indexは0から始まり、0は最も古い入力を指す。
        /// </summary>
        /// <param name="index">取得する履歴のインデックス</param>
        /// <returns>指定したインデックスの入力履歴</returns>
        BufferedInput GetAt(int index);

        /// <summary>
        ///     新しい順に入力履歴を取得する。
        ///     オフセットは0から始まり、0は最新の入力を指す。
        /// </summary>
        /// <param name="offset">取得する履歴のオフセット</param>
        /// <returns>指定したオフセットの入力履歴</returns>
        BufferedInput GetLast(int offset = 0);
    }
}
