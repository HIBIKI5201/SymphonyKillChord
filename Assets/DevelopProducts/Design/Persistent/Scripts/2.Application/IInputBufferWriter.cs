using DevelopProducts.Persistent.Domain.Input;
using UnityEngine;

namespace DevelopProducts.Persistent.Application
{
    /// <summary>
    ///     入力履歴を記録するためのインターフェース。
    /// </summary>
    public interface IInputBufferWriter
    {
        /// <summary>
        ///     入力を記録する
        /// </summary>
        /// <param name="input"></param>
        void Push(in BufferedInput input);

        /// <summary>
        ///     入力履歴をクリアする
        /// </summary>
        void Clear();
    }
}
