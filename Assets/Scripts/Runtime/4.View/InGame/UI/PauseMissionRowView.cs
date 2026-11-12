using TMPro;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.UI
{
    /// <summary>
    ///     ポーズ画面のミッション欄で、ミッション1件分の表示文と達成状況を出すView。
    /// </summary>
    [AddComponentMenu("KillChord/View/UI/PauseMissionRowView")]
    public sealed class PauseMissionRowView : MonoBehaviour
    {
        /// <summary>
        ///     ミッション1件分の内容を表示する。
        /// </summary>
        /// <param name="description"> ミッションの表示文。 </param>
        /// <param name="statusText"> 達成状況の表示文。 </param>
        /// <param name="isAchieved"> 達成している場合はtrue。 </param>
        public void SetContent(string description, string statusText, bool isAchieved)
        {
            if (_descriptionText != null)
            {
                _descriptionText.text = description;
            }

            if (_statusText != null)
            {
                _statusText.text = statusText;
                _statusText.color = isAchieved ? _achievedColor : _notAchievedColor;
            }
        }

        [SerializeField, Tooltip("ミッションの表示文を出すテキストです。")]
        private TMP_Text _descriptionText;

        [SerializeField, Tooltip("達成状況を出すテキストです。")]
        private TMP_Text _statusText;

        [SerializeField, Tooltip("達成しているときの達成状況の文字色です。")]
        private Color _achievedColor = new(1f, 0.8f, 0.2f, 1f);

        [SerializeField, Tooltip("達成していないときの達成状況の文字色です。")]
        private Color _notAchievedColor = new(0.7f, 0.7f, 0.7f, 1f);
    }
}
