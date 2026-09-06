using KillChord.Runtime.Adaptor.OutGame.Skill;
using KillChord.Runtime.Domain.InGame.Skill;
using KillChord.Runtime.Domain.OutGame.SkillBuild;
using KillChord.Runtime.Domain.Player;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace KillChord.Runtime.Adaptor.OutGame.SkillBuild
{
    /// <summary>
    ///     スキル編成状態を ViewModel 用 DTO に変換して反映するプレゼンター。
    /// </summary>
    public sealed class SkillBuildPresenter
    {
        /// <summary>
        ///     プレゼンターを初期化する。
        /// </summary>
        /// <param name="viewModel"> 出力先 ViewModel。 </param>
        /// <param name="textFormatter"> 共通表示文字列フォーマッター。 </param>
        /// <param name="skillGenreIcons"> スキルジャンルとアイコンの対応表。 </param>
        /// <exception cref="ArgumentNullException"></exception>
        public SkillBuildPresenter(
            ISkillBuildViewModelWriter viewModel,
            SkillDisplayTextFormatter textFormatter,
            IReadOnlyDictionary<SkillType, Sprite> skillGenreIcons)
        {
            _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
            _textFormatter = textFormatter ?? throw new ArgumentNullException(nameof(textFormatter));
            _skillGenreIcons = skillGenreIcons;
        }

        /// <summary>
        ///     スキル編成状態を ViewModel に反映する。
        /// </summary>
        /// <param name="equippedSkills"> 現在装備中のスキル一覧。 </param>
        /// <param name="ownedSkills"> 入手済みスキル一覧。 </param>
        /// <param name="allSkills"> 全スキル一覧(未解放を含む)。 </param>
        /// <param name="ownedPoints"> 所持ポイント。 </param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public void Push(
            IReadOnlyList<EquippedSkill> equippedSkills,
            IReadOnlyList<SkillTemplate> ownedSkills,
            IReadOnlyCollection<SkillTemplate> allSkills,
            int ownedPoints)
        {
            if (equippedSkills == null)
            {
                throw new ArgumentNullException(nameof(equippedSkills));
            }

            if (ownedSkills == null)
            {
                throw new ArgumentNullException(nameof(ownedSkills));
            }

            if (allSkills == null)
            {
                throw new ArgumentNullException(nameof(allSkills));
            }

            SkillBuildSlotData[] slots = new SkillBuildSlotData[equippedSkills.Count];
            for (int i = 0; i < equippedSkills.Count; i++)
            {
                EquippedSkill equippedSkill = equippedSkills[i];
                int skillId = equippedSkill.HasSkill
                    ? equippedSkill.SkillTemplate.Id.Value
                    : EMPTY_SKILL_ID;
                slots[i] = new SkillBuildSlotData(i, skillId);
            }

            HashSet<int> ownedSkillIds = new();
            for (int i = 0; i < ownedSkills.Count; i++)
            {
                SkillTemplate skillTemplate = ownedSkills[i];
                if (skillTemplate == null)
                {
                    throw new ArgumentException($"入手済みスキル一覧に null が存在します。 index={i}", nameof(ownedSkills));
                }

                ownedSkillIds.Add(skillTemplate.Id.Value);
            }

            List<SkillViewData> unlockedSkills = new();
            List<SkillViewData> lockedSkills = new();
            foreach (SkillTemplate skillTemplate in allSkills)
            {
                if (skillTemplate == null)
                {
                    continue;
                }

                bool isUnlocked = ownedSkillIds.Contains(skillTemplate.Id.Value);
                SkillViewData viewData = BuildSkillViewData(skillTemplate, isUnlocked);
                (isUnlocked ? unlockedSkills : lockedSkills).Add(viewData);
            }

            // SkillId は文字列IDから焼き込まれたハッシュ値のため番号順にならない。
            // 表示名末尾の数字を「スキル番号」として抽出し、昇順に並び替える。
            unlockedSkills.Sort(CompareBySkillNumber);
            lockedSkills.Sort(CompareBySkillNumber);
            unlockedSkills.AddRange(lockedSkills);
            SkillBuildViewDTO dto = new(slots, unlockedSkills.ToArray(), ownedPoints);
            _viewModel.Apply(in dto);
        }

        private const int EMPTY_SKILL_ID = -1;

        private readonly ISkillBuildViewModelWriter _viewModel;
        private readonly SkillDisplayTextFormatter _textFormatter;
        private readonly IReadOnlyDictionary<SkillType, Sprite> _skillGenreIcons;
        private readonly Dictionary<SkillTemplate, SkillDisplayText> _textCache = new();

        /// <summary>
        ///     スキルテンプレートから表示用データを構築する。
        /// </summary>
        /// <param name="skillTemplate"> スキルテンプレート。 </param>
        /// <param name="isUnlocked"> 解放済みの場合は true。 </param>
        /// <returns> 表示用データ。 </returns>
        private SkillViewData BuildSkillViewData(SkillTemplate skillTemplate, bool isUnlocked)
        {
            SkillDisplayText text = GetOrCreateText(skillTemplate);
            return new SkillViewData(
                skillTemplate.Id.Value,
                skillTemplate.DisplayName,
                skillTemplate.Icon,
                text.ComboLabel,
                text.SkillTypeLabel,
                text.HasEffectDescription,
                text.EffectDescription,
                skillTemplate.Tips,
                skillTemplate.Level.Value,
                isUnlocked,
                ResolveGenreIcon(skillTemplate),
                ResolveGenreIds(skillTemplate));
        }

        /// <summary>
        ///     スキルの先頭ジャンルに対応するアイコンを取得する。
        /// </summary>
        /// <param name="skillTemplate"> スキルテンプレート。 </param>
        /// <returns> ジャンルアイコン。見つからない場合は null。 </returns>
        private Sprite ResolveGenreIcon(SkillTemplate skillTemplate)
        {
            if (_skillGenreIcons == null ||
                skillTemplate.Type == null ||
                skillTemplate.Type.Length == 0)
            {
                return null;
            }

            return _skillGenreIcons.TryGetValue(skillTemplate.Type[0], out Sprite icon) ? icon : null;
        }

        /// <summary>
        ///     スキルが属するジャンルを、View 層が Domain 型に依存せずに
        ///     絞り込み判定できるよう int 配列へ変換する。
        /// </summary>
        /// <param name="skillTemplate"> スキルテンプレート。 </param>
        /// <returns> ジャンル ID 配列。 </returns>
        private static int[] ResolveGenreIds(SkillTemplate skillTemplate)
        {
            if (skillTemplate.Type == null || skillTemplate.Type.Length == 0)
            {
                return Array.Empty<int>();
            }

            int[] result = new int[skillTemplate.Type.Length];
            for (int i = 0; i < skillTemplate.Type.Length; i++)
            {
                result[i] = (int)skillTemplate.Type[i];
            }

            return result;
        }

        /// <summary>
        ///     表示名の末尾から抽出した「スキル番号」で昇順比較する。
        /// </summary>
        /// <param name="a"> 比較対象。 </param>
        /// <param name="b"> 比較対象。 </param>
        /// <returns> 比較結果。 </returns>
        private static int CompareBySkillNumber(SkillViewData a, SkillViewData b)
        {
            return ExtractSkillNumber(a.DisplayName).CompareTo(ExtractSkillNumber(b.DisplayName));
        }

        /// <summary>
        ///     表示名末尾の数字を「スキル番号」として抽出する。
        /// </summary>
        /// <param name="displayName"> 表示名(例: "スキル13")。 </param>
        /// <returns> 抽出した番号。数字が見つからない場合は int.MaxValue(末尾へ)。 </returns>
        private static int ExtractSkillNumber(string displayName)
        {
            if (string.IsNullOrEmpty(displayName))
            {
                return int.MaxValue;
            }

            int end = displayName.Length;
            int start = end;
            while (start > 0 && char.IsDigit(displayName[start - 1]))
            {
                start--;
            }

            if (start == end)
            {
                return int.MaxValue;
            }

            return int.TryParse(displayName.Substring(start, end - start), out int number)
                ? number
                : int.MaxValue;
        }

        /// <summary>
        ///     スキルテンプレートの共通表示文字列を取得する。
        /// </summary>
        /// <param name="skillTemplate"> スキルテンプレート。 </param>
        /// <returns> 表示文字列。 </returns>
        private SkillDisplayText GetOrCreateText(SkillTemplate skillTemplate)
        {
            if (_textCache.TryGetValue(skillTemplate, out SkillDisplayText cachedText))
            {
                return cachedText;
            }

            SkillDisplayText text = _textFormatter.Format(skillTemplate);
            _textCache.Add(skillTemplate, text);
            return text;
        }
    }
}
