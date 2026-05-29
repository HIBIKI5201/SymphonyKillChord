using TMPro;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.Music
{
    /// <summary>
    ///     リズムガイドのラベル表示を行うViewクラス。
    /// </summary>
    public class RhythmGuideLabelView : MonoBehaviour
    {
        /// <summary>
        ///     ラベルのレンダリングを行う。
        /// </summary>
        /// <param name="beatCount"> 拍数。 </param>
        /// <param name="normalized"> 正規化された位置。 </param>
        /// <param name="radius"> 半径。 </param>
        /// <param name="startAngle"> 開始角度。 </param>
        /// <param name="sweepAngle"> 回転角度。 </param>
        public void Render(int beatCount, float normalized, float radius, float startAngle, float sweepAngle)
        {
            _beatText.text = beatCount.ToString();

            float angle = startAngle + normalized * sweepAngle;
            float radian = angle * Mathf.Deg2Rad;

            Vector2 position = new Vector2(
                Mathf.Cos(radian),
                Mathf.Sin(radian)
            ) * radius;

            _root.anchoredPosition = position;
        }

        [Tooltip("ラベルのルート。")]
        [SerializeField] private RectTransform _root;
        [Tooltip("拍数テキスト。")]
        [SerializeField] private TMP_Text _beatText;
    }
}
