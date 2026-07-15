using KillChord.Runtime.Domain.InGame.Music;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace KillChord.Runtime.InfraStructure.InGame.Music
{
    /// <summary>
    ///     リズム判定の定義を保持するScriptableObject。
    /// </summary>
    [CreateAssetMenu(fileName = nameof(RhythmJudgmentDefinitionAsset), menuName = "KillChord/RhythmJudgmentDefinition")]
    public class RhythmJudgmentDefinitionAsset : ScriptableObject
    {
        /// <summary>
        ///     ScriptableObjectのデータからドメイン層の定義オブジェクトを生成する。
        /// </summary>
        /// <returns> リズム判定定義。 </returns>
        public RhythmJudgmentDefinition ToDefinition()
        {
            if (_rangeData == null || _rangeData.Length == 0)
            {
                return new RhythmJudgmentDefinition(Array.Empty<RhythmJudgmentRange>());
            }

            List<RhythmJudgmentRange> judgmentRanges = new(_rangeData.Length);

            for (int i = 0; i < _rangeData.Length; i++)
            {
                RhythmJudgmentRangeData range = _rangeData[i];
                if (range == null)
                {
                    throw new InvalidOperationException($"_rangeData[{i}] is null.");
                }

                judgmentRanges.Add(new RhythmJudgmentRange(range.BeatType, range.StartNormalized, range.EndNormalized));
            }

            return new RhythmJudgmentDefinition(judgmentRanges);
        }

        [Tooltip("判定範囲データのリスト。")]
        [SerializeField] private RhythmJudgmentRangeData[] _rangeData;

        /// <summary>
        ///     インスペクター設定用の判定範囲データ構造。
        /// </summary>
        [Serializable]
        private class RhythmJudgmentRangeData
        {
            /// <summary> 拍の種類。 </summary>
            public BeatType BeatType;

            /// <summary> 開始位置（正規化）。 </summary>
            [Range(0f, 1f)]
            public float StartNormalized;

            /// <summary> 終了位置（正規化）。 </summary>
            [Range(0f, 1f)]
            public float EndNormalized;
        }
    }
}
