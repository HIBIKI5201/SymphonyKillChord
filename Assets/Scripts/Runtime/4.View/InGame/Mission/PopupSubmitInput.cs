using KillChord.Runtime.View.Persistent.Input;
using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace KillChord.Runtime.View.InGame.Mission
{
    /// <summary>
    ///     UIアクションマップの決定入力を、説明ポップアップを閉じる入力として通知するViewクラス。
    ///     UIマップはインゲーム中も有効なため、InGameマップを切り替えずに決定ボタンを読み取れます。
    /// </summary>
    public sealed class PopupSubmitInput : IDisposable
    {
        /// <summary>
        ///     PlayerInputのUIマップから決定入力を取得して購読します。
        /// </summary>
        /// <param name="playerInput"> 入力アクションを持つPlayerInputです。 </param>
        public PopupSubmitInput(PlayerInput playerInput)
        {
            if (playerInput == null)
            {
                throw new ArgumentNullException(nameof(playerInput));
            }

            _submitAction = playerInput.actions.FindAction($"{InputMapNames.UI}/{SUBMIT_ACTION_NAME}", true);
            _submitAction.performed += HandleSubmitPerformed;
        }

        /// <summary> 決定入力が押されたことを通知するイベント。 </summary>
        public event Action OnSubmitted;

        /// <summary>
        ///     決定入力の購読を解除します。
        /// </summary>
        public void Dispose()
        {
            _submitAction.performed -= HandleSubmitPerformed;
            OnSubmitted = null;
        }

        private const string SUBMIT_ACTION_NAME = "Submit";

        private readonly InputAction _submitAction;

        /// <summary>
        ///     決定入力を通知する。
        /// </summary>
        /// <param name="context"> Input System のコールバックコンテキスト。 </param>
        private void HandleSubmitPerformed(InputAction.CallbackContext context)
        {
            // ポーズ中のメニュー決定でポップアップが閉じないよう、時間停止中は通知しない。
            if (Time.timeScale <= 0f)
            {
                return;
            }

            OnSubmitted?.Invoke();
        }
    }
}
