using KillChord.Runtime.Adaptor;
using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace KillChord.Runtime.View
{
    /// <summary>
    ///     入力イベントを受け取り、InputContextを生成して外部に通知するクラス。
    /// </summary>
    public class PlayerInputView
    {
        public PlayerInputView(InputTimestampProvider timestampProvider)
        {
            _timestampProvider = timestampProvider;
        }

        // イベント群。
        public event Action<InputContext<float>> OnOptionInput;
        public event Action<InputContext<float>> OnSubmitInput;
        public event Action<InputContext<float>> OnCancelInput;
        public event Action<InputContext<float>> OnDodgeInput;
        public event Action<InputContext<float>> OnAttackInput;
        public event Action<InputContext<Vector2>> OnMoveInput;

        public void OnOption(InputAction.CallbackContext context)
        {
            float time = _timestampProvider.GetCurrentTimestamp();
            InputContext<float> inputContext = new InputContext<float>(
                InputActionKind.Option, context, time);
            OnOptionInput?.Invoke(inputContext);
        }

        public void OnSubmit(InputAction.CallbackContext context)
        {
            float time = _timestampProvider.GetCurrentTimestamp();
            InputContext<float> inputContext = new InputContext<float>(
                InputActionKind.Submit, context, time);
            OnSubmitInput?.Invoke(inputContext);
        }

        public void OnCancel(InputAction.CallbackContext context)
        {
            float time = _timestampProvider.GetCurrentTimestamp();
            InputContext<float> inputContext = new InputContext<float>(
                InputActionKind.Cancel, context, time);
            OnCancelInput?.Invoke(inputContext);
        }

        public void OnDodge(InputAction.CallbackContext context)
        {
            float time = _timestampProvider.GetCurrentTimestamp();
            InputContext<float> inputContext = new InputContext<float>(
                InputActionKind.Dodge, context, time);
            OnDodgeInput?.Invoke(inputContext);
        }

        public void OnAttack(InputAction.CallbackContext context)
        {
            float time = _timestampProvider.GetCurrentTimestamp();
            InputContext<float> inputContext = new InputContext<float>(
                InputActionKind.Attack, context, time);
            OnAttackInput?.Invoke(inputContext);
        }

        public void OnMove(InputAction.CallbackContext context)
        {
            float time = _timestampProvider.GetCurrentTimestamp();
            InputContext<Vector2> inputContext = new InputContext<Vector2>(
                InputActionKind.Move, context, time);
            OnMoveInput?.Invoke(inputContext);
        }

        public void OnMobileButton(InputActionKind actionId, InputActionPhase phase, float value)
        {
            if (actionId == InputActionKind.Move)
            {
                Debug.LogError("Move action should not be handled by OnMobileButton. Use OnMobileMove instead.");
                return;
            }

            float time = _timestampProvider.GetCurrentTimestamp();
            InputContext<float> inputContext = new InputContext<float>(
                actionId, value, phase, time);

            Action<InputContext<float>> action = actionId switch
            {
                InputActionKind.Option => OnOptionInput,
                InputActionKind.Submit => OnSubmitInput,
                InputActionKind.Cancel => OnCancelInput,
                InputActionKind.Dodge => OnDodgeInput,
                InputActionKind.Attack => OnAttackInput,
                _ => throw new ArgumentOutOfRangeException(nameof(actionId), $"Unexpected actionId: {actionId}")
            };
            action?.Invoke(inputContext);
        }

        public void OnMobileMove(InputActionPhase phase, Vector2 value)
        {
            float time = _timestampProvider.GetCurrentTimestamp();
            InputContext<Vector2> inputContext = new InputContext<Vector2>(
                InputActionKind.Move, value, phase, time);
            OnMoveInput?.Invoke(inputContext);
        }

        private readonly InputTimestampProvider _timestampProvider;
    }
}
