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
            BufferInputActionUsecase bufferInputActionUsecase,
            InputTimestampProvider timestampProvider)
        {
            _bufferButtonInputUsecase = bufferButtonInputUsecase;
            _bufferInputActionUsecase = bufferInputActionUsecase;
            _timestampProvider = timestampProvider;
        }

        public void OnMove(InputAction.CallbackContext context)
        {
            InputActionId inputActionId = new InputActionId(1);

            InputPheseId pheseId = ConvertPhese(context);
            float timestamp = _timestampProvider.GetTimestamp();
            _bufferInputActionUsecase.Execute(inputActionId, pheseId, timestamp);
        }

        private static InputPheseId ConvertPhese(InputAction.CallbackContext context)
        {
            if (context.started) return InputPheseIds.Started;
            if (context.performed) return InputPheseIds.Performed;
            if (context.canceled) return InputPheseIds.Canceled;
            return new InputPheseId(0);
        }

        private BufferButtonInputUsecase _bufferButtonInputUsecase;
        private BufferInputActionUsecase _bufferInputActionUsecase;
        private InputTimestampProvider _timestampProvider;
    }
}