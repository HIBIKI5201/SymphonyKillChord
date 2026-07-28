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
        public override IMissionClearCondition Create()
        {
            if (_innerCondition == null)
            {
                throw new InvalidOperationException($"{nameof(_innerCondition)} is required.");
            }

            return new PopupClearCondition(_innerCondition.Create(), _popupImage);
        }

        /// <inheritdoc />
        protected override string BuildSummary()
        {
            string innerName = _innerCondition != null ? _innerCondition.GetType().Name : "未設定";
            return $"ポップアップ表示 + {innerName}";
        }

        [SerializeReference, SubclassSelector, Tooltip("ポップアップが閉じる判定を委譲する内側のクリア条件。")]
        private MissionClearConditionAssetBase _innerCondition;

        [SerializeField, Tooltip("ポップアップに表示する画像。不要な場合は空欄。")]
        private Sprite _popupImage;
    }
}
