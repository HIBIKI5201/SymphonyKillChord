using System;
using System.Collections.Generic;

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
                if (!TryBuild(entry, out string id, out TDefinition definition)) continue;
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
