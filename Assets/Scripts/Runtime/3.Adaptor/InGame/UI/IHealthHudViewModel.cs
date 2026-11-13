using R3;

namespace KillChord.Runtime.Adaptor.InGame.UI
{
    /// <summary>
    ///     HPのHUD用のViewModelインタフェース。
    /// </summary>
    public interface IHealthHudViewModel
    {
        /// <summary> 現在HP。 </summary>
        public ReadOnlyReactiveProperty<float> CurrentHealth { get; }

        /// <summary> 最大HP。 </summary>
        public ReadOnlyReactiveProperty<float> MaxHealth { get; }

        /// <summary> 最大HPに対する現在HPの割合（0〜1）。最大HPが0以下のときは0。 </summary>
        public ReadOnlyReactiveProperty<float> HealthRatio { get; }

        /// <summary>
        ///     HP情報を更新する。
        /// </summary>
        /// <param name="dto"> 更新後のHP情報。 </param>
        public void UpdateHealth(in HealthHudDTO dto);
    }
}
