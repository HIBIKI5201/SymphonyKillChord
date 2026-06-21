using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace KillChord.Runtime.View
{
    [CreateAssetMenu(fileName = "SliderSetting",menuName ="KillChord/Settings/Slider")]
    public class SettingSlider : SettingBase
    {
        [SerializeField]
        private float _slideValue = 0.3f;

        protected override void Bind()
        {
            Slider slider = _instance.Q<Slider>();
            if(slider == null) {
                Debug.LogError($"{typeof(Slider)}Prefabをデータにバインドしてください。");
                return;
            }
            slider.value = _slideValue;
            slider.RegisterValueChangedCallback(evt =>
            {
               _slideValue = slider.value; 
            });
        }
    }
}
