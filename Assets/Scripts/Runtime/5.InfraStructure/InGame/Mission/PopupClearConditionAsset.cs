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
            if (_innerCondition == null)
            {
                throw new InvalidOperationException($"{nameof(_innerCondition)} is required.");
            }

            return new PopupClearCondition(
                _innerCondition.Create(missionKeyRepository),
                ResolvePopupImageEntryKey(),
                _popupImage);
        }

        /// <inheritdoc />
        protected override string BuildSummary()
        {
            string innerName = _innerCondition != null ? _innerCondition.GetType().Name : "未設定";
            return $"ポップアップ表示 + {innerName}";
        }

        [SerializeReference, SubclassSelector, Tooltip("ポップアップが閉じる判定を委譲する内側のクリア条件。")]
        private MissionClearConditionAssetBase _innerCondition;

        [SerializeField, Tooltip("TutorialPopupImagesテーブルのエントリーキー。空欄の場合は画像名を使用する。")]
        private string _popupImageEntryKey;

        [SerializeField, Tooltip("ローカライズ画像を取得できない場合に表示する画像。不要な場合は空欄。")]
        private Sprite _popupImage;

        /// <summary>
        ///     ポップアップ画像テーブルのエントリーキーを解決します。
        /// </summary>
        /// <returns> 設定済みのキー、または従来画像のアセット名です。 </returns>
        private string ResolvePopupImageEntryKey()
        {
            if (!string.IsNullOrWhiteSpace(_popupImageEntryKey))
            {
                return _popupImageEntryKey;
            }

            return _popupImage != null ? _popupImage.name : string.Empty;
        }
    }
}
