using System;
using UnityEngine;

namespace DevelopProducts.EquipmentBGM
{
    /// <summary>
    ///     スキルIDとCRIセレクターラベル名の対応表を保持するScriptableObject。
    /// </summary>
    [CreateAssetMenu(fileName = "BgmSelectorLabelTable", menuName = "DevelopProducts/BgmSelectorLabelTable")]
    public class BgmSelectorLabelTable : ScriptableObject
    {
        /// <summary>
        ///     スキルIDに対応するセレクターラベル名を取得する。
        /// </summary>
        /// <param name="skillId"> 検索するスキルID。 </param>
        /// <param name="label"> 対応するラベル名。見つからない場合は空文字。 </param>
        /// <returns> 対応エントリが存在する場合はtrue。 </returns>
        public bool TryGetLabel(int skillId, out string label)
        {
            foreach (var entry in _entries)
            {
                if (entry.SkillId == skillId)
                {
                    label = entry.SelectorLabel;
                    return true;
                }
            }
            label = string.Empty;
            return false;
        }

        /// <summary>
        ///     スキルIDとセレクターラベル名の対応エントリ。
        /// </summary>
        [Serializable]
        public struct Entry
        {
            /// <summary> SkillData.Id と一致させるスキルID。 </summary>
            public int SkillId => _skillId;

            /// <summary> CRIセレクターラベル名。 </summary>
            public string SelectorLabel => _selectorLabel;

            [SerializeField, Tooltip("SkillData.Id と一致させるスキルID。")]
            private int _skillId;
            [SerializeField, Tooltip("CRIセレクターラベル名。例: SelectorLabel_S01")]
            private string _selectorLabel;
        }

        /// <summary> 原曲のCRIセレクターラベル名。 </summary>
        public string OriginalLabel => _originalLabel;

        [SerializeField, Tooltip("原曲のCRIセレクターラベル名。例: SelectorLabel_S00")]
        private string _originalLabel;
        [SerializeField, Tooltip("スキルIDとセレクターラベル名の対応エントリ一覧。")]
        private Entry[] _entries;
    }
}
