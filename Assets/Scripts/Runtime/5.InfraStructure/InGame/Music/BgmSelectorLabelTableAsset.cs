using KillChord.Runtime.Domain.InGame.Music;
using KillChord.Runtime.Utility.Identity;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace KillChord.Runtime.InfraStructure.InGame.Music
{
    /// <summary>
    ///     スキルIDとCRIセレクターラベル名の対応、および原曲ラベルを保持するScriptableObject。
    ///     コードを変更せずにアセット上で割り当てを編集できる。
    /// </summary>
    [CreateAssetMenu(
        fileName = nameof(BgmSelectorLabelTableAsset),
        menuName = "KillChord/InGame/Music/BgmSelectorLabelTable")]
    public class BgmSelectorLabelTableAsset : ScriptableObject
    {
        /// <summary> 原曲のセレクターラベル名。 </summary>
        public string OriginalLabel => _originalLabel;

        /// <summary>
        ///     アセット設定からDomain層の対応表オブジェクトを生成する。
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
            /// <summary> SkillTemplateのIDと一致させるスキルID。 </summary>
            public int SkillId => _skillId.Id;

            /// <summary> CRIセレクターラベル名。 </summary>
            public string SelectorLabel => _selectorLabel;

            [SerializeField, SourceDataCollection("Skill"), Tooltip("SkillTemplateのIDと一致させるスキルIDです。")]
            private DataID _skillId;
            [SerializeField, Tooltip("CRIセレクターラベル名です。例: SelectorLabel_S01")]
            private string _selectorLabel;
        }

        [SerializeField, Tooltip("原曲のCRIセレクターラベル名です。ACBの定義と完全一致させる必要があります。")]
        private string _originalLabel;
        [SerializeField, Tooltip("スキルIDとセレクターラベル名の対応エントリ一覧です。")]
        private Entry[] _entries;
    }
}
