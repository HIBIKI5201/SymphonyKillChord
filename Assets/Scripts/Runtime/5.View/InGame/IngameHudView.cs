using UnityEngine;
using UnityEngine.UI;

namespace KillChord.Runtime.View
{
    public class IngameHudView : MonoBehaviour
    {
        [SerializeField] private Image _hpBarImage;

        public void ChangeHitPoint(float fillAmount)
        {
            _hpBarImage.fillAmount = fillAmount;
        }
    }
}