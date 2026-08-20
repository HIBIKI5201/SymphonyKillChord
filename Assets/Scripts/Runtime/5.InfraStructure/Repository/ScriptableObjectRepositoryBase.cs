using System.Collections.Generic;
using UnityEngine;

namespace KillChord.Runtime.InfraStructure.Repository
{
    /// <summary>
    ///     アセット配列をIDで検索可能にする ScriptableObject 用の共通リポジトリ基底クラス。
    /// </summary>
    /// <typeparam name="TId"> 検索キーとなるID型です。 </typeparam>
    /// <typeparam name="TValue"> 検索結果として返す値の型です。 </typeparam>
    /// <typeparam name="TEntry"> シリアライズされたエントリの型です。 </typeparam>
    public abstract class ScriptableObjectRepositoryBase<TId, TValue, TEntry> : ScriptableObject
        where TEntry : Object
    {
        /// <summary>
        ///     指定されたIDに対応する値を取得しようとします。
        /// </summary>
        /// <param name="id"> 検索するIDです。 </param>
        /// <param name="value"> 取得した値です。 </param>
        /// <returns> IDに対応する値が存在する場合はtrueです。 </returns>
        protected bool TryFind(TId id, out TValue value)
        {
            EnsureMap();
            return _map.TryGetValue(id, out value);
        }

        /// <summary>
        ///     検索用辞書を破棄します。
        /// </summary>
        protected void InvalidateCache()
        {
            _map = null;
        }

        /// <summary>
        ///     シリアライズされたエントリの一覧を返します。
        /// </summary>
        protected abstract IReadOnlyList<TEntry> GetEntries();

        /// <summary>
        ///     エントリからID・値を構築します。
        /// </summary>
        /// <param name="entry"> 変換元のエントリです。 </param>
        /// <param name="id"> 構築したIDです。 </param>
        /// <param name="value"> 構築した値です。 </param>
        /// <returns> エントリが有効な場合はtrueです。無効な場合はマップへの登録をスキップします。 </returns>
        protected abstract bool TryBuild(TEntry entry, out TId id, out TValue value);

        /// <summary>
        ///     Inspector上の設定が変わった際に検索用辞書を破棄します。
        /// </summary>
        private void OnValidate()
        {
            InvalidateCache();
        }

        /// <summary>
        ///     ID検索用辞書を構築します。
        /// </summary>
        private void EnsureMap()
        {
            if (_map != null) { return; }

            _map = new Dictionary<TId, TValue>();

            IReadOnlyList<TEntry> entries = GetEntries();
            if (entries == null) { return; }

            for (int i = 0; i < entries.Count; i++)
            {
                TEntry entry = entries[i];
                if (entry == null) { continue; }

                if (!TryBuild(entry, out TId id, out TValue value)) { continue; }

                if (_map.ContainsKey(id))
                {
                    Debug.LogWarning(
                        $"[{GetType().Name}] IDが重複しています。Id: {id}, Asset: {entry.name}", this);
                    continue;
                }

                _map.Add(id, value);
            }
        }

        private Dictionary<TId, TValue> _map;
    }
}
