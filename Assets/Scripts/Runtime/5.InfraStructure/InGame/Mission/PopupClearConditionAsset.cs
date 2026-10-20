using KillChord.Runtime.Domain.InGame.Mission.ClearCondition;
using SymphonyFrameWork.Attribute;
using System;
using UnityEngine;

namespace KillChord.Runtime.InfraStructure.InGame.Mission
{
    /// <summary>
    ///     内側のクリア条件をラップし、ステップ開始時に説明ポップアップを表示するクリア条件のアセットです。
    /// </summary>
    [Serializable]
    public sealed class PopupClearConditionAsset : MissionClearConditionAssetBase
    {
        /// <inheritdoc />
        public override IMissionClearCondition Create(EnemyMissionKeyRepository missionKeyRepository)
        {
            return new PopupClearCondition(
                _innerCondition?.Create(missionKeyRepository),
                _popupPrefab);
        }

        /// <inheritdoc />
        protected override string BuildSummary()
        {
            string innerName = _innerCondition != null ? _innerCondition.GetType().Name : "決定入力で閉じる";
            return $"ポップアップ表示 + {innerName}";
        }

        [SerializeReference, SubclassSelector, Tooltip("説明対象のアクションで閉じる場合の条件。空欄の場合はUIの決定ボタンで閉じる。スマホは常に画面タップで閉じる。")]
        private MissionClearConditionAssetBase _innerCondition;

        [SerializeField, Tooltip("表示する説明ポップアップのPrefab。TutorialPopup_Baseのバリアントを指定する。")]
        private GameObject _popupPrefab;
    }
}
