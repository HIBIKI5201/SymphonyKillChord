using System;
using UnityEngine;

namespace KillChord.Runtime.Domain.InGame.Mission.ClearCondition
{
    /// <summary>
    ///     内側の達成条件をラップし、このステップの開始時に説明ポップアップを表示する目印を兼ねるクリア条件です。
    ///     達成判定(＝ポップアップが閉じる条件)自体は内側の条件に委譲します。
    /// </summary>
    public sealed class PopupClearCondition : IMissionClearCondition, IObjectiveSequenceStepCondition, IObjectiveProgressReporter, IDecoratorClearCondition
    {
        /// <summary>
        ///     PopupClearCondition クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="innerCondition"> ポップアップが閉じる判定を委譲する内側の条件です。 </param>
        /// <param name="popupImage"> ポップアップに表示する画像です。不要な場合はnullです。 </param>
        public PopupClearCondition(IMissionClearCondition innerCondition, Sprite popupImage)
        {
            _innerCondition = innerCondition ?? throw new ArgumentNullException(nameof(innerCondition));
            PopupImage = popupImage;
        }

        /// <summary> ポップアップに表示する画像です。未設定の場合はnullです。 </summary>
        public Sprite PopupImage { get; }

        /// <inheritdoc />
        public IMissionClearCondition InnerCondition => _innerCondition;

        /// <inheritdoc />
        public bool IsSatisfied(MissionProgress progress)
        {
            return _innerCondition.IsSatisfied(progress);
        }

        /// <inheritdoc />
        public string GetDescription()
        {
            return _innerCondition.GetDescription();
        }

        /// <inheritdoc />
        public void BeginStep(MissionProgress progress)
        {
            if (_innerCondition is IObjectiveSequenceStepCondition stepCondition)
            {
                stepCondition.BeginStep(progress);
            }
        }

        /// <inheritdoc />
        public int CurrentCount(MissionProgress progress)
        {
            return _innerCondition is IObjectiveProgressReporter reporter ? reporter.CurrentCount(progress) : 0;
        }

        /// <inheritdoc />
        public int RequiredCount => _innerCondition is IObjectiveProgressReporter reporter ? reporter.RequiredCount : 0;

        private readonly IMissionClearCondition _innerCondition;
    }
}
