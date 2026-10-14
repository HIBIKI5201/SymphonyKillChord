using System;
using UnityEngine.InputSystem;

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
            NavigationFocusVisibility.SetNavigationActive(true);
            _isDisposed = true;
        }

        private bool _isDisposed;

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

            switch (action.activeControl?.device)
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
