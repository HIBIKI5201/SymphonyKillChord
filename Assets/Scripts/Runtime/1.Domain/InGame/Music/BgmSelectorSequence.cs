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
        ///     並び順のセレクターラベル配列からシーケンスを生成する。
        /// </summary>
        /// <param name="labels"> 並び順のセレクターラベル配列。 </param>
        public BgmSelectorSequence(string[] labels)
        {
            _labels = labels ?? Array.Empty<string>();
        }

        /// <summary> シーケンスのステップ数を取得する。 </summary>
        public int Length => _labels?.Length ?? 0;

        /// <summary> 有効なラベルを1つ以上保持しているかどうかを取得する。 </summary>
        public bool HasLabels => Length > 0;

        /// <summary>
        ///     指定したステップに対応するラベルをループ解決する。
        ///     ステップは絶対値であり、負値も剰余を正規化して循環させる。
        /// </summary>
        /// <param name="step"> 進行ステップ。 </param>
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
        ///     2スキル時は [原曲, S1, 原曲, S2]、3スキル時は [原曲, S1, S2, S3] とする。
        ///     それ以外の個数は [原曲, S1, S2, …] とし、常に原曲へ戻れる並びを保証する。
        /// </summary>
        /// <param name="originalLabel"> 原曲のセレクターラベル。 </param>
        /// <param name="skillLabels"> 装備スキルのセレクターラベル列（ID昇順に整列済み）。 </param>
        /// <returns> 構築したシーケンス。スキルラベルが無い場合は空のシーケンス。 </returns>
        public static BgmSelectorSequence Build(string originalLabel, IReadOnlyList<string> skillLabels)
        {
            // スキルラベルが1つも無い場合は切り替える対象が無いため、
            // セレクターを操作せずCRIのデフォルト（通常BGM）に委ねる。
            if (skillLabels == null || skillLabels.Count == 0)
            {
                return new BgmSelectorSequence(Array.Empty<string>());
            }

            switch (skillLabels.Count)
            {
                case TWO_SKILL_COUNT:
                    return new BgmSelectorSequence(
                        new[] { originalLabel, skillLabels[0], originalLabel, skillLabels[1] });

                case THREE_SKILL_COUNT:
                    return new BgmSelectorSequence(
                        new[] { originalLabel, skillLabels[0], skillLabels[1], skillLabels[2] });

                default:
                    // 1個・4個以上は仕様上の規定が無いため、原曲を先頭に置いて全スキルを並べる。
                    // 1個時は [原曲, S1] となり、原曲とスキルが交互に鳴る。
                    string[] labels = new string[skillLabels.Count + 1];
                    labels[0] = originalLabel;
                    for (int i = 0; i < skillLabels.Count; i++)
                    {
                        labels[i + 1] = skillLabels[i];
                    }

                    return new BgmSelectorSequence(labels);
            }
        }

        /// <summary> [原曲, S1, 原曲, S2] の並びを適用するスキル数。 </summary>
        private const int TWO_SKILL_COUNT = 2;
        /// <summary> [原曲, S1, S2, S3] の並びを適用するスキル数。 </summary>
        private const int THREE_SKILL_COUNT = 3;

        private readonly string[] _labels;
    }
}
