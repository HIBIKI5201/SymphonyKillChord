using System.Globalization;
using TMPro;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.UI
{
    /// <summary>
    ///     ポーズ解除までの拍を「3・2・1」と画面中央に出すView。
    /// </summary>
    [AddComponentMenu("KillChord/View/UI/ResumeCountdownView")]
    public sealed class ResumeCountdownView : MonoBehaviour
    {
        /// <summary>
        ///     残りの拍数を表示する。1から3の間だけ数字を出し、それ以外は隠す。
        /// </summary>
        /// <param name="remainingBeatCount"> 再開までに残っている拍の数。 </param>
        public void SetRemainingBeatCount(int remainingBeatCount)
        {
            bool isVisible = remainingBeatCount >= MIN_DISPLAY_COUNT && remainingBeatCount <= MAX_DISPLAY_COUNT;
            if (_countText != null)
            {
                _countText.text = isVisible ? remainingBeatCount.ToString(CultureInfo.InvariantCulture) : string.Empty;
                _countText.gameObject.SetActive(isVisible);
            }
        }

        /// <summary> 数字を出す最小の残り拍数。 </summary>
        private const int MIN_DISPLAY_COUNT = 1;

        /// <summary> 数字を出す最大の残り拍数。最初の拍の途中ではまだ出さない。 </summary>
        private const int MAX_DISPLAY_COUNT = 3;

        [SerializeField, Tooltip("残りの拍数を出すテキストです。")]
        private TMP_Text _countText;

        /// <summary> 起動時は数字を隠す。 </summary>
        private void Awake()
        {
            SetRemainingBeatCount(0);
        }
    }
}
