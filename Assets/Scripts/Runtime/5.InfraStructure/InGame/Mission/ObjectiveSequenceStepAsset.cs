using KillChord.Runtime.Domain.InGame.Mission.ClearCondition;
using SymphonyFrameWork.Attribute;
using System;
using UnityEngine;

namespace KillChord.Runtime.InfraStructure.InGame.Mission
{
    /// <summary>
    ///     目標シーケンスの1ステップを表すアセットクラス。達成条件と案内メッセージを保持する。
    /// </summary>
    [Serializable]
    public class ObjectiveSequenceStepAsset
    {
        /// <summary>
        ///     ステップを生成します。
        /// </summary>
        /// <returns> ステップ。 </returns>
        public ObjectiveSequenceStep Create()
        {
            if (_condition == null)
            {
                throw new InvalidOperationException($"{nameof(_condition)} is required.");
            }

            IMissionClearCondition condition = _condition.Create();
            return _startsEnemyWave
                ? new WaveObjectiveSequenceStep(condition, _guideMessageText)
                : new ObjectiveSequenceStep(condition, _guideMessageText);
        }

        [SerializeReference, SubclassSelector, Tooltip("このステップの達成条件。")]
        private MissionClearConditionAssetBase _condition;

        [SerializeField, TextArea(2, 4), Tooltip("ステップ開始時に案内するメッセージ。不要な場合は空欄。")]
        private string _guideMessageText;

        [SerializeField, Tooltip("このステップの開始時に次の敵Waveを生成する場合はオンにする。")]
        private bool _startsEnemyWave;
    }
}
