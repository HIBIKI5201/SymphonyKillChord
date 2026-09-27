using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace KillChord.Runtime.View
{
    /// <summary>
    ///     スライダー形式の設定項目。
    /// </summary>
    public class SettingSlider : SettingBase
    {
        [SerializeField, Tooltip("スライダーの初期値。")]
        private float _slideValue = 0.3f;
        private Slider _sliderInstance;

        /// <summary>
        ///     スライダーを取得して初期値を設定する。
        /// </summary>
        protected override void OnInitialize()
        {
            _sliderInstance = _baseInstance.Q<Slider>();
            if (_sliderInstance == null)
            {
                Debug.LogError($"{typeof(Slider)}Prefabをデータにバインドしてください。");
                return;
            }
            
        }

        /// <summary>
        ///     設定値の取得・反映の処理とスライダーを結びつける。
        /// </summary>
        public void Bind(Func<float> getter, Action<float> setter)
        {
            _sliderInstance.SetValueWithoutNotify(getter());

            _sliderInstance.RegisterValueChangedCallback(evt =>
            {
                setter(evt.newValue);
            });
        }
    }
}
