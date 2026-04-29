using System;
using System.Collections.Generic;
using UnityEngine;

namespace KillChord.Runtime.InfraStructure
{
    public abstract class CatalogRepositoryBase<TDefinition, TEntry>
    {
        protected CatalogRepositoryBase(IReadOnlyList<TEntry> entries)
        {
            _map = new Dictionary<string, TDefinition>(StringComparer.Ordinal);
            if (entries == null) return;

            for (int i = 0; i < entries.Count; i++)
            {
                TEntry entry = entries[i];
                if (!TryBuild(entry, out string id, out TDefinition definition))
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

        public bool TryFindById(string id, out TDefinition definition)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                definition = default;
                return false;
            }

            return _map.TryGetValue(id, out definition);
        }

        protected abstract bool TryBuild(TEntry entry, out string id, out TDefinition definition);

        private readonly Dictionary<string, TDefinition> _map;
    }
}
