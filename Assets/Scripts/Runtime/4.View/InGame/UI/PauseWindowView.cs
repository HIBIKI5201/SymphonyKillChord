using System;
using UnityEngine;
using UnityEngine.UI;

namespace KillChord.Runtime.View
{
    public class PauseWindowView : MonoBehaviour
    {

        /// <summary> 再開ボタンが押されたときに発火するイベント。 </summary>
        public event Action OnRestartRequested;
        /// <summary> タイトルに戻るボタンが押されたときに発火するイベント。 </summary>
        public event Action OnReturnToTitleRequested;
        /// <summary> ポーズウィンドウが開いているかどうか </summary>
        public bool IsPaused => gameObject.activeSelf;

        public void Show()
        {
            gameObject.SetActive(true);
        }
        public void Hide()
        {
            gameObject.SetActive(false);
        }
        [SerializeField] private Button _restartButton;
        [SerializeField] private Button _returnToTitleButton;
        private void Awake()
        {
            gameObject.SetActive(false);
            _restartButton.onClick.AddListener(() => OnRestartRequested?.Invoke());
            _returnToTitleButton.onClick.AddListener(() => OnReturnToTitleRequested?.Invoke());
        }
    }
}
