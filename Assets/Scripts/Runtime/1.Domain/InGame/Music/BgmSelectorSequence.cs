using System;
using System.Collections.Generic;

namespace KillChord.Runtime.Domain.InGame.Music
{
    /// <summary>
    ///     装備スキル構成に基づき、小節ごとに切り替えるBGMセレクターラベルの並びを表す値オブジェクト。
    /// </summary>
    public readonly struct BgmSelectorSequence
    {
        /// <summary>
        ///     小節順に並んだセレクターラベル配列からシーケンスを生成する。
        /// </summary>
        /// <param name="labels"> 小節順に並んだセレクターラベル配列。 </param>
        public BgmSelectorSequence(string[] labels)
        {
            _labels = labels ?? Array.Empty<string>();
        }

        /// <summary> シーケンスのステップ数を取得する。 </summary>
        public int Length => _labels?.Length ?? 0;

        /// <summary> 有効なラベルを1つ以上持つかどうかを取得する。 </summary>
        public bool HasLabels => Length > 0;

        /// <summary>
        ///     指定したステップに対応するラベルをループ解決する。
        /// </summary>
        /// <param name="step"> 進行ステップ。負値も許容し循環させる。 </param>
        /// <returns> 対応するセレクターラベル。ラベルが無い場合は空文字。 </returns>
        public string ResolveLabel(int step)
        {
            if (_labels == null || _labels.Length == 0)
            {
                return string.Empty;
            }

            int index = ((step % _labels.Length) + _labels.Length) % _labels.Length;
            return _labels[index];
        }

        /// <summary>
        ///     原曲ラベルと装備スキルラベル列から小節ループ用のシーケンスを構築する。
        ///     2スキル時は [原曲, S1, 原曲, S2]、3スキル時は [原曲, S1, S2, S3] の並びとする。
        /// </summary>
        /// <param name="originalLabel"> 原曲のセレクターラベル。 </param>
        /// <param name="skillLabels"> 装備スキルのセレクターラベル列（スロット順）。 </param>
        /// <returns> 構築したシーケンス。ラベルが無い場合は空のシーケンス。 </returns>
        public static BgmSelectorSequence Build(string originalLabel, IReadOnlyList<string> skillLabels)
        {
            if (skillLabels == null || skillLabels.Count == 0)
            {
                return new BgmSelectorSequence(Array.Empty<string>());
            }

            switch (skillLabels.Count)
            {
                case TWO_SKILL_COUNT:
                    return new BgmSelectorSequence(new[] { originalLabel, skillLabels[0], originalLabel, skillLabels[1] });
                case THREE_SKILL_COUNT:
                    return new BgmSelectorSequence(new[] { originalLabel, skillLabels[0], skillLabels[1], skillLabels[2] });
                default:
                    string[] fallback = new string[skillLabels.Count];
                    for (int i = 0; i < skillLabels.Count; i++)
                    {
                        fallback[i] = skillLabels[i];
                    }

                    return new BgmSelectorSequence(fallback);
            }
        }

        private const int TWO_SKILL_COUNT = 2;
        private const int THREE_SKILL_COUNT = 3;

        private readonly string[] _labels;
    }
}
