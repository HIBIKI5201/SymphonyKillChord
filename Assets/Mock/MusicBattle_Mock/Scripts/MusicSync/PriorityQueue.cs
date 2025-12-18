using System.Collections.Generic;

namespace Mock.MusicBattle.Utility
{
    /// <summary>
    /// 優先順位付きキュー。<br></br>
    /// 常に最小の要素が先頭に来るように管理される。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class PriorityQueue<T>
    {
        public PriorityQueue(IComparer<T> comparer)
        {
            _comparer = comparer;
        }

        /// <summary>
        /// キューに要素を追加する。
        /// </summary>
        /// <param name="item">追加する要素</param>
        public void Enqueue(T item)
        {
            _heap.Add(item);
            HeapifyUp(_heap.Count - 1);
        }

        /// <summary>
        /// キューから先頭の最小要素を取り出す。<br></br>
        /// 取り出された要素がキューから削除される。
        /// </summary>
        /// <returns>先頭要素</returns>
        public T Dequeue()
        {
            if (_heap.Count == 0) return default;

            T root = _heap[0];
            int last = _heap.Count - 1;

            _heap[0] = _heap[last];
            _heap.RemoveAt(last);

            if (_heap.Count > 0)
                HeapifyDown(0);

            return root;
        }

        /// <summary>
        /// キューの先頭要素を削除せず、参照する。
        /// </summary>
        /// <returns>先頭要素</returns>
        public T Peek()
        {
            if (_heap.Count == 0) return default;
            return _heap[0];
        }

        private List<T> _heap = new List<T>();
        private IComparer<T> _comparer;
        public int Count => _heap.Count;

        private void HeapifyUp(int index)
        {
            while (index > 0)
            {
                int parent = (index - 1) / 2;
                if (_comparer.Compare(_heap[index], _heap[parent]) >= 0)
                    break;

                Swap(index, parent);
                index = parent;
            }
        }

        private void HeapifyDown(int index)
        {
            int last = _heap.Count - 1;

            while (true)
            {
                int left = index * 2 + 1;
                int right = left + 1;
                int smallest = index;

                if (left <= last && _comparer.Compare(_heap[left], _heap[smallest]) < 0)
                    smallest = left;

                if (right <= last && _comparer.Compare(_heap[right], _heap[smallest]) < 0)
                    smallest = right;

                if (smallest == index)
                    break;

                Swap(index, smallest);
                index = smallest;
            }
        }

        private void Swap(int a, int b)
        {
            T tmp = _heap[a];
            _heap[a] = _heap[b];
            _heap[b] = tmp;
        }
    }
}
