using KillChord.Runtime.Domain.InGame.Mission.FailCondition;
using SymphonyFrameWork.Attribute;
using System;
using UnityEngine;

namespace KillChord.Runtime.InfraStructure.InGame.Mission
{
    /// <summary>
    ///     ミッション失敗条件アセットの基底クラス。
    /// </summary>
    [Serializable]
    public abstract class MissionFailConditionAssetBase : ISerializationCallbackReceiver
    {
        /// <summary>
        ///     失敗条件を生成します。
        /// </summary>
        /// <returns>失敗条件。</returns>
        public abstract IMissionFailCondition Create();

        /// <summary>
        ///     シリアライズ前の処理を行います。
        /// </summary>
        public void OnBeforeSerialize()
        {
            _inspectorSummary = BuildSummary();
        }

        /// <summary>
        ///     デシリアライズ後の処理を行います。
        /// </summary>
        public void OnAfterDeserialize()
        {
        }

        /// <summary>
        ///     サマリーを構築します。
        /// </summary>
        /// <returns>サマリー文字列。</returns>
        protected abstract string BuildSummary();

        [SerializeField, TextArea(2, 4), ReadOnly, Tooltip("設定内容の要約。")]
        private string _inspectorSummary;
    }
}
