using KillChord.Runtime.Domain.InGame.Mission.ClearCondition;
using SymphonyFrameWork.Attribute;
using System;
using UnityEngine;

namespace KillChord.Runtime.InfraStructure.InGame.Mission
{
    /// <summary>
    ///     内側のクリア条件をラップし、ステップ開始時に次の敵Waveを生成するクリア条件のアセットです。
    /// </summary>
    [Serializable]
    public sealed class WaveStartClearConditionAsset : MissionClearConditionAssetBase
    {
        /// <inheritdoc />
        public override IMissionClearCondition Create()
        {
            if (_innerCondition == null)
            {
                throw new InvalidOperationException($"{nameof(_innerCondition)} is required.");
            }

            return new WaveStartClearCondition(_innerCondition.Create());
        }

        /// <inheritdoc />
        protected override string BuildSummary()
        {
            string innerName = _innerCondition != null ? _innerCondition.GetType().Name : "未設定";
            return $"Wave開始 + {innerName}";
        }

        [SerializeReference, SubclassSelector, Tooltip("Wave完了の判定を委譲する内側のクリア条件。")]
        private MissionClearConditionAssetBase _innerCondition;
    }
}
