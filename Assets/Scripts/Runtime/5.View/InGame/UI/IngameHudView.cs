using R3;
using UnityEngine;
using UnityEngine.UI;

namespace KillChord.Runtime.View
{
    public class IngameHudView : MonoBehaviour
    {
        [SerializeField] private Image _healthBarImage;

        private IngameHudViewModel _viewModel;
        
        public void Bind(IngameHudViewModel viewModel)
        {
            _viewModel = viewModel;

            _viewModel.HealthRate.Subscribe(ChangeHitPoint).RegisterTo(destroyCancellationToken);
        }

        private void ChangeHitPoint(float fillAmount)
        {
            _healthBarImage.fillAmount = Mathf.Clamp(fillAmount, 0f, 1f);
        }
    }
}