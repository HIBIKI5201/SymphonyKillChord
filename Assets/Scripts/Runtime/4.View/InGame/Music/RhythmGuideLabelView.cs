using TMPro;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.Music
{
    public class RhythmGuideLabelView : MonoBehaviour
    {
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

        [SerializeField] private RectTransform _root;
        [SerializeField] private TMP_Text _beatText;
    }
}
