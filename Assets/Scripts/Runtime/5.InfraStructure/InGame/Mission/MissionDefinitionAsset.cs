using KillChord.Runtime.Domain.InGame.Mission;
using KillChord.Runtime.Domain.InGame.Mission.EvaluationCondition;
using SymphonyFrameWork.Attribute;
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
        /// <returns>ミッション定義。</returns>
        public MissionDefinition Create()
        {
            List<IMissionEvaluationCondition> evaluations = new();

            for (int i = 0; i < _evaluationConditions.Count; i++)
            {
                if (_evaluationConditions[i] != null)
                {
                    evaluations.Add(_evaluationConditions[i].Create());
                }
            }

            return new MissionDefinition(
                new MissionId(_missionId),
                _displayName,
                _mainMissionText,
                _clearCondition?.Create(),
                _failCondition?.Create(),
                evaluations
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

        [Header("クリア条件")]
        [SerializeReference, SubclassSelector, Tooltip("ミッションクリアとなる条件。")]
        private MissionClearConditionAssetBase _clearCondition;

        [Header("失敗条件")]
        [SerializeReference, SubclassSelector, Tooltip("ミッション失敗となる条件。")]
        private MissionFailConditionAssetBase _failCondition;

        [Header("評価条件")]
        [SerializeReference, SubclassSelector, Tooltip("追加の達成目標。")]
        private List<MissionEvaluationConditionAssetBase> _evaluationConditions = new();
    }
}
