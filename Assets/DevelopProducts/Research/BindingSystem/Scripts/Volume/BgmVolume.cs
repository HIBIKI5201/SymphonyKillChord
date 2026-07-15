using System;

namespace DevelopProducts.BindingSystem
{
    /// <summary>
    ///     BGMボリューム設定。
    /// </summary>
    [Serializable]
    public class BgmVolume : VolumeSettingBase
    {
        protected override string BusName => "BGM";
    }
}
