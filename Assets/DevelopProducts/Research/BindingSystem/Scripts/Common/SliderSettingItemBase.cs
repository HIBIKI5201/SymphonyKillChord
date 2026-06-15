using System;
using UnityEngine;
using UnityEngine.UI;

namespace DevelopProducts.BindingSystem
{
    /// <summary>
    ///     スライダーで操作する設定項目の共通基底クラス。
    /// </summary>
    [Serializable]
    public abstract class SliderSettingItemBase : SettingItemBase, ISliderSetting
    {
        [SerializeField] private Slider _slider;
        [SerializeField, Range(0f, 1f)] private float _defaultValue = 1f;

        public Slider Slider => _slider;

        /// <summary>値を実システムに反映する処理。</summary>
        protected abstract void ApplyValue(float value);

        public override void Initialize(PauseMenuPanelView pauseMenu)
        {
            base.Initialize(pauseMenu);
            _slider.onValueChanged.AddListener(_ => Apply());
        }

        public override void Load() => Apply();

        public override void Apply() => ApplyValue(_slider.value);

        public override void ResetToDefault()
        {
            _slider.SetValueWithoutNotify(_defaultValue);
            Apply();
        }
    }
}
