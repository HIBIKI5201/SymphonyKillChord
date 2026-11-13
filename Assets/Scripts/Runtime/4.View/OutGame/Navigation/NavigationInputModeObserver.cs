using System;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

namespace KillChord.Runtime.View.OutGame.Navigation
{
    /// <summary>
    ///     最後に操作された入力機器がポインター(マウス・タッチ・ペン)かを監視し、
    ///     <see cref="NavigationFocusVisibility"/> へ反映します。
    ///     <para>
    ///         操作案内用の InputDeviceKindObserver はマウスとキーボードを同じ種類として扱い、
    ///         タッチを対象外とするため、フォーカス表示の判定には使えません。
    ///         同じ <see cref="InputSystem.onActionChange"/> を使い、分類だけを変えています。
    ///     </para>
    ///     <para>
    ///         UI Toolkit の画面ではタッチやクリックが入力アクションを通らないことがあるため、
    ///         機器のボタン押下(<see cref="InputSystem.onAnyButtonPress"/>)からも判定します。
    ///         入力の更新ごとに uGUI の選択も入力状態へ合わせます。
    ///     </para>
    /// </summary>
    public sealed class NavigationInputModeObserver : IDisposable
    {
        /// <summary>
        ///     ゲームパッドの接続有無から初期状態を決め、入力アクションの監視を開始します。
        /// </summary>
        public NavigationInputModeObserver()
        {
            NavigationFocusVisibility.SetNavigationActive(Gamepad.current != null);
            InputSystem.onActionChange += HandleActionChange;
            InputSystem.onAfterUpdate += HandleAfterUpdate;
            _anyButtonPressSubscription = InputSystem.onAnyButtonPress.Call(HandleAnyButtonPress);
        }

        /// <summary>
        ///     入力アクションの監視を解除し、フォーカス表示を既定の状態へ戻します。
        /// </summary>
        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            InputSystem.onActionChange -= HandleActionChange;
            InputSystem.onAfterUpdate -= HandleAfterUpdate;
            _anyButtonPressSubscription?.Dispose();
            _anyButtonPressSubscription = null;
            NavigationFocusVisibility.SetNavigationActive(true);
            _isDisposed = true;
        }

        private bool _isDisposed;
        private IDisposable _anyButtonPressSubscription;

        /// <summary>
        ///     実行されたアクションの機器からナビゲーション操作中かを判定して反映します。
        /// </summary>
        /// <param name="target"> 変化したアクションまたはアクションマップです。 </param>
        /// <param name="change"> 変化の種類です。 </param>
        private void HandleActionChange(object target, InputActionChange change)
        {
            if (change != InputActionChange.ActionPerformed || target is not InputAction action)
            {
                return;
            }

            ApplyDevice(action.activeControl?.device);
        }

        /// <summary>
        ///     押された機器のボタンからナビゲーション操作中かを判定して反映します。
        /// </summary>
        /// <param name="control"> 押されたボタンです。 </param>
        private void HandleAnyButtonPress(InputControl control)
        {
            ApplyDevice(control?.device);
        }

        /// <summary>
        ///     入力の更新後に、uGUI の選択を入力状態へ合わせます。
        /// </summary>
        private void HandleAfterUpdate()
        {
            NavigationFocusVisibility.UpdateUguiSelection();
        }

        /// <summary>
        ///     操作された機器の種類から、ナビゲーション操作中かを反映します。
        /// </summary>
        /// <param name="device"> 操作された機器です。 </param>
        private static void ApplyDevice(InputDevice device)
        {
            switch (device)
            {
                case Pointer:
                    NavigationFocusVisibility.SetNavigationActive(false);
                    break;
                case Gamepad:
                case Joystick:
                case Keyboard:
                    NavigationFocusVisibility.SetNavigationActive(true);
                    break;
            }
        }
    }
}
