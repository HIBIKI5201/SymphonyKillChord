using DevelopProducts.Persistent.Domain.Input;
using DevelopProducts.Persistent.Utility;
using System;

namespace DevelopProducts.Persistent.Application
{
    /// <summary>
    ///     入力履歴保存の実体。
    ///     リングバッファを使用して、一定数の入力履歴を保存する。
    /// </summary>
    public class BufferdInputBuffer
    {
        public BufferdInputBuffer(int capacity)
        {
            _buffer = new RingBuffer<BufferedInput>(capacity);
        }

        /// <summary> 新しい入力がバッファに記録されたときに発火するイベント。 </summary>
        public event Action<BufferedInput> OnBuffered;

        public int Count => _buffer.Count;

        public void Push(in BufferedInput input)
        {
            _buffer.Enqueue(input);
            OnBuffered?.Invoke(input);
        }

        public BufferedInput GetAt(int index)
        {
            return _buffer.Peek(index);
        }

        public BufferedInput GetLast(int offset = 0)
        {
            return _buffer.PeekLast(offset);
        }

        public void Clear()
        {
            _buffer.Clear();
        }

        private readonly RingBuffer<BufferedInput> _buffer;
    }
}
