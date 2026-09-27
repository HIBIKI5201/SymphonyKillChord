using KillChord.Runtime.Adaptor.InGame.UI;
using R3;

namespace KillChord.Runtime.View.InGame.UI
{
    /// <summary>
    ///     HUDの表示状態を管理するViewModelクラス。
    /// </summary>
    public class IngameHudViewModel : IIngameHudViewModel
    {
        /// <summary> 最大 HP に対する現在 HP の割合。 </summary>
        public ReadOnlyReactiveProperty<float> HealthRate => _healthRate;

        private ReactiveProperty<float> _healthRate = new(1);

        /// <summary>
        ///     HP の表示内容から HP の割合を計算して反映する。
        ///     最大 HP が 0 以下の場合は 0 にする。
        /// </summary>
        public void UpdateHealth(in IngameHudDTO dto)
        {
            if (dto.MaxHealth <= 0)
            {
                _healthRate.Value = 0;
                return;
            }

            _healthRate.Value = dto.CurrentHealth / dto.MaxHealth;
        }
    }
}
