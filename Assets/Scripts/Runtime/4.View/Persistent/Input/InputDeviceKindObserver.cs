using System;
using UnityEngine.InputSystem;

namespace KillChord.Runtime.View.Persistent.Input
{
    /// <summary>
    ///     最後に操作された入力機器の種類を監視し、変化を通知するクラスです。
    /// </summary>
    public sealed class InputDeviceKindObserver : IDisposable
    {
        /// <summary>
        ///     接続中の機器から初期の種類を決め、入力アクションの監視を開始します。
        /// </summary>
        public InputDeviceKindObserver()
        {
            // 入力前はゲームパッドの接続有無で決め、最初の案内を実際の機器に近づける。
            Gamepad gamepad = Gamepad.current;
            _currentKind = gamepad != null ? ClassifyGamepad(gamepad) : InputDeviceKind.Keyboard;
            InputSystem.onActionChange += HandleActionChange;
        }

        /// <summary> 入力機器の種類が変わったときに通知するイベントです。 </summary>
        public event Action<InputDeviceKind> OnKindChanged;

        /// <summary> 最後に操作された入力機器の種類です。 </summary>
        public InputDeviceKind CurrentKind => _currentKind;

        /// <summary>
        ///     入力アクションの監視を解除します。
        /// </summary>
        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            InputSystem.onActionChange -= HandleActionChange;
            OnKindChanged = null;
            _isDisposed = true;
        }

        private const string DUAL_SHOCK_LAYOUT = "DualShockGamepad";
        private const string SWITCH_PRO_LAYOUT = "SwitchProControllerHID";

        private InputDeviceKind _currentKind;
        private bool _isDisposed;

        /// <summary>
        ///     実行されたアクションの機器から種類を判定し、変わっていれば通知します。
        /// </summary>
        /// <param name="target"> 変化したアクションまたはアクションマップです。 </param>
        /// <param name="change"> 変化の種類です。 </param>
        private void HandleActionChange(object target, InputActionChange change)
        {
            if (change != InputActionChange.ActionPerformed || target is not InputAction action)
            {
                return;
            }

            // タッチなど案内の対象外の機器では、直前の表示を保つ。
            if (!TryClassify(action.activeControl?.device, out InputDeviceKind kind) || kind == _currentKind)
            {
                return;
            }

            _currentKind = kind;
            OnKindChanged?.Invoke(kind);
        }

        /// <summary>
        ///     入力機器を操作案内の種類へ分類します。
        /// </summary>
        /// <param name="device"> 分類する入力機器です。 </param>
        /// <param name="kind"> 分類した種類です。 </param>
        /// <returns> 操作案内の対象の機器であればtrueです。 </returns>
        private static bool TryClassify(InputDevice device, out InputDeviceKind kind)
        {
            switch (device)
            {
                case Keyboard:
                case Mouse:
                    kind = InputDeviceKind.Keyboard;
                    return true;
                case Gamepad gamepad:
                    kind = ClassifyGamepad(gamepad);
                    return true;
                default:
                    kind = default;
                    return false;
            }
        }

        /// <summary>
        ///     ゲームパッドのレイアウトから種類を判定します。
        /// </summary>
        /// <param name="gamepad"> 判定するゲームパッドです。 </param>
        /// <returns> 判定した種類です。判別できない場合はXboxです。 </returns>
        private static InputDeviceKind ClassifyGamepad(Gamepad gamepad)
        {
            // 対応プラットフォームが限られる型を直接参照しないよう、レイアウト名で判定する。
            if (InputSystem.IsFirstLayoutBasedOnSecond(gamepad.layout, DUAL_SHOCK_LAYOUT))
            {
                return InputDeviceKind.PlayStation;
            }

            if (InputSystem.IsFirstLayoutBasedOnSecond(gamepad.layout, SWITCH_PRO_LAYOUT))
            {
                return InputDeviceKind.Switch;
            }

            return InputDeviceKind.Xbox;
        }
    }
}
