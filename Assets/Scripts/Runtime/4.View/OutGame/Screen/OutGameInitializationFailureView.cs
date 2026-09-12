using System;
using UnityEngine;

namespace KillChord.Runtime.View.OutGame.Screen
{
    /// <summary>
    ///     画面初期化に失敗した際、通常のUIやロード済みアセットに依存せず復帰操作を表示します。
    /// </summary>
    public sealed class OutGameInitializationFailureView : MonoBehaviour
    {
        /// <summary> 復帰ボタンが押された時のイベントです。 </summary>
        public event Action OnRecoveryRequested;

        /// <summary>
        ///     復帰操作の表示名を設定します。
        /// </summary>
        /// <param name="actionLabel"> ボタンに表示する文言です。 </param>
        public void Initialize(string actionLabel)
        {
            _actionLabel = actionLabel;
        }

        /// <summary>
        ///     復帰処理中の入力を無効にします。
        /// </summary>
        /// <param name="isBusy"> 復帰処理中の場合はtrueです。 </param>
        public void SetBusy(bool isBusy)
        {
            _isBusy = isBusy;
        }

        /// <summary>
        ///     復帰にも失敗したことを表示し、再操作を受け付けます。
        /// </summary>
        public void ShowRecoveryFailed()
        {
            _message = "画面を読み込めませんでした。もう一度お試しください。";
            _isBusy = false;
        }

        private const float PANEL_WIDTH = 560f;
        private const float PANEL_HEIGHT = 180f;
        private const float SCREEN_MARGIN = 20f;
        private const float BUTTON_HEIGHT = 48f;
        private const float CONTENT_SPACING = 20f;
        private const int FONT_SIZE = 20;

        private string _message = "画面の読み込みに失敗しました。";
        private string _actionLabel = "タイトルへ戻る";
        private bool _isBusy;

        /// <summary>
        ///     UIDocumentの初期化成否やゲーム内時間に依存しない復帰画面を描画します。
        /// </summary>
        private void OnGUI()
        {
            int previousDepth = GUI.depth;
            Color previousColor = GUI.color;
            bool previousEnabled = GUI.enabled;
            GUI.depth = int.MinValue;

            try
            {
                GUI.color = Color.black;
                GUI.DrawTexture(new Rect(0f, 0f, UnityEngine.Screen.width, UnityEngine.Screen.height), Texture2D.whiteTexture);
                GUI.color = Color.white;

                float width = Mathf.Min(PANEL_WIDTH, UnityEngine.Screen.width - SCREEN_MARGIN * 2f);
                Rect panel = new Rect(
                    (UnityEngine.Screen.width - width) * 0.5f,
                    (UnityEngine.Screen.height - PANEL_HEIGHT) * 0.5f,
                    width,
                    PANEL_HEIGHT);

                GUILayout.BeginArea(panel, GUI.skin.box);
                GUILayout.Label(_message, new GUIStyle(GUI.skin.label)
                {
                    alignment = TextAnchor.MiddleCenter,
                    wordWrap = true,
                    fontSize = FONT_SIZE,
                });
                GUILayout.Space(CONTENT_SPACING);
                GUI.enabled = !_isBusy;
                bool requested = GUILayout.Button(
                    _isBusy ? "読み込み中…" : _actionLabel,
                    GUILayout.Height(BUTTON_HEIGHT));
                GUILayout.EndArea();

                // 通常UIのフォーカスやInputMapが未構築でもキーボードで復帰できます。
                Event currentEvent = Event.current;
                if (!_isBusy && currentEvent.type == EventType.KeyDown
                    && (currentEvent.keyCode == KeyCode.Return || currentEvent.keyCode == KeyCode.KeypadEnter))
                {
                    currentEvent.Use();
                    requested = true;
                }

                if (requested && !_isBusy)
                {
                    OnRecoveryRequested?.Invoke();
                }
            }
            finally
            {
                GUI.depth = previousDepth;
                GUI.color = previousColor;
                GUI.enabled = previousEnabled;
            }
        }
    }
}
