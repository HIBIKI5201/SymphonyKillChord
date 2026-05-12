using KillChord.Runtime.Domain.InGame.Mission.EvaluationCondition;
using SymphonyFrameWork.Attribute;
using System;
using UnityEngine;

namespace KillChord.Runtime.InfraStructure.InGame.Mission
{
    [Serializable]
    public abstract class MissionEvaluationConditionAssetBase : ISerializationCallbackReceiver
    {
        public abstract IMissionEvaluationCondition Create();

        public void OnBeforeSerialize()
        {
            _inspectorSummary = BuildSummary();
        }

        public void OnAfterDeserialize()
        {
        }

        protected string GetDisplayText()
        {
            if(!string.IsNullOrWhiteSpace(_displayText))
            {
                return _displayText;
            }

            return BuildSummary();
        }

        protected abstract string BuildSummary();

        [SerializeField,Header("説明用のテキスト")]　private string _displayText;

        [SerializeField, TextArea(2, 4), ReadOnly]
        private string _inspectorSummary;
    }
}
