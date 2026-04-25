using KillChord.Runtime.Domain.InGame.Mission;
using KillChord.Runtime.Domain.InGame.Mission.EvaluationCondition;
using SymphonyFrameWork.Attribute;
using System.Collections.Generic;
using UnityEngine;

namespace KillChord.Runtime.InfraStructure
{
    [CreateAssetMenu(fileName = nameof(MissionDefinition), menuName = "KillChord/Mission/" + nameof(MissionDefinition))]
    public class MissionDefinitionAsset : ScriptableObject
    {
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
        [SerializeField, TextArea] private string _plannerMemo;
#endif
        [Header("基礎情報")]
        [SerializeField] private string _missionId;
        [SerializeField] private string _displayName;

        [Header("UI情報")]
        [SerializeField, TextArea] private string _mainMissionText;

        [Header("クリア条件")]
        [SerializeReference, SubclassSelector]
        private MissionClearConditionAssetBase _clearCondition;

        [Header("失敗条件")]
        [SerializeReference, SubclassSelector]
        private MissionFailConditionAssetBase _failCondition;

        [Header("評価条件")]
        [SerializeReference, SubclassSelector]
        private List<MissionEvaluationConditionAssetBase> _evaluationConditions = new();
    }
}
