using KillChord.Runtime.Domain.InGame.Mission.ClearCondition;
using KillChord.Runtime.Domain.InGame.Mission.StepEntryAction;
using SymphonyFrameWork.Attribute;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace KillChord.Runtime.InfraStructure.InGame.Mission
{
    /// <summary>
    ///     目標シーケンスの1ステップを表すアセットクラス。達成条件、案内メッセージ、進入時アクションを保持する。
    ///     Wave開始やポップアップ表示などの達成条件に付随する振る舞いは、達成条件側のデコレータ(<see cref="WaveStartClearConditionAsset"/>、
    ///     <see cref="PopupClearConditionAsset"/>)で表現する。
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

            return new ObjectiveSequenceStep(
                _condition.Create(missionKeyRepository),
                _guideMessageText,
                CreateEntryActions());
        }

        [SerializeReference, SubclassSelector, Tooltip("このステップの達成条件。")]
        private MissionClearConditionAssetBase _condition;

        [SerializeField, TextArea(2, 4), Tooltip("ステップ開始時に案内するメッセージ。不要な場合は空欄。")]
        private string _guideMessageText;

        /// <summary> ステップ進入時に実行するアクション一覧です。 </summary>
        [SerializeReference, SubclassSelector, Tooltip("ステップ進入時に実行するアクション一覧。不要な場合は空のままにする。")]
        private List<MissionStepEntryActionAssetBase> _entryActions = new();

        /// <summary>
        ///     ステップ進入時アクション一覧を生成します。
        /// </summary>
        /// <returns>ステップ進入時アクション一覧です。</returns>
        private IReadOnlyList<IMissionStepEntryAction> CreateEntryActions()
        {
            if (_entryActions == null || _entryActions.Count == 0)
            {
                return Array.Empty<IMissionStepEntryAction>();
            }

            List<IMissionStepEntryAction> actions = new(_entryActions.Count);
            for (int i = 0; i < _entryActions.Count; i++)
            {
                MissionStepEntryActionAssetBase entryAction = _entryActions[i];
                if (entryAction == null)
                {
                    throw new InvalidOperationException($"{nameof(_entryActions)}[{i}] is required.");
                }

                try
                {
                    actions.Add(entryAction.Create());
                }
                catch (Exception exception)
                {
                    throw new InvalidOperationException(
                        $"{nameof(_entryActions)}[{i}] ({entryAction.GetType().Name}) の生成に失敗しました。"
                        + $" GuideMessageText: {_guideMessageText}",
                        exception);
                }
            }

            return actions;
        }
    }
}
