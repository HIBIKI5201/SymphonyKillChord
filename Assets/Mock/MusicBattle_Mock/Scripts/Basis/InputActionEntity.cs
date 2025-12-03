using System;
using UnityEngine.InputSystem;

namespace Mock.MusicBattle.Basis
{
    /// <summary>
    ///     入力アクションのイベントラッパークラス。
    /// </summary>
    /// <typeparam name="T">　入力値の型（Vector2, float等の構造体）</typeparam>
    public class InputActionEntity<T> : IDisposable
        where T : struct
    {
        /// <summary>
        ///     InputActionEntityのコンストラクタ。
        /// </summary>
        /// <param name="inputAction"> ラップするUnityのInputAction</param>
        public InputActionEntity(InputAction inputAction)
        {
            _inputAction = inputAction;
            inputAction.started += StartedHandler;
            inputAction.performed += PerformedHandler;
            inputAction.canceled += CanceledHandler;
        }

        /// <summary>
        ///     入力が開始された時のイベント。
        /// </summary>
        public event Action<T> Started;

        /// <summary>
        ///     入力が実行された時のイベント。
        /// </summary>
        public event Action<T> Performed;
        /// <summary>
        ///     入力がキャンセルされた時のイベント。
        /// </summary>
        public event Action<T> Canceled;
        /// <summary>
        ///     登録されている全てのStartedイベントハンドラーを手動で呼び出します。
        /// </summary>
        /// <param name="value">イベントハンドラーに渡す値</param>
        public void InvokeStarted(T value)
        {
            Started?.Invoke(value);
        }

        /// <summary>
        ///     登録されている全てのPerformedイベントハンドラーを手動で呼び出します。
        /// </summary>
        /// <param name="value">イベントハンドラーに渡す値</param>
        public void InvokePerformed(T value)
        {
            Performed?.Invoke(value);
        }
        /// <summary>
        ///     登録されている全てのCanceledイベントハンドラーを手動で呼び出します。
        /// </summary>
        /// <param name="value">イベントハンドラーに渡す値</param>
        public void InvokeCanceled(T value)
        {
            Canceled?.Invoke(value);
        }

        public void Dispose()
        {
            _inputAction.started -= StartedHandler;
            _inputAction.performed -= PerformedHandler;
            _inputAction.canceled -= CanceledHandler;
        }

        // ラップ対象のUnity InputAction。
        private readonly InputAction _inputAction;

        private void StartedHandler(InputAction.CallbackContext ctx)
        {
            T value = ctx.ReadValue<T>();
            InvokeStarted(value);
        }

        private void PerformedHandler(InputAction.CallbackContext ctx)
        {
            T value = ctx.ReadValue<T>();
            InvokePerformed(value);
        }

        private void CanceledHandler(InputAction.CallbackContext ctx)
        {
            T value = ctx.ReadValue<T>();
            InvokeCanceled(value);
        }
    }
}