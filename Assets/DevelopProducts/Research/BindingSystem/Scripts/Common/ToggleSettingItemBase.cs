using System;
using UnityEngine;
using UnityEngine.UI;

namespace DevelopProducts.BindingSystem
{
    /// <summary>
    ///     トグル(ON/OFF)で操作する設定項目の共通基底クラス。
    /// </summary>
    [Serializable]
    public abstract class ToggleSettingItemBase : SettingItemBase, IToggleSetting
    {
        [SerializeField] private Toggle _toggle;
        [SerializeField] private bool _defaultValue;

        public Toggle Toggle => _toggle;

        /// <summary>値を実システムに反映する処理。</summary>
        protected abstract void ApplyValue(bool value);

        public override void Initialize(PauseMenuPanelView pauseMenu)
        {
            base.Initialize(pauseMenu);
            _toggle.onValueChanged.AddListener(_ => Apply());
        }

        public override void Load() => Apply();

        public override void Apply() => ApplyValue(_toggle.isOn);

        public override void ResetToDefault()
        {
            _toggle.SetIsOnWithoutNotify(_defaultValue);
            Apply();
        }
    }
}
