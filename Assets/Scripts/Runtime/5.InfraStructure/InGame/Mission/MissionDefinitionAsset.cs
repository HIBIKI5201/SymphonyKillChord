using KillChord.Runtime.Domain.InGame.Mission;
using KillChord.Runtime.Domain.InGame.Mission.ClearCondition;
using KillChord.Runtime.Domain.InGame.Mission.EvaluationCondition;
using SymphonyFrameWork.Attribute;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace KillChord.Runtime.InfraStructure.InGame.Mission
{
    /// <summary>
    ///     ミッション定義を保持するアセットクラス。
    /// </summary>
    [CreateAssetMenu(fileName = nameof(MissionDefinition), menuName = "KillChord/Mission/" + nameof(MissionDefinition))]
    public class MissionDefinitionAsset : ScriptableObject
    {
        /// <summary>
        ///     ミッション定義を生成します。
        /// </summary>
        /// <returns> ミッション定義。 </returns>
        public MissionDefinition Create()
        {
            List<ObjectiveSequenceStep> steps = new();

            if (_clearConditionSteps != null)
            {
                for (int i = 0; i < _clearConditionSteps.Count; i++)
                {
                    if (_clearConditionSteps[i] == null)
                    {
                        throw new InvalidOperationException(
                            $"クリア条件ステップ[{i}]が未設定です。 MissionId: {_missionId}");
                    }

                    steps.Add(_clearConditionSteps[i].Create());
                }
            }

            ObjectiveSequenceClearCondition clearCondition = new ObjectiveSequenceClearCondition(steps);

            List<IMissionEvaluationCondition> evaluations = new();

            HashSet<string> evaluationIds = new(StringComparer.Ordinal);

            for (int i = 0; i < _evaluationConditions.Count; i++)
            {
                MissionEvaluationConditionAssetBase condition = _evaluationConditions[i];

                if (condition == null)
                {
                    continue;
                }

                string evaluationId = condition.EvaluationIdValue;

                if (string.IsNullOrWhiteSpace(evaluationId))
                {
                    throw new InvalidOperationException(
                        $"評価条件[{i}]のEvaluationIdが未設定です。" +
                        $" MissionId: {_missionId}");
                }

                if (!evaluationIds.Add(evaluationId))
                {
                    throw new InvalidOperationException(
                        $"EvaluationIdが重複しています。" +
                        $" MissionId: {_missionId}, " +
                        $"EvaluationId: {evaluationId}");
                }

                evaluations.Add(condition.Create());
            }

            return new MissionDefinition(
                new MissionId(_missionId),
                _displayName,
                _mainMissionText,
                clearCondition,
                _failCondition?.Create(),
                evaluations,
                _defeatTips
            );
        }

#if UNITY_EDITOR
        [Header("プランナーメモ")]
        [SerializeField, TextArea, Tooltip("プランナー向けのメモ。ゲームには影響しません。")] private string _plannerMemo;
#endif
        [Header("基礎情報")]
        [SerializeField, Tooltip("ミッションを一意に識別するID。")] private string _missionId;
        [SerializeField, Tooltip("ミッションの表示名。")] private string _displayName;

        [Header("UI情報")]
        [SerializeField, TextArea, Tooltip("ミッションHUDに表示される説明文。")] private string _mainMissionText;
        [SerializeField, Tooltip("敗北時に表示する攻略Tips一覧。")] private List<string> _defeatTips = new();

        [Header("クリア条件")]
        [SerializeField, Tooltip("ミッションクリアとなる目標の並び。単一の条件で済む場合は要素数1にする。")]
        private List<ObjectiveSequenceStepAsset> _clearConditionSteps = new();

        [Header("失敗条件")]
        [SerializeReference, SubclassSelector, Tooltip("ミッション失敗となる条件。")]
        private MissionFailConditionAssetBase _failCondition;

        [Header("評価条件")]
        [SerializeReference, SubclassSelector, Tooltip("追加の達成目標。")]
        private List<MissionEvaluationConditionAssetBase> _evaluationConditions = new();
    }
}
