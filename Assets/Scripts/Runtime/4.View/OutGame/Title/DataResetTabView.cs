using KillChord.Runtime.View.OutGame.Screen;
using System;
using UnityEngine.UIElements;

namespace KillChord.Runtime.View.OutGame.Title
{
    /// <summary>
    ///     データリセットタブの View クラス。
    /// </summary>
    public sealed class DataResetTabView :  IDisposable
    {
        /// <summary>
        ///     データリセットタブの UI 要素を初期化します。
        /// </summary>
        /// <param name="root"></param>
        /// <param name="outGameUIEvent"></param>
        /// <exception cref="NullReferenceException"></exception>
        public DataResetTabView(VisualElement root, OutGameUIEvent outGameUIEvent)
        {
            _outGameUIEvent = outGameUIEvent;
            _resetButton = root.Q<Button>(RESET_BUTTON_NAME)
                ?? throw new NullReferenceException($"{RESET_BUTTON_NAME} が見つかりません。");
            RegisterEvents();
        }

        /// <summary>
        ///     データリセットタブの View のリソースを解放します。
        /// </summary>
        public void Dispose()
        {
            UnregisterEvents();
        }

        private const string RESET_BUTTON_NAME = "DataResetButton";
        private Button _resetButton;

        private OutGameUIEvent _outGameUIEvent;

        /// <summary>
        ///    データリセットボタンのクリックイベントを登録します。
        /// </summary>
        private void RegisterEvents()
        {
            _resetButton.clicked += OnResetButtonClicked;
        }

        /// <summary>
        ///     データリセットボタンのクリックイベントを解除します。
        /// </summary>
        private void UnregisterEvents()
        {
            _resetButton.clicked -= OnResetButtonClicked;
        }

        /// <summary>
        ///    データリセットボタンがクリックされたときに呼び出されます。
        /// </summary>
        private void OnResetButtonClicked()
        {
            _outGameUIEvent.OnDataResetButtonClicked?.Invoke();
        }
    }
}
