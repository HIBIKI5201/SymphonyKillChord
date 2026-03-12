using DevelopProducts.Persistent.Application;
using DevelopProducts.Persistent.Domain.Input;
using UnityEngine;
using UnityEngine.EventSystems;

namespace DevelopProducts.Persistent.View
{
    /// <summary>
    ///     UI Buttonにアタッチして使用する、モバイル向けの移動入力ボタンのView。
    ///     ボタンが押されたときに、BufferMoveInputUsecaseを呼び出して入力をバッファに記録する。
    /// </summary>
    public class MobileMoveButtonView : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        public bool IsPressed => _isPressed;
        public Vector2 Direction => _direction;

        public void Initialize(
            BufferMoveInputUsecase moveInputUseCase,
            InputTimestampProvider timestampProvider)
        {
            _moveInputUseCase = moveInputUseCase;
            _timestampProvider = timestampProvider;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            _isPressed = true;

            _moveInputUseCase.Execute(
                _direction.x,
                _direction.y,
                InputPheseIds.Performed,
                _timestampProvider.GetTimestamp());

            Debug.Log($"MobileMoveButtonView: OnPointerDown - Direction: {_direction}, Timestamp: {_timestampProvider.GetTimestamp()}");
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _isPressed = false;

            _moveInputUseCase.Execute(
                0f,
                0f,
                InputPheseIds.Canceled,
                _timestampProvider.GetTimestamp());
        }

        [SerializeField] private Vector2 _direction;

        private BufferMoveInputUsecase _moveInputUseCase;
        private InputTimestampProvider _timestampProvider;
        private bool _isPressed;
    }
}
