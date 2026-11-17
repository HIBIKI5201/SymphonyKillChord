using KillChord.Runtime.Domain.InGame.Music;
using KillChord.Runtime.Domain.OutGame.StageSelect;
using KillChord.Runtime.Utility.Identity;
using SymphonyFrameWork.Attribute;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace KillChord.Runtime.InfraStructure.InGame.Music
{
    /// <summary>
    ///     リズム判定の定義を保持するScriptableObject。
    ///     判定範囲は持たず、拍子ごとのジャスト範囲と解放条件から、使える拍子に合わせて範囲を作る。
    /// </summary>
    [CreateAssetMenu(fileName = nameof(RhythmJudgmentDefinitionAsset), menuName = "KillChord/RhythmJudgmentDefinition")]
    public class RhythmJudgmentDefinitionAsset : ScriptableObject
    {
        /// <summary>
        ///     ScriptableObjectのデータから、使える拍子に合わせて判定の定義を作るファクトリーを生成する。
        /// </summary>
        /// <returns> リズム判定定義のファクトリー。 </returns>
        public RhythmJudgmentDefinitionFactory ToFactory()
        {
            if (_beatSettings == null || _beatSettings.Length == 0)
            {
                return new RhythmJudgmentDefinitionFactory(Array.Empty<BeatJudgmentSetting>(), _narrowRangeJustWidthRatio);
            }

            List<BeatJudgmentSetting> settings = new(_beatSettings.Length);
            for (int i = 0; i < _beatSettings.Length; i++)
            {
                BeatSettingData data = _beatSettings[i];
                if (data == null)
                {
                    throw new InvalidOperationException($"_beatSettings[{i}] is null.");
                }

                settings.Add(new BeatJudgmentSetting(
                    data.BeatType, data.JustStartNormalized, data.JustEndNormalized,
                    data.IsNarrowRange, new StageId(data.UnlockStageId.Id)));
            }

            return new RhythmJudgmentDefinitionFactory(settings, _narrowRangeJustWidthRatio);
        }

        [Tooltip("拍子ごとのジャスト範囲と解放条件。判定範囲は、使える拍子の拍の位置の中間を境目にして自動で決まる。")]
        [SerializeField] private BeatSettingData[] _beatSettings;

        [Tooltip("ジャストの付近だけを判定範囲にする拍子で、判定範囲をジャストの幅の何倍にするか。ジャストの中心から前後に同じ幅を取る。")]
        [SerializeField, Min(1f)] private float _narrowRangeJustWidthRatio = 1.5f;

        /// <summary>
        ///     インスペクター設定用の拍子ごとのデータ構造。
        /// </summary>
        [Serializable]
        private class BeatSettingData
        {
            /// <summary> 拍の種類。 </summary>
            public BeatType BeatType;

            /// <summary> ジャスト開始位置（含む）。 </summary>
            [Min(0f), Tooltip("直前の入力からの小節進捗。ジャスト開始位置を含みます。")]
            public float JustStartNormalized;

            /// <summary> ジャスト終了位置（含まない）。 </summary>
            [Min(0f), Tooltip("ジャスト終了位置を含みません。1拍のジャスト範囲には1小節を超える値を指定できます。")]
            public float JustEndNormalized;

            /// <summary> ジャストの付近だけを判定範囲にするか。 </summary>
            [Tooltip("オンにすると、ほかの拍子の範囲を分けずに、ジャストの付近だけをこの拍子の範囲にする（3・6拍子）。")]
            public bool IsNarrowRange;

            /// <summary> クリアすると拍子が解放されるステージ。 </summary>
            [SourceDataCollection("StageAsset"), Tooltip("このステージをクリアするまで拍子を使えない（挑戦中は使える）。未設定なら最初から使える。")]
            public DataID UnlockStageId;
        }
    }
}
