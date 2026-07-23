using KillChord.Runtime.Domain.InGame.Mission;
using KillChord.Runtime.Domain.InGame.Mission.ClearCondition;
using KillChord.Runtime.Domain.InGame.Music;
using System;
using UnityEngine;

namespace KillChord.Runtime.InfraStructure.InGame.Mission
{
    /// <summary>
    ///     指定拍子の攻撃を必要回数実行するクリア条件のアセットです。
    /// </summary>
    [Serializable]
    public sealed class AttackBeatRepeatCountClearConditionAsset : MissionClearConditionAssetBase
    {
        /// <inheritdoc />
        public override IMissionClearCondition Create()
        {
            if (_requiredCount <= 0)
            {
                throw new InvalidOperationException($"{nameof(_requiredCount)} must be greater than 0.");
            }

            return new ActionRepeatCountClearCondition(ToMissionActionKind(_beatType), _requiredCount);
        }

        /// <inheritdoc />
        protected override string BuildSummary()
        {
            return $"{(int)_beatType}拍子の攻撃を{_requiredCount}回以上行う条件";
        }

        [SerializeField, Tooltip("達成対象となる攻撃の拍子です。")]
        private BeatType _beatType = BeatType.One;

        [SerializeField, Min(1), Tooltip("クリアに必要な攻撃回数です。")]
        private int _requiredCount = 1;

        /// <summary>
        ///     拍子をMission行動種別へ変換します。
        /// </summary>
        /// <param name="beatType">変換する拍子です。</param>
        /// <returns>対応するMission行動種別です。</returns>
        /// <exception cref="ArgumentOutOfRangeException">未対応の拍子が指定されました。</exception>
        private static MissionActionKind ToMissionActionKind(BeatType beatType)
        {
            return beatType switch
            {
                BeatType.One => MissionActionKind.AttackOneBeat,
                BeatType.Two => MissionActionKind.AttackTwoBeat,
                BeatType.Three => MissionActionKind.AttackThreeBeat,
                BeatType.Four => MissionActionKind.AttackFourBeat,
                BeatType.Six => MissionActionKind.AttackSixBeat,
                BeatType.Eight => MissionActionKind.AttackEightBeat,
                _ => throw new ArgumentOutOfRangeException(nameof(beatType), beatType, "未対応の拍子です。"),
            };
        }
    }
}
