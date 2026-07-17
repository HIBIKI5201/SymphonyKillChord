using UnityEngine.UI;

namespace DevelopProducts.BindingSystem
{
    /// <summary>
    ///     スライダーで操作する設定項目（音量・感度など）。
    /// </summary>
    public interface ISliderSetting : ISettingItem
    {
        Slider Slider { get; }
    }
}
