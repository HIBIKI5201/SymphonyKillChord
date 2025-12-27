using System.Collections.Generic;

namespace Mock.MusicBattle.Utility
{
    /// <summary>
    ///     優先順位付きキュー。
    ///     常に最小の要素が先頭に来るように管理される。
    /// </summary>
    /// <typeparam name="T">キューに格納する要素の型。</typeparam>
    public class PriorityQueue<T>
    {
        /// <summary>
        ///     指定された比較子を使用して、<see cref="PriorityQueue{T}"/>クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="comparer">要素の比較に使用するIComparer{T}。</param>
        public PriorityQueue(IComparer<T> comparer)
        {
            _comparer = comparer;
        }
        #region パブリックプロパティ
        /// <summary> キューに含まれる要素の数を取得します。 </summary>
        public int Count => _heap.Count;
        #endregion
        #region Publicメソッド
        /// <summary>
        ///     キューに要素を追加します。
        /// </summary>
        /// <param name="item">追加する要素。</param>
        public void Enqueue(T item)
        {
            _heap.Add(item);
            HeapifyUp(_heap.Count - 1);
        }

        /// <summary>
        ///     キューから先頭の最小要素を取り出して返します。
        ///     この操作により、要素はキューから削除されます。
        /// </summary>
        /// <returns>キューの先頭にある最小の要素。</returns>
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
        ///     キューの先頭にある最小の要素を、削除せずに返します。
        /// </summary>
        /// <returns>キューの先頭にある最小の要素。</returns>
        public T Peek()
        {
            if (_heap.Count == 0) return default;
            return _heap[0];
        }
        #endregion
        #region プライベートフィールド
        /// <summary> ヒープ構造を保持するリスト。 </summary>
        private readonly List<T> _heap = new List<T>();
        /// <summary> 要素の比較に使用する比較子。 </summary>
        private readonly IComparer<T> _comparer;
        #endregion
        #region Privateメソッド
        /// <summary>
        ///     指定されたインデックスからヒープを上方向に再構築します。
        /// </summary>
        /// <param name="index">再構築を開始するインデックス。</param>
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

        /// <summary>
        ///     指定されたインデックスからヒープを下方向に再構築します。
        /// </summary>
        /// <param name="index">再構築を開始するインデックス。</param>
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

        /// <summary>
        ///     ヒープ内の2つの要素を交換します。
        /// </summary>
        /// <param name="a">交換する最初の要素のインデックス。</param>
        /// <param name="b">交換する2番目の要素のインデックス。</param>
        private void Swap(int a, int b)
        {
            T tmp = _heap[a];
            _heap[a] = _heap[b];
            _heap[b] = tmp;
        }
        #endregion
    }
}

