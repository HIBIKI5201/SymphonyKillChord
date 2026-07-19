using KillChord.Runtime.Domain.InGame.Music;
using KillChord.Runtime.Utility.Identity;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace KillChord.Runtime.InfraStructure.InGame.Music
{
    /// <summary>
    ///     スキルIDとCRIセレクターラベル名の対応、および原曲ラベルを保持するScriptableObject。
    /// </summary>
    [CreateAssetMenu(fileName = nameof(BgmSelectorLabelTableAsset), menuName = "KillChord/BgmSelectorLabelTable")]
    public class BgmSelectorLabelTableAsset : ScriptableObject
    {
        /// <summary>
        ///     アセット設定からドメイン層の対応表オブジェクトを生成する。
        /// </summary>
        /// <returns> スキルBGMセレクター対応表。 </returns>
        public SkillBgmSelectorTable ToDomain()
        {
            Dictionary<int, string> skillLabels = new(_entries?.Length ?? 0);
            if (_entries != null)
            {
                for (int i = 0; i < _entries.Length; i++)
                {
                    Entry entry = _entries[i];
                    if (string.IsNullOrEmpty(entry.SelectorLabel))
                    {
                        continue;
                    }

                    skillLabels[entry.SkillId] = entry.SelectorLabel;
                }
            }

            return new SkillBgmSelectorTable(_originalLabel, skillLabels);
        }

        /// <summary>
        ///     スキルIDとセレクターラベル名の対応エントリ。
        /// </summary>
        [Serializable]
        public struct Entry
        {
            /// <summary> SkillTemplate.Id と一致させるスキルID。 </summary>
            public int SkillId => _skillId.Id;

            /// <summary> CRIセレクターラベル名。 </summary>
            public string SelectorLabel => _selectorLabel;

            [SerializeField, SourceDataCollection("Skill"), Tooltip("SkillTemplate.Id と一致させるスキルID。")]
            private DataID _skillId;
            [SerializeField, Tooltip("CRIセレクターラベル名。例: SelectorLabel_S01")]
            private string _selectorLabel;
        }

        [SerializeField, Tooltip("原曲のCRIセレクターラベル名。例: SelectorLabel_S00")]
        private string _originalLabel;
        [SerializeField, Tooltip("スキルIDとセレクターラベル名の対応エントリ一覧。")]
        private Entry[] _entries;
    }
}
