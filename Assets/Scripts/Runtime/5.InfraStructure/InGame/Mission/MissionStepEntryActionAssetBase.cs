using KillChord.Runtime.Domain.InGame.Mission.StepEntryAction;
using SymphonyFrameWork.Attribute;
using System;
using UnityEngine;

namespace KillChord.Runtime.InfraStructure.InGame.Mission
{
    /// <summary>
    ///     ミッション目標ステップ進入時アクションのアセット基底クラスです。
    /// </summary>
    [Serializable]
    public abstract class MissionStepEntryActionAssetBase : ISerializationCallbackReceiver
    {
        /// <summary>
        ///     目標ステップ進入時アクションを生成します。
        /// </summary>
        /// <returns>目標ステップ進入時アクションです。</returns>
        public abstract IMissionStepEntryAction Create();

        /// <summary>
        ///     シリアライズ前にインスペクター表示用サマリーを更新します。
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
        ///     インスペクター表示用サマリーを構築します。
        /// </summary>
        /// <returns>サマリー文字列です。</returns>
        protected abstract string BuildSummary();

        /// <summary> インスペクター表示用の設定サマリーです。 </summary>
        [SerializeField, TextArea(2, 4), ReadOnly, Tooltip("設定内容の要約です。")]
        private string _inspectorSummary;
    }
}
