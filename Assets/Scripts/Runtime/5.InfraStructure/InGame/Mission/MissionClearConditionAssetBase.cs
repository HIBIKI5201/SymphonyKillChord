using KillChord.Runtime.Domain.InGame.Mission.ClearCondition;
using SymphonyFrameWork.Attribute;
using System;
using UnityEngine;

namespace KillChord.Runtime.InfraStructure
{
    [Serializable]
    public abstract class MissionClearConditionAssetBase : ISerializationCallbackReceiver
    {
        public abstract IMissionClearCondition Create();

        public void OnBeforeSerialize()
        {
            _inspectorSummary = BuildSummary();
        }

        public void OnAfterDeserialize()
        {
        }

        protected abstract string BuildSummary();

        [SerializeField, TextArea(2, 4), ReadOnly]
        private string _inspectorSummary;
    }
}
