using System;
using UnityEngine.InputSystem;

namespace Mock.MusicBattle.Basis
{
    /// <summary>
    ///     入力アクションのイベントラッパークラス。
    /// </summary>
    /// <typeparam name="T">入力値の型（Vector2, float等の構造体）。</typeparam>
    public class InputActionEntity<T> : IDisposable
        where T : struct
    {
        /// <summary>
        ///     <see cref="InputActionEntity{T}"/>の新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="inputAction">ラップするUnityのInputAction。</param>
        public InputActionEntity(InputAction inputAction)
        {
            _inputAction = inputAction;
            inputAction.started += StartedHandler;
            inputAction.performed += PerformedHandler;
            inputAction.canceled += CanceledHandler;
        }

        #region Publicイベント
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
        #endregion

        // PUBLIC_PROPERTIES
        // INTERFACE_PROPERTIES
        // PUBLIC_CONSTANTS
        #region Publicメソッド
        /// <summary>
        ///     登録されている全てのStartedイベントハンドラーを手動で呼び出します。
        /// </summary>
        /// <param name="value">イベントハンドラーに渡す値。</param>
        public void InvokeStarted(T value)
        {
            Started?.Invoke(value);
        }

        /// <summary>
        ///     登録されている全てのPerformedイベントハンドラーを手動で呼び出します。
        /// </summary>
        /// <param name="value">イベントハンドラーに渡す値。</param>
        public void InvokePerformed(T value)
        {
            Performed?.Invoke(value);
        }

        /// <summary>
        ///     登録されている全てのCanceledイベントハンドラーを手動で呼び出します。
        /// </summary>
        /// <param name="value">イベントハンドラーに渡す値。</param>
        public void InvokeCanceled(T value)
        {
            Canceled?.Invoke(value);
        }
        #endregion

        #region パブリックインターフェースメソッド
        /// <summary>
        ///     このインスタンスによって使用されているリソースを解放します。
        /// </summary>
        public void Dispose()
        {
            _inputAction.started -= StartedHandler;
            _inputAction.performed -= PerformedHandler;
            _inputAction.canceled -= CanceledHandler;
        }
        #endregion

        // PUBLIC_ENUM_DEFINITIONS
        // PUBLIC_CLASS_DEFINITIONS
        // PUBLIC_STRUCT_DEFINITIONS
        // CONSTANTS
        // INSPECTOR_FIELDS
        #region プライベートフィールド
        /// <summary> ラップ対象のUnity InputAction。 </summary>
        private readonly InputAction _inputAction;
        #endregion

        // UNITY_LIFECYCLE_METHODS
        #region イベントハンドラメソッド
        /// <summary>
        ///     InputActionのstartedイベントのハンドラー。
        /// </summary>
        /// <param name="ctx">コールバックコンテキスト。</param>
        private void StartedHandler(InputAction.CallbackContext ctx)
        {
            T value = ctx.ReadValue<T>();
            InvokeStarted(value);
        }

        /// <summary>
        ///     InputActionのperformedイベントのハンドラー。
        /// </summary>
        /// <param name="ctx">コールバックコンテキスト。</param>
        private void PerformedHandler(InputAction.CallbackContext ctx)
        {
            T value = ctx.ReadValue<T>();
            InvokePerformed(value);
        }

        /// <summary>
        ///     InputActionのcanceledイベントのハンドラー。
        /// </summary>
        /// <param name="ctx">コールバックコンテキスト。</param>
        private void CanceledHandler(InputAction.CallbackContext ctx)
        {
            T value = ctx.ReadValue<T>();
            InvokeCanceled(value);
        }
        #endregion

        // PROTECTED_INTERFACE_VIRTUAL_METHODS
        // PRIVATE_METHODS
        // PRIVATE_ENUM_DEFINITIONS
        // PRIVATE_CLASS_DEFINITIONS
        // PRIVATE_STRUCT_DEFINITIONS
    }
}