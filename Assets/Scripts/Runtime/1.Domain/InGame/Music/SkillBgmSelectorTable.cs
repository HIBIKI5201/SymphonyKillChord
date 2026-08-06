using System.Collections.Generic;

namespace KillChord.Runtime.Domain.InGame.Music
{
    /// <summary>
    ///     スキルIDとBGMセレクターラベルの対応、および原曲ラベルを保持するドメインオブジェクト。
    /// </summary>
    public sealed class SkillBgmSelectorTable
    {
        /// <summary>
        ///     原曲ラベルとスキルIDごとのラベル対応から対応表を生成する。
        /// </summary>
        /// <param name="originalLabel"> 原曲のセレクターラベル。 </param>
        /// <param name="skillLabels"> スキルIDとセレクターラベルの対応。 </param>
        public SkillBgmSelectorTable(string originalLabel, IReadOnlyDictionary<int, string> skillLabels)
        {
            _originalLabel = originalLabel ?? string.Empty;
            _skillLabels = skillLabels ?? EmptySkillLabels;
        }

        /// <summary> 原曲のセレクターラベルを取得する。 </summary>
        public string OriginalLabel => _originalLabel;

        /// <summary> 原曲ラベルが設定されているかどうかを取得する。 </summary>
        public bool HasOriginalLabel => !string.IsNullOrEmpty(_originalLabel);

        /// <summary>
        ///     スキルIDに対応するセレクターラベルの取得を試みる。
        /// </summary>
        /// <param name="skillId"> スキルID。 </param>
        /// <param name="label"> 対応するセレクターラベル。 </param>
        /// <returns> 対応が存在する場合はtrue。 </returns>
        public bool TryGetLabel(int skillId, out string label)
        {
            if (_skillLabels.TryGetValue(skillId, out string found) && !string.IsNullOrEmpty(found))
            {
                label = found;
                return true;
            }

            label = string.Empty;
            return false;
        }

        /// <summary>
        ///     ソートされたスキルID列から小節ループ用のシーケンスを構築する。
        ///     対応ラベルが登録されていないスキルはシーケンスから除外されるため、
        ///     設定漏れを呼び出し側が検知できるよう対象IDを併せて返す。
        /// </summary>
        /// <param name="equippedSkillIds"> ソートされたスキルのID列（スロット順）。 </param>
        /// <param name="unmappedSkillIds"> 対応ラベルが見つからなかったスキルID一覧。 </param>
        /// <returns> 構築したシーケンス。対応ラベルが1つも無い場合は空のシーケンス。 </returns>
        public BgmSelectorSequence CreateSequence(
            IReadOnlyList<int> equippedSkillIds,
            out IReadOnlyList<int> unmappedSkillIds)
        {
            unmappedSkillIds = EmptySkillIds;

            if (equippedSkillIds == null || equippedSkillIds.Count == 0)
            {
                return new BgmSelectorSequence(null);
            }

            List<string> skillLabels = new(equippedSkillIds.Count);
            List<int> unmapped = null;

            for (int i = 0; i < equippedSkillIds.Count; i++)
            {
                int skillId = equippedSkillIds[i];
                if (TryGetLabel(skillId, out string label))
                {
                    skillLabels.Add(label);
                    continue;
                }

                unmapped ??= new List<int>();
                unmapped.Add(skillId);
            }

            if (unmapped != null)
            {
                unmappedSkillIds = unmapped;
            }

            return BgmSelectorSequence.Build(_originalLabel, skillLabels);
        }

        private static readonly IReadOnlyDictionary<int, string> EmptySkillLabels = new Dictionary<int, string>();
        private static readonly IReadOnlyList<int> EmptySkillIds = new List<int>();

        private readonly string _originalLabel;
        private readonly IReadOnlyDictionary<int, string> _skillLabels;
    }
}
