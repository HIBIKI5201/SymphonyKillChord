using System.Collections.Generic;

namespace KillChord.Runtime.Domain.InGame.Music
{
    /// <summary>
    ///     スキルIDとBGMセレクターラベルの対応、および原曲ラベルを保持するドメインオブジェクト。
    /// </summary>
    public sealed class SkillBgmSelectorTable
    {
        /// <summary>
        ///     原曲ラベルとスキルIDごとのラベル対応からテーブルを生成する。
        /// </summary>
        /// <param name="originalLabel"> 原曲のセレクターラベル。 </param>
        /// <param name="skillLabels"> スキルIDとセレクターラベルの対応。 </param>
        public SkillBgmSelectorTable(string originalLabel, IReadOnlyDictionary<int, string> skillLabels)
        {
            _originalLabel = originalLabel ?? string.Empty;
            _skillLabels = skillLabels ?? EmptySkillLabels;
        }

        /// <summary>
        ///     装備スキルID列から小節ループ用のシーケンスを構築する。
        /// </summary>
        /// <param name="equippedSkillIds"> 装備スキルのID列（スロット順）。 </param>
        /// <returns> 構築したシーケンス。対応ラベルが無い場合は空のシーケンス。 </returns>
        public BgmSelectorSequence CreateSequence(IReadOnlyList<int> equippedSkillIds)
        {
            if (equippedSkillIds == null || equippedSkillIds.Count == 0)
            {
                return new BgmSelectorSequence(null);
            }

            List<string> skillLabels = new(equippedSkillIds.Count);
            for (int i = 0; i < equippedSkillIds.Count; i++)
            {
                if (_skillLabels.TryGetValue(equippedSkillIds[i], out string label)
                    && !string.IsNullOrEmpty(label))
                {
                    skillLabels.Add(label);
                }
            }

            return BgmSelectorSequence.Build(_originalLabel, skillLabels);
        }

        private static readonly IReadOnlyDictionary<int, string> EmptySkillLabels = new Dictionary<int, string>();

        private readonly string _originalLabel;
        private readonly IReadOnlyDictionary<int, string> _skillLabels;
    }
}
