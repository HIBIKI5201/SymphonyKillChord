using KillChord.Runtime.Domain.InGame.Music;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace KillChord.Runtime.InfraStructure.InGame.Music
{
    /// <summary>
    ///     装備スキルの組み合わせと再生するBGMのキュー名の対応表を保持するScriptableObject。
    /// </summary>
    [CreateAssetMenu(fileName = nameof(SkillBgmCatalogAsset), menuName = "KillChord/SkillBgmCatalog")]
    public class SkillBgmCatalogAsset : ScriptableObject
    {
        /// <summary>
        ///     ScriptableObjectのデータからドメイン層のカタログオブジェクトを生成する。
        /// </summary>
        /// <returns> スキルBGMカタログ。 </returns>
        public SkillBgmCatalog ToDomain()
        {
            List<SkillBgmEntry> entries = _entries == null
                ? new List<SkillBgmEntry>()
                : new List<SkillBgmEntry>(_entries.Length);

            if (_entries != null)
            {
                for (int i = 0; i < _entries.Length; i++)
                {
                    SkillBgmEntryData data = _entries[i];
                    if (data == null)
                    {
                        continue;
                    }

                    entries.Add(new SkillBgmEntry(data.SkillIds, data.CueName));
                }
            }

            return new SkillBgmCatalog(entries, _defaultCueName);
        }

        [Tooltip("装備スキルの組み合わせとBGMのキュー名の対応表。")]
        [SerializeField] private SkillBgmEntryData[] _entries;
        [Tooltip("対応表に一致する組み合わせが無い場合に再生するデフォルトのBGMキュー名。")]
        [SerializeField] private string _defaultCueName;

        /// <summary>
        ///     インスペクター設定用の、スキル組み合わせとキュー名の対応データ構造。
        /// </summary>
        [Serializable]
        private class SkillBgmEntryData
        {
            /// <summary> 対応する装備スキルIDの組み合わせ。 </summary>
            public IReadOnlyList<int> SkillIds => _skillIds;

            /// <summary> 再生するBGMのキュー名。 </summary>
            public string CueName => _cueName;

            [Tooltip("対応する装備スキルIDの組み合わせ。")]
            [SerializeField] private int[] _skillIds;
            [Tooltip("再生するBGMのキュー名。")]
            [SerializeField] private string _cueName;
        }
    }
}
