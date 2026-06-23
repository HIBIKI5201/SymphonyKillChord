using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace KillChord.Runtime.View
{
    public class SettingSlider : SettingBase
    {
        [SerializeField]
        private float _slideValue = 0.3f;
        private Slider _sliderInstance;

        protected override void OnInitialize()
        {
            _sliderInstance = _baseInstance.Q<Slider>();
            if (_sliderInstance == null)
            {
                Debug.LogError($"{typeof(Slider)}Prefabをデータにバインドしてください。");
                return;
            }
            
        }

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
