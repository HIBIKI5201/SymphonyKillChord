using KillChord.Runtime.View.Persistent.Localization;
using System;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Settings;

namespace KillChord.Runtime.View.Persistent.Load
{
    /// <summary>
    ///     操作を保留する中央通知を、非スケール時間で表示してからフェードアウトします。
    /// </summary>
    public sealed class EventNotificationView : MonoBehaviour
    {
        /// <summary> 通知による入力抑止状態が変わった時に通知します。 </summary>
        public event Action<bool> OnVisibilityChanged;

        /// <summary> 通知の表示処理が進行中であればtrueです。 </summary>
        public bool IsVisible => _activeCancellation != null;

        /// <summary>
        ///     必須参照を検証し、最初の通知まで表示を隠します。
        /// </summary>
        public bool Initialize()
        {
            if (_panel == null || _message == null)
            {
                Debug.LogError($"[{nameof(EventNotificationView)}] 通知のパネルまたはテキストが未設定です。", this);
                return false;
            }

            _panel.alpha = 0f;
            _panel.gameObject.SetActive(false);
            return true;
        }

        /// <summary>
        ///     翻訳された通知を1秒保持し、0.5秒のフェードアウト完了まで待ちます。
        /// </summary>
        public async Task ShowAsync(string entry, CancellationToken cancellationToken)
        {
            // 表示中や無効な状態では表示できない。
            cancellationToken.ThrowIfCancellationRequested();
            if (IsVisible || !isActiveAndEnabled)
            {
                throw new InvalidOperationException("通知を表示できる状態ではありません。");
            }

            // 呼び出し元の取り消しか破棄で止められるようにする。
            using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(
                cancellationToken, destroyCancellationToken);
            _activeCancellation = cancellation;
            try
            {
                // 通知を表示し、一定時間待ってからフェードアウトさせる。
                OnVisibilityChanged?.Invoke(true);
                using var localizedText = new LocalizedElementText(
                    "UICommon", entry, text => _message.text = text, GetFallback(entry));
                _panel.alpha = 1f;
                _panel.gameObject.SetActive(true);
                double startedAt = Time.realtimeSinceStartupAsDouble;
                while (true)
                {
                    await Awaitable.NextFrameAsync(cancellation.Token);
                    cancellation.Token.ThrowIfCancellationRequested();
                    double elapsed = Time.realtimeSinceStartupAsDouble - startedAt;
                    if (elapsed >= HOLD_SECONDS + FADE_SECONDS) { break; }
                    _panel.alpha = 1f - Mathf.Clamp01((float)(elapsed - HOLD_SECONDS) / FADE_SECONDS);
                }
                _panel.alpha = 0f;
            }
            finally
            {
                // 止められた場合も含め、必ず非表示に戻す。
                if (_panel != null)
                {
                    _panel.alpha = 0f;
                    _panel.gameObject.SetActive(false);
                }
                _activeCancellation = null;
                OnVisibilityChanged?.Invoke(false);
            }
        }

        /// <summary>
        ///     後続処理を実行せず、現在の通知を取り消します。
        /// </summary>
        public void Cancel()
        {
            _activeCancellation?.Cancel();
        }

        private const float HOLD_SECONDS = 3f;
        private const float FADE_SECONDS = 0.5f;

        [SerializeField, Tooltip("中央通知の背景と文字をまとめてフェードするパネルです。")]
        private CanvasGroup _panel;

        [SerializeField, Tooltip("UICommonの翻訳を表示する通知テキストです。")]
        private TMP_Text _message;

        private CancellationTokenSource _activeCancellation;

        /// <summary>
        ///     常駐画面の無効化時に表示待機を終了します。
        /// </summary>
        private void OnDisable()
        {
            Cancel();
        }

        /// <summary>
        ///     翻訳ロード中も選択言語で同じ内容を表示します。
        /// </summary>
        private static string GetFallback(string entry)
        {
            bool isEnglish = LocalizationSettings.SelectedLocale != null
                && LocalizationSettings.SelectedLocale.Identifier.Code.StartsWith("en", StringComparison.Ordinal);
            return entry switch
            {
                "ui.notification.demo_expired" => isEnglish
                    ? "Time is up. The demo will now end."
                    : "お時間になりました。体験版を終了します。",
                "ui.notification.home_expired" => isEnglish
                    ? "The home timer has expired. Moving to the mission."
                    : "ホームタイマーが終了しました。\nミッションに移動します。",
                "ui.notification.save_reset" => isEnglish
                    ? "Save data has been reset."
                    : "セーブデータをリセットしました。",
                "ui.notification.save_failed" => isEnglish
                    ? "Failed to save data."
                    : "データの保存に失敗しました。",
                _ => throw new ArgumentOutOfRangeException(nameof(entry), entry, "未定義の通知です。"),
            };
        }
    }
}
