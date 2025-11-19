using System;
using System.Collections.Generic;
using UnityEngine.InputSystem;

namespace Mock.MusicBattle_Mock.Scripts.Basis
{ 
    /// <summary>
    ///     入力アクションのイベントラッパークラス。
    /// </summary>
    /// <typeparam name="T">入力値の型（Vector2, float等の構造体）</typeparam>
    public class InputActionEntity<T> where T : struct
    {
        /// <summary>
        ///     InputActionEntityのコンストラクタ。
        /// </summary>
        /// <param name="inputAction">ラップするUnityのInputAction</param>
        public InputActionEntity(InputAction inputAction)
        {
            _inputAction = inputAction;
        }

        /// <summary>
        ///     入力が開始された時のイベント。
        /// </summary>
        public event Action<T> Started
        {
            add
            {
                // CallbackContextからT型に変換するラッパーハンドラーを作成
                Action<InputAction.CallbackContext> handler = ctx => value(ctx.ReadValue<T>());
                
                // 後でremove時に特定できるよう、元のハンドラーとラッパーを紐付けて保存
                _startedHandlers[value] = handler;
                
                // 実際のInputActionにラッパーハンドラーを登録
                _inputAction.started += handler;
            }
            remove
            {
                // Dictionaryから対応するラッパーハンドラーを取得
                if (_startedHandlers.TryGetValue(value, out var handler))
                {
                    // InputActionからラッパーハンドラーを解除
                    _inputAction.started -= handler;
                    
                    // Dictionaryからも削除してメモリリークを防止
                    _startedHandlers.Remove(value);
                }
            }
        }

        /// <summary>
        ///     入力が実行された時のイベント。
        /// </summary>
        public event Action<T> Performed
        {
            add
            {
                // CallbackContextからT型に変換するラッパーハンドラーを作成
                Action<InputAction.CallbackContext> handler = ctx => value(ctx.ReadValue<T>());
                
                // 後でremove時に特定できるよう、元のハンドラーとラッパーを紐付けて保存
                _performedHandlers[value] = handler;
                
                // 実際のInputActionにラッパーハンドラーを登録
                _inputAction.performed += handler;
            }
            remove
            {
                // Dictionaryから対応するラッパーハンドラーを取得
                if (_performedHandlers.TryGetValue(value, out var handler))
                {
                    // InputActionからラッパーハンドラーを解除
                    _inputAction.performed -= handler;
                    
                    // Dictionaryからも削除してメモリリークを防止
                    _performedHandlers.Remove(value);
                }
            }
        }

        /// <summary>
        ///     入力がキャンセルされた時のイベント。
        /// </summary>
        public event Action<T> Canceled
        {
            add
            {
                // CallbackContextからT型に変換するラッパーハンドラーを作成
                Action<InputAction.CallbackContext> handler = ctx => value(ctx.ReadValue<T>());
                
                // 後でremove時に特定できるよう、元のハンドラーとラッパーを紐付けて保存
                _canceledHandlers[value] = handler;
                
                // 実際のInputActionにラッパーハンドラーを登録
                _inputAction.canceled += handler;
            }
            remove
            {
                // Dictionaryから対応するラッパーハンドラーを取得
                if (_canceledHandlers.TryGetValue(value, out var handler))
                {
                    // InputActionからラッパーハンドラーを解除
                    _inputAction.canceled -= handler;
                    
                    // Dictionaryからも削除してメモリリークを防止
                    _canceledHandlers.Remove(value);
                }
            }
        }

        /// <summary>
        ///     登録されている全てのStartedイベントハンドラーを手動で呼び出します。
        /// </summary>
        /// <param name="value">イベントハンドラーに渡す値</param>
        public void InvokeStarted(T value)
        {
            foreach (var kvp in _startedHandlers)
            {
                kvp.Key.Invoke(value);
            }
        }

        /// <summary>
        ///     登録されている全てのPerformedイベントハンドラーを手動で呼び出します。
        /// </summary>
        /// <param name="value">イベントハンドラーに渡す値</param>
        public void InvokePerfomed(T value)
        {
            foreach (var kvp in _performedHandlers)
            {
                kvp.Key.Invoke(value);
            }
        }

        /// <summary>
        ///     登録されている全てのCanceledイベントハンドラーを手動で呼び出します。
        /// </summary>
        /// <param name="value">イベントハンドラーに渡す値</param>
        public void InvokeCanceled(T value)
        {
            foreach (var kvp in _canceledHandlers)
            {
                kvp.Key.Invoke(value);
            }
        }

        // ラップ対象のUnity InputAction
        private readonly InputAction _inputAction;

        // 各イベントごとにラムダを保持するDictionary
        // キー: 外部から登録されたAction<T>ハンドラー
        // 値: CallbackContextをT型に変換するラッパーハンドラー
        private readonly Dictionary<Action<T>, Action<InputAction.CallbackContext>> _startedHandlers = new();
        private readonly Dictionary<Action<T>, Action<InputAction.CallbackContext>> _performedHandlers = new();
        private readonly Dictionary<Action<T>, Action<InputAction.CallbackContext>> _canceledHandlers = new();
    }
}