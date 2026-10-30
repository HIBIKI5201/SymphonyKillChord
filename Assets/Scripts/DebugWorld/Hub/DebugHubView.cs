using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace KillChord.DebugWorld.Hub
{
    /// <summary>
    ///     裏世界のハブ画面を組み立てて、選んだテストを通知するViewクラスです。
    /// </summary>
    public sealed class DebugHubView : IDisposable
    {
        /// <summary>
        ///     ルート要素の下にハブ画面を組み立てます。
        /// </summary>
        /// <param name="rootElement"> ハブ画面を置くルート要素です。 </param>
        public DebugHubView(VisualElement rootElement)
        {
            _rootElement = rootElement ?? throw new ArgumentNullException(nameof(rootElement));
            _container = CreateContainer();
            _container.Add(CreateHeader());
            _container.Add(CreateUnavailableButton(PLAYER_CONTROL_TEST_TEXT));
            _container.Add(CreateUnavailableButton(ENEMY_TEST_TEXT));
            _container.Add(CreateUnavailableButton(BOSS_TEST_TEXT));
            _returnButton = CreateButton(RETURN_TEXT, HandleReturnClicked);
            _container.Add(_returnButton);
            _rootElement.Add(_container);
            _returnButton.Focus();
        }

        /// <summary> タイトルへ戻るボタンが押されたときに通知します。 </summary>
        public event Action OnReturnRequested;

        /// <summary>
        ///     ボタンを押せなくして、二重に遷移しないようにします。
        /// </summary>
        public void Lock()
        {
            _container.SetEnabled(false);
        }

        /// <summary>
        ///     ボタンを押せる状態に戻します。
        /// </summary>
        public void Unlock()
        {
            _container.SetEnabled(true);
        }

        /// <summary>
        ///     組み立てた画面を取り除きます。
        /// </summary>
        public void Dispose()
        {
            _returnButton.clicked -= HandleReturnClicked;
            _container.RemoveFromHierarchy();
            OnReturnRequested = null;
        }

        private const string HEADER_TEXT = "裏世界";
        private const string PLAYER_CONTROL_TEST_TEXT = "プレイヤー操作テスト";
        private const string ENEMY_TEST_TEXT = "敵テスト";
        private const string BOSS_TEST_TEXT = "ボステスト";
        private const string UNAVAILABLE_SUFFIX = "（準備中）";
        private const string RETURN_TEXT = "タイトルへ戻る";
        private const int HEADER_FONT_SIZE = 56;
        private const int BUTTON_FONT_SIZE = 32;
        private const float BUTTON_WIDTH = 560f;
        private const float BUTTON_MARGIN = 12f;
        private const float BUTTON_PADDING = 16f;
        private const float HEADER_MARGIN_BOTTOM = 32f;
        private static readonly Color BACKGROUND_COLOR = new(0.05f, 0.05f, 0.08f, 1f);

        private readonly VisualElement _rootElement;
        private readonly VisualElement _container;
        private readonly Button _returnButton;

        /// <summary>
        ///     タイトルへ戻るボタンが押されたことを通知します。
        /// </summary>
        private void HandleReturnClicked()
        {
            OnReturnRequested?.Invoke();
        }

        /// <summary>
        ///     画面全体を覆い、中身を中央に並べる要素を作ります。
        /// </summary>
        /// <returns> 作った要素です。 </returns>
        private static VisualElement CreateContainer()
        {
            VisualElement container = new();
            container.style.position = Position.Absolute;
            container.style.left = 0f;
            container.style.right = 0f;
            container.style.top = 0f;
            container.style.bottom = 0f;
            container.style.alignItems = Align.Center;
            container.style.justifyContent = Justify.Center;
            container.style.backgroundColor = BACKGROUND_COLOR;
            return container;
        }

        /// <summary>
        ///     画面の見出しを作ります。
        /// </summary>
        /// <returns> 作った見出しです。 </returns>
        private static Label CreateHeader()
        {
            Label header = new(HEADER_TEXT);
            header.style.fontSize = HEADER_FONT_SIZE;
            header.style.color = Color.white;
            header.style.marginBottom = HEADER_MARGIN_BOTTOM;
            return header;
        }

        /// <summary>
        ///     まだ使えないテストのボタンを、押せない状態で作ります。
        /// </summary>
        /// <param name="text"> ボタンの文言です。 </param>
        /// <returns> 作ったボタンです。 </returns>
        private static Button CreateUnavailableButton(string text)
        {
            Button button = CreateButton(text + UNAVAILABLE_SUFFIX, null);
            button.SetEnabled(false);
            return button;
        }

        /// <summary>
        ///     ハブ画面で使う大きさのボタンを作ります。
        /// </summary>
        /// <param name="text"> ボタンの文言です。 </param>
        /// <param name="clicked"> 押されたときの処理です。nullの場合は何もしません。 </param>
        /// <returns> 作ったボタンです。 </returns>
        private static Button CreateButton(string text, Action clicked)
        {
            Button button = new(clicked) { text = text };
            button.style.width = BUTTON_WIDTH;
            button.style.marginTop = BUTTON_MARGIN;
            button.style.marginBottom = BUTTON_MARGIN;
            button.style.paddingTop = BUTTON_PADDING;
            button.style.paddingBottom = BUTTON_PADDING;
            button.style.fontSize = BUTTON_FONT_SIZE;
            return button;
        }
    }
}
