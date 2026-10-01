using KillChord.Runtime.Utility.Collections;
using System;

namespace KillChord.Runtime.Domain.Persistent.Input
{
    /// <summary>
    ///     入力をバッファリングするクラス。
    ///     一定数の入力を保持し、新しい入力が追加されると古い入力が削除される。
    /// </summary>
    public class InputBufferingQueue
    {
        /// <summary>
        ///     指定容量の入力バッファを生成する。
        /// </summary>
        public InputBufferingQueue(int capacity)
        {
            _buffer = new RingBuffer<BufferedInput>(capacity);
        }

        /// <summary> 新しい入力がバッファに記録されたときに発火するイベント。 </summary>
        public event Action<BufferedInput> OnBuffered;

        /// <summary> バッファに記録されている入力数。 </summary>
        public int Count => _buffer.Count;

        /// <summary>
        ///     入力をバッファに記録し、記録イベントを発火する。
        /// </summary>
        public void Push(in BufferedInput input)
        {
            _buffer.Enqueue(input);
            OnBuffered?.Invoke(input);
        }

        /// <summary>
        ///     古い側から数えて指定位置の入力を取得する。
        /// </summary>
        public BufferedInput GetAt(int offset = 0) => _buffer.PeekFirst(offset);

        /// <summary>
        ///     新しい側から数えて指定位置の入力を取得する。
        /// </summary>
        public BufferedInput GetLast(int offset = 0) => _buffer.PeekLast(offset);

        /// <summary>
        ///     バッファを空にする。
        /// </summary>
        public void Clear() => _buffer.Clear();

        private readonly RingBuffer<BufferedInput> _buffer;
    }
}
