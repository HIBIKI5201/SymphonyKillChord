using KillChord.Runtime.Adaptor.InGame.UI;
using R3;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.UI
{
    /// <summary>
    ///     HPのHUD用のViewModelクラス。
    ///     受け取ったDTOを、Viewが表示に使う値（現在HP・最大HP・割合）に分けて保持する。
    /// </summary>
    public class HealthHudViewModel : IHealthHudViewModel
    {
        /// <summary>
        ///     現在 HP と最大 HP を指定して生成する。
        /// </summary>
        /// <param name="currentHealth"> 現在HP。 </param>
        /// <param name="maxHealth"> 最大HP。 </param>
        public HealthHudViewModel(float currentHealth, float maxHealth)
        {
            _maxHealth = new ReactiveProperty<float>(maxHealth);
            _currentHealth = new ReactiveProperty<float>(currentHealth);
            _healthRatio = new ReactiveProperty<float>(CalculateRatio(currentHealth, maxHealth));
        }

        /// <summary> 現在HP。 </summary>
        public ReadOnlyReactiveProperty<float> CurrentHealth => _currentHealth;

        /// <summary> 最大HP。 </summary>
        public ReadOnlyReactiveProperty<float> MaxHealth => _maxHealth;

        /// <summary> 最大HPに対する現在HPの割合（0〜1）。最大HPが0以下のときは0。 </summary>
        public ReadOnlyReactiveProperty<float> HealthRatio => _healthRatio;

        /// <summary>
        ///     HP情報を更新する。
        ///     割合を購読するViewが最大HPを参照できるよう、最大HP・現在HP・割合の順に更新する。
        /// </summary>
        /// <param name="dto"> 更新後のHP情報。 </param>
        public void UpdateHealth(in HealthHudDTO dto)
        {
            _maxHealth.Value = dto.MaxHealth;
            _currentHealth.Value = dto.CurrentHealth;
            _healthRatio.Value = CalculateRatio(dto.CurrentHealth, dto.MaxHealth);
        }

        private readonly ReactiveProperty<float> _currentHealth;
        private readonly ReactiveProperty<float> _maxHealth;
        private readonly ReactiveProperty<float> _healthRatio;

        /// <summary>
        ///     最大HPに対する現在HPの割合を求める。
        /// </summary>
        /// <param name="currentHealth"> 現在HP。 </param>
        /// <param name="maxHealth"> 最大HP。 </param>
        /// <returns> 0〜1の割合。最大HPが0以下のときは0。 </returns>
        private static float CalculateRatio(float currentHealth, float maxHealth)
        {
            return maxHealth > 0f ? Mathf.Clamp01(currentHealth / maxHealth) : 0f;
        }
    }
}
