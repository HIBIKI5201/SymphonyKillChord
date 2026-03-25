using UnityEngine;
using UnityEngine.UI;

namespace KillChord.Runtime.View
{
    public class IngameHudView : MonoBehaviour
    {
        [SerializeField] private Image _halthBarImage;

        private void ChangeHitPoint(float fillAmount)
        {
            _halthBarImage.fillAmount = Mathf.Clamp(fillAmount, 0f, 1f);
        }
    }
}

