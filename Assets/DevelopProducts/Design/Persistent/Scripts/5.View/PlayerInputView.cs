using DevelopProducts.Persistent.Adaptor;
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
            ButtonInputAdaptor buttonInputAdaptor,
            MoveInputAdaptor moveInputAdaptor)
        {
            _buttonInputAdaptor = buttonInputAdaptor;
            _moveInputAdaptor = moveInputAdaptor;
        }

        public void OnOption(InputAction.CallbackContext context)
        {
            HandleButton(InputActionIds.Option, context);
        }

        public void OnMove(InputAction.CallbackContext context)
        {
            Vector2 move = context.ReadValue<Vector2>();
            InputPheseId pheseId = ConvertPhese(context);

            _moveInputAdaptor.HandleMove(move, pheseId);
        }

        public void OnSubmit(InputAction.CallbackContext context)
        {
            HandleButton(InputActionIds.Submit, context);
        }

        public void OnCancel(InputAction.CallbackContext context)
        {
            HandleButton(InputActionIds.Cancel, context);
        }

        private void HandleButton(InputActionId inputActionId, InputAction.CallbackContext context)
        {
            InputPheseId pheseId = ConvertPhese(context);
            _buttonInputAdaptor.HandleButton(inputActionId, pheseId);
        }

        private static InputPheseId ConvertPhese(InputAction.CallbackContext context)
        {
            if (context.started) return InputPheseIds.Started;
            if (context.performed) return InputPheseIds.Performed;
            if (context.canceled) return InputPheseIds.Canceled;
            return new InputPheseId(0);
        }

        private ButtonInputAdaptor _buttonInputAdaptor;
        private MoveInputAdaptor _moveInputAdaptor;
    }
}