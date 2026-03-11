using DevelopProducts.Persistent.Application;
using DevelopProducts.Persistent.Domain.Input;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DevelopProducts.Persistent.View
{
    /// <summary>
    ///     プレイヤーの入力を処理するクラス。
    /// </summary>
    public class PlayerInputView : MonoBehaviour
    {
        public void Initialize(
            BufferButtonInputUsecase bufferButtonInputUsecase,
            BufferMoveInputUsecase bufferInputActionUsecase,
            InputTimestampProvider timestampProvider)
        {
            _bufferButtonInputUsecase = bufferButtonInputUsecase;
            _bufferInputActionUsecase = bufferInputActionUsecase;
            _timestampProvider = timestampProvider;
        }

        public void OnOption(InputAction.CallbackContext context)
        {
            HandleButton(InputActionIds.Option, context);
        }

        public void OnMove(InputAction.CallbackContext context)
        {
            Vector2 move = context.ReadValue<Vector2>();
            InputPheseId pheseId = ConvertPhese(context);

            float timestamp = _timestampProvider.GetTimestamp();
            _bufferInputActionUsecase.Execute(move.x, move.y, pheseId, timestamp);
        }

        public void OnSubmit(InputAction.CallbackContext context)
        {
            HandleButton(InputActionIds.Submit, context);
        }

        public void OnCancel(InputAction.CallbackContext context)
        {
            HandleButton(InputActionIds.Cancel, context);
        }

        private static InputPheseId ConvertPhese(InputAction.CallbackContext context)
        {
            if (context.started) return InputPheseIds.Started;
            if (context.performed) return InputPheseIds.Performed;
            if (context.canceled) return InputPheseIds.Canceled;
            return new InputPheseId(0);
        }

        private void HandleButton(InputActionId inputActionId, InputAction.CallbackContext context)
        {
            InputPheseId pheseId = ConvertPhese(context);
            float timestamp = _timestampProvider.GetTimestamp();
            _bufferButtonInputUsecase.Execute(inputActionId, pheseId, timestamp);
        }

        private BufferButtonInputUsecase _bufferButtonInputUsecase;
        private BufferMoveInputUsecase _bufferInputActionUsecase;
        private InputTimestampProvider _timestampProvider;
    }
}