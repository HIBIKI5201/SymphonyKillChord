using System;
using UnityEngine;

namespace KillChord.Runtime.Utility
{
    /// <summary>
    ///     固定長のリングバッファ。
    ///     古いデータを上書きする。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class RingBuffer<T>
    {
        public RingBuffer(int capacity)
        {
            _buffer = new T[capacity];
            _head = 0;
            _count = 0;
        }

        public int Capacity => _buffer.Length;
        public int Count => _count;

        /// <summary>
        ///     要素を追加する。
        ///     バッファが満杯の場合は最も古い要素を上書きする。
        /// </summary>
        /// <param name="item"></param>
        public void Enqueue(T item)
        {
            int index = (_head + _count) % _buffer.Length;
            _buffer[index] = item;

            if (_count == _buffer.Length)
            {
                _head = (_head + 1) % _buffer.Length;
            }
            else
            {
                _count++;
            }
        }

        /// <summary>
        ///     古い順にoffsetで指定した要素を取得する。
        /// </summary>
        /// <param name="offset"></param>
        /// <returns></returns>
        /// <exception cref="IndexOutOfRangeException"></exception>
        public T PeekFirst(int offset = 0)
        {
            if (offset < 0 || _count <= offset)
            {
                throw new IndexOutOfRangeException();
            }

            int bufferIndex = GetIndexByRing(offset);
            return _buffer[bufferIndex];
        }

        /// <summary>
        ///     新しい順にoffsetで指定した要素を取得する。
        /// </summary>
        /// <param name="offset"></param>
        /// <returns></returns>
        /// <exception cref="IndexOutOfRangeException"></exception>
        public T PeekLast(int offset = 0)
        {
            if (offset < 0 || _count <= offset)
            {
                throw new IndexOutOfRangeException();
            }

            int tailOffset = _count - 1 - offset;
            int bufferIndex = GetIndexByRing(tailOffset);
            return _buffer[bufferIndex];
        }

        /// <summary>
        ///     全要素をクリアする。
        /// </summary>
        public void Clear()
        {
            _head = 0;
            _count = 0;
            _buffer.AsSpan().Clear();
        }

        private readonly T[] _buffer;
        private int _head;
        private int _count;

        private int GetIndexByRing(int offset) => (_head + offset + _buffer.Length) % _buffer.Length;
    }
}
