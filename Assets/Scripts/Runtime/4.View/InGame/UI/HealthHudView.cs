using KillChord.Runtime.Adaptor.InGame.UI;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace KillChord.Runtime.View.InGame.UI
{
    /// <summary>
    ///     HPバーHUDのViewクラス。
    /// </summary>
    public class HealthHudView : MonoBehaviour
    {
        /// <summary>
        ///     依存関係構築、及びReactivePropertyの購読。
        /// </summary>
        /// <param name="vm"></param>
        public void Bind(IHealthHudViewModel vm)
        {
            _vm = vm;

            _vm.HealthHudDTO.Subscribe(UpdateHpHud).RegisterTo(destroyCancellationToken);
        }

        [SerializeField] private Image _healthBarImage;
        [SerializeField] private TextMeshProUGUI _currentHealthText;
        [SerializeField] private TextMeshProUGUI _maxHealthText;
        private IHealthHudViewModel _vm;

        /// <summary>
        ///     HUDを更新する。
        /// </summary>
        /// <param name="dto"></param>
        private void UpdateHpHud(HealthHudDTO dto)
        {
            _currentHealthText.SetText("{0}", Mathf.CeilToInt(dto.CurrentHealth));
            _maxHealthText.SetText("{0}", Mathf.CeilToInt(dto.MaxHealth));
            _healthBarImage.fillAmount = dto.CurrentHealth / dto.MaxHealth;
        }
    }
}
