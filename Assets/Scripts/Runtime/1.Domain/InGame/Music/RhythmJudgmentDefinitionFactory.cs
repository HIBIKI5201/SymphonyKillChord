using KillChord.Runtime.Domain.OutGame.StageSelect;
using System;
using System.Collections.Generic;

namespace KillChord.Runtime.Domain.InGame.Music
{
    /// <summary>
    ///     使える拍子の組み合わせから、リズム判定の定義（ゲージの範囲）を作るクラス。
    ///     通常の拍子は、隣り合う拍子の拍の位置（1小節 ÷ 拍子）の中間を境目にして範囲を分ける。
    ///     ジャストの付近だけを判定範囲にする拍子は、ジャストの幅に倍率を掛けた幅を、ジャストの中心から前後に取る。
    /// </summary>
    public class RhythmJudgmentDefinitionFactory
    {
        /// <summary>
        ///     新しいファクトリーを生成する。
        /// </summary>
        /// <param name="settings"> 拍子ごとの設定。同じ拍子は1つまで。 </param>
        /// <param name="narrowRangeJustWidthRatio"> ジャストの付近だけを判定範囲にする拍子で、判定範囲をジャストの幅の何倍にするか。 </param>
        public RhythmJudgmentDefinitionFactory(IReadOnlyList<BeatJudgmentSetting> settings, float narrowRangeJustWidthRatio)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            if (float.IsNaN(narrowRangeJustWidthRatio) || float.IsInfinity(narrowRangeJustWidthRatio)
                || narrowRangeJustWidthRatio <= 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(narrowRangeJustWidthRatio));
            }

            HashSet<BeatType> beatTypes = new();
            List<BeatType> allBeatTypes = new(settings.Count);
            for (int i = 0; i < settings.Count; i++)
            {
                if (!beatTypes.Add(settings[i].BeatType))
                {
                    throw new ArgumentException($"拍子 {settings[i].BeatType} の設定が重複しています。", nameof(settings));
                }

                allBeatTypes.Add(settings[i].BeatType);
            }

            _settings = settings;
            _allBeatTypes = allBeatTypes;
            _narrowRangeJustWidthRatio = narrowRangeJustWidthRatio;
        }

        /// <summary> 設定されている全ての拍子。 </summary>
        public IReadOnlyList<BeatType> AllBeatTypes => _allBeatTypes;

        /// <summary>
        ///     クリア済みのステージと挑戦中のステージから、使える拍子を求める。
        ///     解放条件のステージに挑戦中の場合も、その拍子を使える。
        /// </summary>
        /// <param name="clearedStageIds"> クリア済みのステージID。 </param>
        /// <param name="currentStageId"> 挑戦中のステージID。無い場合はnull。 </param>
        /// <returns> 使える拍子。 </returns>
        public IReadOnlyList<BeatType> ResolveAvailableBeatTypes(
            IReadOnlyCollection<StageId> clearedStageIds,
            StageId? currentStageId)
        {
            if (clearedStageIds == null)
            {
                throw new ArgumentNullException(nameof(clearedStageIds));
            }

            HashSet<StageId> clearedStages = new(clearedStageIds);
            List<BeatType> availableBeatTypes = new(_settings.Count);
            for (int i = 0; i < _settings.Count; i++)
            {
                BeatJudgmentSetting setting = _settings[i];
                bool isUnlocked = !setting.HasUnlockCondition
                    || clearedStages.Contains(setting.UnlockStageId)
                    || (currentStageId.HasValue && currentStageId.Value.Equals(setting.UnlockStageId));
                if (isUnlocked)
                {
                    availableBeatTypes.Add(setting.BeatType);
                }
            }

            return availableBeatTypes;
        }

        /// <summary>
        ///     使える拍子だけでリズム判定の定義を作る。使えない拍子は、範囲もジャストもゲージから消える。
        /// </summary>
        /// <param name="availableBeatTypes"> 使える拍子。 </param>
        /// <returns> 開始位置の順に判定範囲を並べたリズム判定の定義。 </returns>
        public RhythmJudgmentDefinition Create(IReadOnlyCollection<BeatType> availableBeatTypes)
        {
            if (availableBeatTypes == null)
            {
                throw new ArgumentNullException(nameof(availableBeatTypes));
            }

            HashSet<BeatType> available = new(availableBeatTypes);
            List<BeatJudgmentSetting> wideSettings = new(_settings.Count);
            List<BeatJudgmentSetting> narrowSettings = new(_settings.Count);
            for (int i = 0; i < _settings.Count; i++)
            {
                BeatJudgmentSetting setting = _settings[i];
                if (!available.Contains(setting.BeatType))
                {
                    continue;
                }

                if (setting.IsNarrowRange)
                {
                    narrowSettings.Add(setting);
                }
                else
                {
                    wideSettings.Add(setting);
                }
            }

            // 拍の位置が早い順（8・4・2・1拍子の順）に並べ、隣の拍子との中間を境目にする。
            wideSettings.Sort((left, right) => GetBeatPosition(left.BeatType).CompareTo(GetBeatPosition(right.BeatType)));
            List<RhythmJudgmentRange> narrowRanges = new(narrowSettings.Count);
            for (int i = 0; i < narrowSettings.Count; i++)
            {
                narrowRanges.Add(CreateNarrowRange(narrowSettings[i]));
            }

            List<RhythmJudgmentRange> ranges = new(wideSettings.Count + narrowRanges.Count * 2);
            for (int i = 0; i < wideSettings.Count; i++)
            {
                float start = i == 0
                    ? 0f
                    : GetMidpoint(wideSettings[i - 1].BeatType, wideSettings[i].BeatType);
                float end = i == wideSettings.Count - 1
                    ? 1f
                    : GetMidpoint(wideSettings[i].BeatType, wideSettings[i + 1].BeatType);
                AddWideRangeExcludingNarrowRanges(ranges, wideSettings[i], start, end, narrowRanges);
            }

            ranges.AddRange(narrowRanges);

            // ジャストが重なるときは先に並ぶ拍子を優先するため、開始位置の順に並べる。
            ranges.Sort((left, right) => left.StartNormalized.CompareTo(right.StartNormalized));
            return new RhythmJudgmentDefinition(ranges);
        }

        /// <summary> これより短い範囲は、狭い範囲で切り分けた残りとして作らない。 </summary>
        private const float MIN_RANGE_WIDTH = 0.0001f;

        private readonly IReadOnlyList<BeatJudgmentSetting> _settings;
        private readonly IReadOnlyList<BeatType> _allBeatTypes;
        private readonly float _narrowRangeJustWidthRatio;

        /// <summary>
        ///     拍の位置（1小節 ÷ 拍子）を求める。
        /// </summary>
        /// <param name="beatType"> 拍の種類。 </param>
        /// <returns> 1小節を1とした拍の位置。 </returns>
        private static float GetBeatPosition(BeatType beatType)
        {
            return 1f / (int)beatType;
        }

        /// <summary>
        ///     2つの拍子の拍の位置の中間を求める。
        /// </summary>
        /// <param name="earlier"> 拍の位置が早い拍子。 </param>
        /// <param name="later"> 拍の位置が遅い拍子。 </param>
        /// <returns> 中間の位置。 </returns>
        private static float GetMidpoint(BeatType earlier, BeatType later)
        {
            return (GetBeatPosition(earlier) + GetBeatPosition(later)) * 0.5f;
        }

        /// <summary>
        ///     ジャストの付近だけの判定範囲を作る。
        /// </summary>
        /// <param name="setting"> 拍子の設定。 </param>
        /// <returns> ジャストの中心から前後に、ジャストの幅に倍率を掛けた幅を取った範囲。 </returns>
        private RhythmJudgmentRange CreateNarrowRange(BeatJudgmentSetting setting)
        {
            float justCenter = (setting.JustStartNormalized + setting.JustEndNormalized) * 0.5f;
            float halfWidth = (setting.JustEndNormalized - setting.JustStartNormalized) * _narrowRangeJustWidthRatio * 0.5f;
            float start = Math.Clamp(justCenter - halfWidth, 0f, 1f);
            float end = Math.Clamp(justCenter + halfWidth, start, 1f);
            return new RhythmJudgmentRange(
                setting.BeatType, start, end, setting.JustStartNormalized, setting.JustEndNormalized);
        }

        /// <summary>
        ///     通常の拍子の範囲から、ジャストの付近だけの範囲と重なる部分を除いて追加する。
        /// </summary>
        /// <param name="ranges"> 追加先。 </param>
        /// <param name="setting"> 通常の拍子の設定。 </param>
        /// <param name="start"> 範囲の開始位置。 </param>
        /// <param name="end"> 範囲の終了位置。 </param>
        /// <param name="narrowRanges"> ジャストの付近だけの範囲。 </param>
        private static void AddWideRangeExcludingNarrowRanges(
            List<RhythmJudgmentRange> ranges,
            BeatJudgmentSetting setting,
            float start,
            float end,
            IReadOnlyList<RhythmJudgmentRange> narrowRanges)
        {
            // 狭い範囲を開始位置の順に見て、手前の残りを切り出していく。
            List<RhythmJudgmentRange> overlaps = new();
            for (int i = 0; i < narrowRanges.Count; i++)
            {
                RhythmJudgmentRange narrow = narrowRanges[i];
                if (narrow.EndNormalized > start && narrow.StartNormalized < end)
                {
                    overlaps.Add(narrow);
                }
            }

            overlaps.Sort((left, right) => left.StartNormalized.CompareTo(right.StartNormalized));
            float cursor = start;
            for (int i = 0; i < overlaps.Count; i++)
            {
                AddRangeIfWideEnough(ranges, setting, cursor, overlaps[i].StartNormalized);
                cursor = Math.Max(cursor, overlaps[i].EndNormalized);
            }

            AddRangeIfWideEnough(ranges, setting, cursor, end);
        }

        /// <summary>
        ///     幅がある場合だけ判定範囲を追加する。
        /// </summary>
        /// <param name="ranges"> 追加先。 </param>
        /// <param name="setting"> 拍子の設定。 </param>
        /// <param name="start"> 範囲の開始位置。 </param>
        /// <param name="end"> 範囲の終了位置。 </param>
        private static void AddRangeIfWideEnough(
            List<RhythmJudgmentRange> ranges,
            BeatJudgmentSetting setting,
            float start,
            float end)
        {
            if (end - start < MIN_RANGE_WIDTH)
            {
                return;
            }

            ranges.Add(new RhythmJudgmentRange(
                setting.BeatType, start, end, setting.JustStartNormalized, setting.JustEndNormalized));
        }
    }
}
