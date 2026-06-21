using UnityEngine.UI;

namespace DevelopProducts.BindingSystem
{
    /// <summary>
    ///     トグル(ON/OFF)で操作する設定項目（フルスクリーン・Vsyncなど）。
    /// </summary>
    public interface IToggleSetting : ISettingItem
    {
        Toggle Toggle { get; }
    }
}
