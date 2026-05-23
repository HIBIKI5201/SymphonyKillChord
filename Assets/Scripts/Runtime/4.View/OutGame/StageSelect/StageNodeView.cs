using KillChord.Runtime.Adaptor.OutGame.StageSelect;
using KillChord.Runtime.View.OutGame.Screen;
using System;
using UnityEngine.UIElements;

namespace KillChord.Runtime.View.OutGame.StageSelect
{
    /// <summary>
    ///     ステージツリー上の1ノードを表す View クラス。
    ///     クリック時に OutGameUIEvent へ選択イベントを通知します。
    /// </summary>
    public sealed class StageNodeView : IStageNodeViewModel, IDisposable
    {
        /// <summary>
        ///     StageNodeView を初期化します。
        /// </summary>
        /// <param name="root"> このノードに対応する VisualElement。</param>
        /// <param name="nodeIndex"> ノードの ID。クリック時の通知に使用。</param>
        /// <param name="outGameUIEvent"> UI イベント管理クラス。</param>
        public StageNodeView(VisualElement root, int nodeIndex, OutGameUIEvent outGameUIEvent)
        {
            _root = root;
            _nodeId = nodeIndex;
            _outGameUIEvent = outGameUIEvent;

            _root.RegisterCallback<ClickEvent>(OnNodeClicked);
        }

        /// <summary>
        ///     ノードの解放状態に合わせて表示を更新します。
        /// </summary>
        /// <param name="status"> View 向けのステージ状態。</param>
        public void UpdateStatus(StageStatusView status)
        {
            _root.RemoveFromClassList(CLASS_LOCKED);
            _root.RemoveFromClassList(CLASS_UNLOCKED);
            _root.RemoveFromClassList(CLASS_CLEARED);

            // 状態に応じた USS クラスを適用する
            switch (status)
            {
                case StageStatusView.Locked:
                    _root.AddToClassList(CLASS_LOCKED);
                    _root.SetEnabled(false);
                    break;
                case StageStatusView.Unlocked:
                    _root.AddToClassList(CLASS_UNLOCKED);
                    _root.SetEnabled(true);
                    break;
                case StageStatusView.Cleared:
                    _root.AddToClassList(CLASS_CLEARED);
                    _root.SetEnabled(true);
                    break;
            }
        }

        /// <summary>
        ///     リソースを解放します。
        /// </summary>
        public void Dispose()
        {
            _root.UnregisterCallback<ClickEvent>(OnNodeClicked);
        }

        /// <summary>
        ///     ノードがクリックされたときの処理。
        /// </summary>
        private void OnNodeClicked(ClickEvent evt)
        {
            _outGameUIEvent.OnStageNodeSelected?.Invoke(_nodeId);
        }

        private const string CLASS_LOCKED = "stage-node--locked";
        private const string CLASS_UNLOCKED = "stage-node--unlocked";
        private const string CLASS_CLEARED = "stage-node--cleared";

        private readonly VisualElement _root;
        private readonly int _nodeId;
        private readonly OutGameUIEvent _outGameUIEvent;
    }
}
