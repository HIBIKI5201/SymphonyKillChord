using KillChord.Runtime.Domain.InGame.Mission.ClearCondition;
using SymphonyFrameWork.Attribute;
using System;
using UnityEngine;

namespace KillChord.Runtime.InfraStructure.InGame.Mission
{
    /// <summary>
    ///     目標シーケンスの1ステップを表すアセットクラス。達成条件と案内メッセージを保持する。
    ///     Wave開始やポップアップ表示などの特殊な振る舞いは、達成条件側のデコレータ(<see cref="WaveStartClearConditionAsset"/>、
    ///     <see cref="PopupClearConditionAsset"/>)で表現するため、本クラス自体は継承しない。
    /// </summary>
    [Serializable]
    public sealed class ObjectiveSequenceStepAsset
    {
        /// <summary>
        ///     ステップを生成します。
        /// </summary>
        /// <param name="missionKeyRepository"> 敵ミッションキーの解決に使うリポジトリです。 </param>
        /// <returns> ステップ。 </returns>
        public ObjectiveSequenceStep Create(EnemyMissionKeyRepository missionKeyRepository)
        {
            if (_condition == null)
            {
                throw new InvalidOperationException($"{nameof(_condition)} is required.");
            }

            return new ObjectiveSequenceStep(_condition.Create(missionKeyRepository), _guideMessageText);
        }

        [SerializeReference, SubclassSelector, Tooltip("このステップの達成条件。")]
        private MissionClearConditionAssetBase _condition;

        [SerializeField, TextArea(2, 4), Tooltip("ステップ開始時に案内するメッセージ。不要な場合は空欄。")]
        private string _guideMessageText;
    }
}
