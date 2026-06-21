using System;
using CriWare;

namespace DevelopProducts.BindingSystem
{
    /// <summary>
    ///     CRIのバスボリュームをスライダーで操作する設定項目の共通基底クラス。
    /// </summary>
    [Serializable]
    public abstract class VolumeSettingBase : SliderSettingItemBase
    {
        /// <summary>操作対象のCRIバス名。</summary>
        protected abstract string BusName { get; }

        protected override void ApplyValue(float value) => CriAtomExAsr.SetBusVolume(BusName, value);
    }
}
