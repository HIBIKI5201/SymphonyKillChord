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

        private void Awake()
        {
            if (_healthBarImage == null || _currentHealthText == null || _maxHealthText == null)
            {
                Debug.LogError("[HealthHudView] UIの参照が失っています。", this);
            }
        }

        [SerializeField] private Image _healthBarImage;
        [SerializeField] private TextMeshProUGUI _currentHealthText;
        [SerializeField] private TextMeshProUGUI _maxHealthText;
        private IHealthHudViewModel _vm;

        /// <summary>
        ///     HUDを更新する。
        /// </summary>
        /// <param name="dto">HP HUD用のDTO</param>
        private void UpdateHpHud(HealthHudDTO dto)
        {
            _currentHealthText.SetText("{0}", Mathf.CeilToInt(dto.CurrentHealth));
            _maxHealthText.SetText("{0}", Mathf.CeilToInt(dto.MaxHealth));
            _healthBarImage.fillAmount = Mathf.Clamp01(dto.CurrentHealth / dto.MaxHealth);
        }
    }
}
