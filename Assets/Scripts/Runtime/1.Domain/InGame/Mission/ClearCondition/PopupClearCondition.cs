using UnityEngine;

namespace KillChord.Runtime.Domain.InGame.Mission.ClearCondition
{
    /// <summary>
    ///     このステップの開始時に説明ポップアップを表示し、閉じる入力で達成となるクリア条件です。
    ///     <para>
    ///         特定のアクションを説明するポップアップは、内側の条件(そのアクションの実行)で閉じます。
    ///         内側の条件が無いポップアップは、決定入力(<see cref="MissionActionKind.PopupConfirm"/>)で閉じます。
    ///         スマートフォンの画面タップも決定入力として通知されるため、内側の条件があっても閉じられます。
    ///     </para>
    /// </summary>
    public sealed class PopupClearCondition : IMissionClearCondition, IObjectiveSequenceStepCondition, IObjectiveProgressReporter, IDecoratorClearCondition
    {
        /// <summary>
        ///     PopupClearCondition クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="innerCondition"> 説明対象のアクションで閉じる場合の条件です。決定入力で閉じる場合はnullです。 </param>
        /// <param name="popupPrefab"> 表示する説明ポップアップのPrefabです。表示しない場合はnullです。 </param>
        public PopupClearCondition(
            IMissionClearCondition innerCondition,
            GameObject popupPrefab)
        {
            _innerCondition = innerCondition;
            PopupPrefab = popupPrefab;
        }

        /// <summary> 表示する説明ポップアップのPrefabです。未設定の場合はnullです。 </summary>
        public GameObject PopupPrefab { get; }

        /// <summary>
        ///     説明対象のアクションの実行で閉じるポップアップかどうかです。
        ///     falseの場合は、PCのUI決定ボタンで閉じます。
        /// </summary>
        public bool IsClosedByAction => _innerCondition != null;

        /// <inheritdoc />
        public IMissionClearCondition InnerCondition => _innerCondition;

        /// <inheritdoc />
        public bool IsSatisfied(MissionProgress progress)
        {
            if (progress.ActionRecord.GetCount(MissionActionKind.PopupConfirm) > _baselineConfirmCount)
            {
                return true;
            }

            return _innerCondition != null && _innerCondition.IsSatisfied(progress);
        }

        /// <inheritdoc />
        public string GetDescription()
        {
            return _innerCondition != null ? _innerCondition.GetDescription() : "説明ポップアップを決定入力で閉じる。";
        }

        /// <inheritdoc />
        public void BeginStep(MissionProgress progress)
        {
            // ステップ開始前の決定入力で閉じないよう、開始時点の回数を基準にする。
            _baselineConfirmCount = progress.ActionRecord.GetCount(MissionActionKind.PopupConfirm);

            if (_innerCondition is IObjectiveSequenceStepCondition stepCondition)
            {
                stepCondition.BeginStep(progress);
            }
        }

        /// <inheritdoc />
        public int CurrentCount(MissionProgress progress)
        {
            return 0;
        }

        /// <summary>
        ///     ポップアップは閉じる操作を案内するもので回数の目標ではないため、HUDに回数を出さないよう0を返します。
        /// </summary>
        public int RequiredCount => 0;

        private readonly IMissionClearCondition _innerCondition;
        private int _baselineConfirmCount;
    }
}
