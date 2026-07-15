using System;

namespace DevelopProducts.BindingSystem
{
    /// <summary>
    ///     「←  項目  →」のように選択肢を切り替える設定項目（解像度・画質プリセットなど）。
    /// </summary>
    public interface ISelectSetting : ISettingItem
    {
        Array Items { get; }
        int CurrentIndex { get; }

        void MoveNext();
        void MoveBack();
    }
}
