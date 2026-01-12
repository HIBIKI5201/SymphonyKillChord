using CriWare;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Mock.MusicBattle.Basis
{
    [RequireComponent(typeof(Canvas))]
    public class ExplanationManager : MonoBehaviour
    {
        private Canvas _canvas;
        private bool _isShowing = false;

        private void Awake()
        {
            _canvas = GetComponent<Canvas>();
        }

        private void Start()
        {
            Hide();
        }

        private void Update()
        {
            bool isPressed = Keyboard.current.tabKey.isPressed;
            bool isShowing = _isShowing;

            if (isShowing != isPressed)
            {
                if (isShowing)
                {
                    Hide();
                }
                else
                {
                    Show();
                }
            }
        }

        private void Show()
        {
            _canvas.enabled = true;
            _isShowing = true;
        }

        private void Hide()
        {
            _canvas.enabled = false;
            _isShowing = false;
        }
    }
}
