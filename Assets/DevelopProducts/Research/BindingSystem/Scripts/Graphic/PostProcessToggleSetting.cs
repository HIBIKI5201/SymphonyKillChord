using System;
using UnityEngine;

namespace DevelopProducts.BindingSystem
{
    /// <summary>
    ///     ポストプロセスの有効/無効を切り替える設定項目。
    /// </summary>
    [Serializable]
    public class PostProcessToggleSetting : GraphicToggleSettingBase
    {
        [SerializeField] private GameObject _postProcessVolume;

        protected override void ApplyValue(bool value) => _postProcessVolume.SetActive(value);
    }
}
