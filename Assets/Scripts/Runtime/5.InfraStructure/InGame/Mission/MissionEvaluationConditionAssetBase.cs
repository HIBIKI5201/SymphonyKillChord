using KillChord.Runtime.Domain.InGame.Mission;
using KillChord.Runtime.Domain.InGame.Mission.EvaluationCondition;
using SymphonyFrameWork.Attribute;
using System;
using UnityEngine;

namespace KillChord.Runtime.InfraStructure.InGame.Mission
{
    /// <summary>
    ///     ミッション評価条件アセットの基底クラス。
    /// </summary>
    [Serializable]
    public abstract class MissionEvaluationConditionAssetBase : ISerializationCallbackReceiver
    {
        /// <summary>
        ///     評価条件を生成します。
        /// </summary>
        /// <returns>評価条件。</returns>
        public abstract IMissionEvaluationCondition Create();

        /// <summary>
        ///     評価条件を永続的に隙別するId。
        /// </summary>
        protected MissionEvaluationId EvaluationId => new(_evaluationId);

        /// <summary>
        ///     EvaluationのId。
        /// </summary>
        public string EvaluationIdValue => _evaluationId;

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
        ///     表示用のテキストを取得します。
        /// </summary>
        /// <returns>表示テキスト。</returns>
        protected string GetDisplayText()
        {
            if (!string.IsNullOrWhiteSpace(_displayText))
            {
                return _displayText;
            }

            return BuildSummary();
        }

        /// <summary>
        ///     サマリーを構築します。
        /// </summary>
        /// <returns>サマリー文字列。</returns>
        protected abstract string BuildSummary();

        [SerializeField, Header("識別用Id")] private string _evaluationId;

        [SerializeField, Header("説明用のテキスト"), Tooltip("ミッションHUDに表示される条件の説明。空の場合は自動生成されます。")] private string _displayText;

        [SerializeField, TextArea(2, 4), ReadOnly, Tooltip("設定内容の要約。")]
        private string _inspectorSummary;
    }
}
