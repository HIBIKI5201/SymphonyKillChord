using KillChord.DebugWorld.Common;
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace KillChord.DebugWorld.Title
{
    /// <summary>
    ///     タイトル画面に裏世界への入口ボタンを出し、押されたらハブシーンへ移るクラスです。
    ///     <para> PCではショートカットキーでも開けます。 </para>
    /// </summary>
    public sealed class DebugWorldTitleEntry : MonoBehaviour
    {
        private const string TITLE_CONTAINER_NAME = "TitleContainer";
        private const string BUTTON_NAME = "debug-world-entry-button";
        private const string BUTTON_TEXT = "裏世界";
        private const float BUTTON_MARGIN = 24f;
        private const float BUTTON_PADDING = 12f;
        private const int BUTTON_FONT_SIZE = 28;
        private static readonly Color BUTTON_BACKGROUND_COLOR = new(0.1f, 0.1f, 0.1f, 0.75f);
        private static readonly Key SHORTCUT_KEY = Key.F12;

        private Button _entryButton;
        private bool _isOpening;

        private void Start()
        {
            VisualElement titleContainer = FindTitleContainer();
            if (titleContainer == null)
            {
                Debug.LogWarning(
                    $"[{nameof(DebugWorldTitleEntry)}] {TITLE_CONTAINER_NAME}が見つからないため、入口ボタンを出しません。"
                    + $" {SHORTCUT_KEY}キーでは開けます。",
                    this);
                return;
            }

            _entryButton = CreateEntryButton();
            titleContainer.Add(_entryButton);
        }

        private void Update()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard != null && keyboard[SHORTCUT_KEY].wasPressedThisFrame)
            {
                OpenHub();
            }
        }

        private void OnDestroy()
        {
            if (_entryButton == null)
            {
                return;
            }

            _entryButton.clicked -= OpenHub;
            _entryButton.UnregisterCallback<PointerDownEvent>(HandlePointerDown);
            _entryButton.RemoveFromHierarchy();
            _entryButton = null;
        }

        /// <summary>
        ///     ボタンを押したときに、背後のタイトル画面へ押下を伝えないようにします。
        /// </summary>
        /// <param name="evt"> 押下イベントです。 </param>
        private void HandlePointerDown(PointerDownEvent evt)
        {
            evt.StopPropagation();
        }

        /// <summary>
        ///     同じシーンにあるUIDocumentから、タイトル画面のルートを探します。
        /// </summary>
        /// <returns> タイトル画面のルートです。見つからない場合はnullです。 </returns>
        private VisualElement FindTitleContainer()
        {
            UIDocument[] documents = FindObjectsByType<UIDocument>(FindObjectsSortMode.None);
            foreach (UIDocument document in documents)
            {
                if (document.gameObject.scene != gameObject.scene || document.rootVisualElement == null)
                {
                    continue;
                }

                VisualElement container = document.rootVisualElement.Q<VisualElement>(TITLE_CONTAINER_NAME);
                if (container != null)
                {
                    return container;
                }
            }

            return null;
        }

        /// <summary>
        ///     画面右上に置く入口ボタンを作ります。
        /// </summary>
        /// <returns> 作ったボタンです。 </returns>
        private Button CreateEntryButton()
        {
            Button button = new(OpenHub)
            {
                name = BUTTON_NAME,
                text = BUTTON_TEXT,
            };
            button.style.position = Position.Absolute;
            button.style.top = BUTTON_MARGIN;
            button.style.right = BUTTON_MARGIN;
            button.style.paddingLeft = BUTTON_PADDING;
            button.style.paddingRight = BUTTON_PADDING;
            button.style.paddingTop = BUTTON_PADDING;
            button.style.paddingBottom = BUTTON_PADDING;
            button.style.fontSize = BUTTON_FONT_SIZE;
            button.style.color = Color.white;
            button.style.backgroundColor = BUTTON_BACKGROUND_COLOR;
            button.RegisterCallback<PointerDownEvent>(HandlePointerDown);
            return button;
        }

        /// <summary>
        ///     タイトルシーンを閉じてハブシーンを開きます。
        /// </summary>
        private async void OpenHub()
        {
            if (_isOpening)
            {
                return;
            }

            _isOpening = true;
            try
            {
                bool isSuccess = await DebugWorldSceneChanger.ChangeSceneAsync(
                    gameObject.scene.name,
                    DebugWorldSceneNames.HUB);
                if (!isSuccess && this != null)
                {
                    _isOpening = false;
                }
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                if (this != null)
                {
                    _isOpening = false;
                }
            }
        }
    }
}
