using KillChord.Runtime.Domain.InGame.Mission.ClearCondition;
using KillChord.Runtime.Domain.InGame.Mission.EvaluationCondition;
using KillChord.Runtime.Domain.InGame.Mission.FailCondition;
using System;
using System.Collections.Generic;

namespace KillChord.Runtime.Domain.InGame.Mission
{
    /// <summary>
    ///     ミッションの定義を表すクラス。
    /// </summary>
    public class MissionDefinition
    {
        /// <summary>
        ///     MissionDefinition クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="missionId">ミッションID。</param>
        /// <param name="displayName">表示名。</param>
        /// <param name="mainMissionText">メインミッションのテキスト。</param>
        /// <param name="clearCondition">クリア条件。</param>
        /// <param name="failCondition">失敗条件。</param>
        /// <param name="evaluationConditions">評価条件のリスト。</param>
        /// <param name="defeatTips"> 敗北時に表示する攻略Tips一覧。 </param>
        public MissionDefinition(
            MissionId missionId,
            string displayName,
            string mainMissionText,
            ObjectiveSequenceClearCondition clearCondition,
            IMissionFailCondition failCondition,
            IReadOnlyList<IMissionEvaluationCondition> evaluationConditions,
            IReadOnlyList<string> defeatTips)
        {
            MissionId = missionId;
            DisplayName = displayName;
            MainMissionText = mainMissionText;
            ClearCondition = clearCondition;
            FailCondition = failCondition;
            EvaluationConditions = evaluationConditions;
            _defeatTips = CreateDefeatTips(defeatTips);
        }

        /// <summary> ミッションIDを取得します。 </summary>
        public MissionId MissionId { get; }
        /// <summary> 表示名を取得します。 </summary>
        public string DisplayName { get; }
        /// <summary> メインミッションのテキストを取得します。 </summary>
        public string MainMissionText { get; }

        /// <summary>
        ///     クリア条件を取得します。全てのミッションは目標シーケンスとして表現され、
        ///     単一の条件しか持たないミッションは要素数1のシーケンスとして扱います。
        /// </summary>
        public ObjectiveSequenceClearCondition ClearCondition { get; }
        /// <summary> 失敗条件を取得します。 </summary>
        public IMissionFailCondition FailCondition { get; }
        /// <summary> 評価条件のリストを取得します。 </summary>
        public IReadOnlyList<IMissionEvaluationCondition> EvaluationConditions { get; }
        /// <summary> 敗北時に表示するTips一覧。 </summary>
        public IReadOnlyList<string> DefeatTips => _defeatTips;

        private readonly IReadOnlyList<string> _defeatTips;

        /// <summary>
        ///     空文字を除外した敗北Tips一覧を生成する。
        /// </summary>
        /// <param name="defeatTips"> 元のTips一覧。 </param>
        /// <returns> 有効なTips一覧。 </returns>
        private static IReadOnlyList<string> CreateDefeatTips(IReadOnlyList<string> defeatTips)
        {
            if (defeatTips == null || defeatTips.Count == 0)
            {
                return Array.Empty<string>();
            }

            List<string> validTips = new(defeatTips.Count);

            for (int i = 0; i < defeatTips.Count; i++)
            {
                string tip = defeatTips[i];

                if (string.IsNullOrWhiteSpace(tip))
                {
                    continue;
                }

                validTips.Add(tip);
            }

            return validTips.AsReadOnly();
        }
    }
}
