using System;
using DevelopProducts.Persistent.Adaptor;
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

        public void Initialize(MoveInputAdaptor moveInput)
        {
            if (moveInput == null) throw new ArgumentException(nameof(moveInput));
            _moveInputAdaptor = moveInput;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            _isPressed = true;
            _moveInputAdaptor.HandleMove(_direction, InputPheseIds.Started);
            Debug.Log($"MobileMoveButtonView: OnPointerDown, Direction: {_direction}");
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _isPressed = false;
            _moveInputAdaptor.Release();
        }

        [SerializeField] private Vector2 _direction;

        private MoveInputAdaptor _moveInputAdaptor;
        private bool _isPressed;
    }
}
