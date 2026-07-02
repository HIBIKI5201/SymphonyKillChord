using System;
using System.Collections.Generic;

namespace KillChord.Runtime.Domain.InGame.Music
{
    /// <summary>
    ///     装備スキルの組み合わせから、再生するBGMのキュー名を解決するドメインオブジェクト。
    /// </summary>
    public sealed class SkillBgmCatalog
    {
        /// <summary>
        ///     対応表とデフォルトキュー名からカタログを生成する。
        /// </summary>
        /// <param name="entries"> スキル組み合わせとキュー名の対応の一覧。 </param>
        /// <param name="defaultCueName"> 一致する対応が無い場合に使用するデフォルトのキュー名。 </param>
        public SkillBgmCatalog(IReadOnlyList<SkillBgmEntry> entries, string defaultCueName)
        {
            _entries = entries ?? Array.Empty<SkillBgmEntry>();
            _defaultCueName = defaultCueName ?? string.Empty;
        }

        /// <summary> 一致する対応が無い場合に使用するデフォルトのキュー名。 </summary>
        public string DefaultCueName => _defaultCueName;

        /// <summary>
        ///     装備スキルIDの組み合わせに対応するBGMのキュー名を解決する。
        ///     順序を区別せず集合として一致を判定し、一致する対応が無ければデフォルトのキュー名を返す。
        /// </summary>
        /// <param name="equippedSkillIds"> 装備スキルIDの集合。 </param>
        /// <returns> 解決されたBGMのキュー名。 </returns>
        public string Resolve(IReadOnlyList<int> equippedSkillIds)
        {
            int[] normalizedSkillIds = SkillBgmEntry.NormalizeSkillIds(equippedSkillIds);

            for (int i = 0; i < _entries.Count; i++)
            {
                if (_entries[i].Matches(normalizedSkillIds))
                {
                    return _entries[i].CueName;
                }
            }

            return _defaultCueName;
        }

        private readonly IReadOnlyList<SkillBgmEntry> _entries;
        private readonly string _defaultCueName;
    }
}
