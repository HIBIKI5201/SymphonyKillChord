using System;
using System.Collections.Generic;
using UnityEngine;

namespace KillChord.Runtime.InfraStructure.OutGame.Scenario
{
    /// <summary>
    /// カタログアセットを辞書参照可能にする基底リポジトリ。
    /// </summary>
    public abstract class CatalogRepositoryBase<TId, TDefinition, TEntry>
        where TId : struct
    {
        /// <summary>
        /// カタログアセットから検索用辞書を構築する。
        /// </summary>
        protected CatalogRepositoryBase(IReadOnlyList<TEntry> entries)
        {
            _map = new Dictionary<TId, TDefinition>();
            if (entries == null) return;

            for (int i = 0; i < entries.Count; i++)
            {
                TEntry entry = entries[i];
                if (!TryBuild(entry, out TId id, out TDefinition definition))
                {
                    Debug.LogWarning($"{GetType().Name}: skipped invalid catalog entry at index {i}.");
                    continue;
                }
                if (_map.ContainsKey(id))
                {
                    Debug.LogWarning($"{GetType().Name}: duplicated id '{id}' at index {i}. Existing value will be overwritten.");
                }
                _map[id] = definition;
            }
        }

        /// <summary>
        /// ID から定義情報を検索する。
        /// </summary>
        public bool TryFindById(TId id, out TDefinition definition)
        {
            return _map.TryGetValue(id, out definition);
        }

        /// <summary>
        /// カタログエントリから検索用の定義情報を生成する。
        /// </summary>
        protected abstract bool TryBuild(TEntry entry, out TId id, out TDefinition definition);

        private readonly Dictionary<TId, TDefinition> _map;
    }
}
