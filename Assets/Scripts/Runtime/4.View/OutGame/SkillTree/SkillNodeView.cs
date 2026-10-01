using KillChord.Runtime.Adaptor.OutGame.SkillTree;
using KillChord.Runtime.Utility.OutGame;
using KillChord.Runtime.View.OutGame.Navigation;
using KillChord.Runtime.View.OutGame.Screen;
using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace KillChord.Runtime.View.OutGame.SkillTree
{
    /// <summary>
    ///     スキルノードのViewクラス。
    /// </summary>
    public class SkillNodeView : ISkillNodeViewModel, IDisposable
    {
        /// <summary>
        ///     ノードの要素と ID を指定して生成する。
        /// </summary>
        public SkillNodeView(VisualElement root, int nodeId, OutGameUIEvent outGameUIEvent)
        {
            _root = root;
            _nodeId = nodeId;
            _outGameUIEvent = outGameUIEvent;

            _root.MakeNavigable();
            _activationRegistration = _root.RegisterActivation(HandleActivationHandler);

            SetLocked();
        }

        /// <summary>
        ///     操作の登録を解除し、再生中の演出を止める。
        /// </summary>
        public void Dispose()
        {
            _activationRegistration.Dispose();
            _unlockPopRemovalItem?.Pause();
            StopSelectedGlow();
        }

        /// <summary> このノードの要素を取得します。初期フォーカスの設定に使用します。 </summary>
        public VisualElement RootElement => _root;

        /// <summary>
        ///     スキルノードを解放済みにする。
        /// </summary>
        public void SetUnlocked()
        {
            _root.RemoveFromClassList(UssClassNameConstants.USS_CLASS_SKILL_NODE_LOCKED);
            _root.AddToClassList(UssClassNameConstants.USS_CLASS_SKILL_NODE_UNLOCKED);
            PlayUnlockPop();
        }

        /// <summary>
        ///     スキルノードを未解放にする。
        /// </summary>
        public void SetLocked()
        {
            _root.RemoveFromClassList(UssClassNameConstants.USS_CLASS_SKILL_NODE_UNLOCKED);
            _root.RemoveFromClassList(UssClassNameConstants.USS_CLASS_SKILL_NODE_SELECTED);
            _root.AddToClassList(UssClassNameConstants.USS_CLASS_SKILL_NODE_LOCKED);
            StopSelectedGlow();
        }

        /// <summary>
        ///     スキルノードを選択済みにする。
        /// </summary>
        public void SetSelected()
        {
            _root.AddToClassList(UssClassNameConstants.USS_CLASS_SKILL_NODE_SELECTED);
            StartSelectedGlow();
        }

        /// <summary>
        ///     スキルノードを未選択にする。
        /// </summary>
        public void SetUnSelected()
        {
            _root.RemoveFromClassList(UssClassNameConstants.USS_CLASS_SKILL_NODE_SELECTED);
            StopSelectedGlow();
        }

        /// <summary>
        ///     ノードが強化するステータスの種別アイコンを設定する。
        /// </summary>
        /// <param name="icon"> 表示するアイコン。null の場合はアイコンを非表示にする。 </param>
        public void SetIcon(Sprite icon)
        {
            if (icon == null)
            {
                _root.style.backgroundImage = new StyleBackground();
                _root.RemoveFromClassList(UssClassNameConstants.USS_CLASS_SKILL_NODE_ICON);
                return;
            }

            _root.style.backgroundImage = new StyleBackground(icon);
            _root.AddToClassList(UssClassNameConstants.USS_CLASS_SKILL_NODE_ICON);
        }

        /// <summary> 解放ポップ演出を維持する時間(ミリ秒)。SkillNode.ussのscale transition-durationと一致させること。 </summary>
        private const long UNLOCK_POP_DURATION_MILLISECONDS = 180L;

        /// <summary> 選択中グローのパルス間隔(ミリ秒)。SkillNode.ussのbox-shadow transition-durationと一致させること。 </summary>
        private const long SELECTED_GLOW_PULSE_INTERVAL_MILLISECONDS = 600L;

        private readonly int _nodeId;
        private readonly VisualElement _root;
        private readonly OutGameUIEvent _outGameUIEvent;
        private readonly IDisposable _activationRegistration;
        private IVisualElementScheduledItem _unlockPopRemovalItem;
        private IVisualElementScheduledItem _selectedGlowPulseItem;
        private bool _isGlowAtPeak;

        /// <summary>
        ///     スキルノードが作動した時の処理。
        /// </summary>
        private void HandleActivationHandler()
        {
            SetSelected();
            _outGameUIEvent.OnSkillNodeSelected?.Invoke(_root.name);
        }

        /// <summary>
        ///     解放時に一度だけ再生するポップ演出を開始する。
        /// </summary>
        private void PlayUnlockPop()
        {
            _unlockPopRemovalItem?.Pause();
            _root.AddToClassList(UssClassNameConstants.USS_CLASS_SKILL_NODE_UNLOCK_POP);
            _unlockPopRemovalItem = _root.schedule.Execute(() =>
            {
                _root.RemoveFromClassList(UssClassNameConstants.USS_CLASS_SKILL_NODE_UNLOCK_POP);
                _unlockPopRemovalItem = null;
            }).StartingIn(UNLOCK_POP_DURATION_MILLISECONDS);
        }

        /// <summary>
        ///     選択中の淡いパルスグローを開始する。
        /// </summary>
        private void StartSelectedGlow()
        {
            StopSelectedGlow();
            _root.AddToClassList(UssClassNameConstants.USS_CLASS_SKILL_NODE_SELECTED_GLOW);
            _isGlowAtPeak = false;
            _selectedGlowPulseItem = _root.schedule.Execute(() =>
            {
                _isGlowAtPeak = !_isGlowAtPeak;
                if (_isGlowAtPeak)
                {
                    _root.AddToClassList(UssClassNameConstants.USS_CLASS_SKILL_NODE_SELECTED_GLOW_PEAK);
                }
                else
                {
                    _root.RemoveFromClassList(UssClassNameConstants.USS_CLASS_SKILL_NODE_SELECTED_GLOW_PEAK);
                }
            }).Every(SELECTED_GLOW_PULSE_INTERVAL_MILLISECONDS);
        }

        /// <summary>
        ///     選択中の淡いパルスグローを停止する。
        /// </summary>
        private void StopSelectedGlow()
        {
            _selectedGlowPulseItem?.Pause();
            _selectedGlowPulseItem = null;
            _root.RemoveFromClassList(UssClassNameConstants.USS_CLASS_SKILL_NODE_SELECTED_GLOW_PEAK);
            _root.RemoveFromClassList(UssClassNameConstants.USS_CLASS_SKILL_NODE_SELECTED_GLOW);
        }

    }
}
