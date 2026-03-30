using KillChord.Runtime.Domain.Persistent.Input;
using KillChord.Runtime.Utility;
using System;

namespace KillChord.Runtime.Application.Persistent.Input
{
    /// <summary>
    ///     入力をバッファリングするクラス。
    ///     一定数の入力を保持し、新しい入力が追加されると古い入力が削除される。
    /// </summary>
    public class InputBufferingQueue
    {
        public InputBufferingQueue(int capacity)
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

        public BufferedInput GetAt(int offset = 0) => _buffer.PeekFirst(offset);

        public BufferedInput GetLast(int offset = 0) => _buffer.PeekLast(offset);

        public void Clear() => _buffer.Clear();

        private readonly RingBuffer<BufferedInput> _buffer;
    }
}
