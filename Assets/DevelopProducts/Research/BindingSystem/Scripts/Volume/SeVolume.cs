using System;

namespace DevelopProducts.BindingSystem
{
    /// <summary>
    ///     SEボリューム設定。
    /// </summary>
    [Serializable]
    public class SeVolume : VolumeSettingBase
    {
        protected override string BusName => "SE";
    }
}
