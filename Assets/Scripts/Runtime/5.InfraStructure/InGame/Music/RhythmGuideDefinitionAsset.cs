using KillChord.Runtime.Domain.InGame.Music;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms;

namespace KillChord.Runtime.InfraStructure.InGame.Music
{
    /// <summary>
    ///     リズムガイドの定義を保持するScriptableObject。
    /// </summary>
    [CreateAssetMenu(fileName = nameof(RhythmGuideDefinitionAsset), menuName = "KillChord/RhythmGuideDefinition")]
    public class RhythmGuideDefinitionAsset : ScriptableObject
    {
        /// <summary>
        ///     ScriptableObjectのデータからドメイン層の定義オブジェクトを生成する。
        /// </summary>
        /// <returns> リズムガイド定義。 </returns>
        public RhythmGuideDefinition ToDefinition()
        {
            if (_rangeData == null || _rangeData.Length == 0)
            {
                return new RhythmGuideDefinition(Array.Empty<RhythmGuideRange>());
            }

            List<RhythmGuideRange> guideRanges = new(_rangeData.Length);

            for (int i = 0; i < _rangeData.Length; i++)
            {
                RhythmGuideRangeData range = _rangeData[i];
                if (range == null)
                {
                    throw new InvalidOperationException($"_rangeData[{i}] is null.");
                }
                guideRanges.Add(new RhythmGuideRange(range.BeatType, range.StartNormalized, range.EndNormalized));
            }

            return new RhythmGuideDefinition(guideRanges);
        }

        [Tooltip("判定範囲データのリスト。")]
        [SerializeField] private RhythmGuideRangeData[] _rangeData;

        /// <summary>
        ///     インスペクター設定用の判定範囲データ構造。
        /// </summary>
        [Serializable]
        private class RhythmGuideRangeData
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
