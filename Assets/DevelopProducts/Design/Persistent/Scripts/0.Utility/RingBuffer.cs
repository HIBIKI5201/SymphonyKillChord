using UnityEngine;
using System;

namespace DevelopProducts.Persistent.Utility
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

            if(_count == _buffer.Length)
            {
                _head = (_head + 1) % _buffer.Length;
            }
            else
            {
                _count++;
            }
        }

        /// <summary>
        ///     古い順に要素を取得する。
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        /// <exception cref="IndexOutOfRangeException"></exception>
        public T Peek(int index)
        {
            if (index < 0 || index >= _count)
            {
                throw new IndexOutOfRangeException();
            }

            int bufferIndex = (_head + index) % _buffer.Length;
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
            if (offset < 0 || offset >= _count)
            {
                throw new IndexOutOfRangeException();
            }

            int bufferIndex = (_head + _count - 1 - offset + _buffer.Length) % _buffer.Length;
            return _buffer[bufferIndex];
        }

        /// <summary>
        ///     全要素をクリアする。
        /// </summary>
        public void Clear()
        {
            _head = 0;
            _count = 0;
        }

        private readonly T[] _buffer;
        private int _head;
        private int _count;
    }
}
