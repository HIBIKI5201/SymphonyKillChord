using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace DevelopProducts.BindingSystem
{
    /// <summary>
    ///     キーボード・マウスの割り当てを変更する設定項目。
    ///     1つのアクション・1つのバインディングに対応する。
    /// </summary>
    [Serializable]
    public class KeyRebindSetting : BindingSettingBase
    {
        [Header("対象アクション")]
        [SerializeField] private InputActionReference _actionReference;
        [SerializeField] private int _bindingIndex;

        [Header("UI")]
        [SerializeField] private TextMeshProUGUI _bindingText;
        [SerializeField] private Button _rebindButton;

        private InputActionRebindingExtensions.RebindingOperation _rebindOp;

        private InputAction Action => _actionReference.action;

        public override void Initialize(PauseMenuPanelView pauseMenu)
        {
            base.Initialize(pauseMenu);
            _rebindButton.onClick.AddListener(StartRebind);
            RefreshText();
        }

        public override void Load() => RefreshText();

        public override void Apply()
        {
            // 再割り当てはボタン操作で即時に反映されるため、ここでは何もしない。
        }

        public override void ResetToDefault()
        {
            Action.RemoveBindingOverride(_bindingIndex);
            RefreshText();
        }

        private void StartRebind()
        {
            _bindingText.text = "入力待ち...";
            Action.Disable();
            _rebindButton.interactable = false;

            _rebindOp = Action.PerformInteractiveRebinding(_bindingIndex)
                .WithControlsHavingToMatchPath("<Keyboard>")
                .WithControlsHavingToMatchPath("<Mouse>")
                .WithExpectedControlType("Button")
                .WithCancelingThrough("<Keyboard>/escape")
                .OnComplete(_ => FinishRebind())
                .OnCancel(_ => FinishRebind())
                .Start();
        }

        private void FinishRebind()
        {
            _rebindOp?.Dispose();
            _rebindOp = null;
            Action.Enable();
            _rebindButton.interactable = true;
            RefreshText();
        }

        private void RefreshText()
            => _bindingText.text = InputControlPath.ToHumanReadableString(Action.bindings[_bindingIndex].effectivePath);
    }
}
