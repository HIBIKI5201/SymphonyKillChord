using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace KillChord.Runtime.View
{
    public class SettingToggle : SettingBase
    {
        [SerializeField]
        private bool _isOn;
        private Toggle _toggleInstance;

        protected override void OnInitialize()
        {
            _toggleInstance = _baseInstance.Q<Toggle>();
            if (_toggleInstance == null)
            {
                Debug.LogError($"{typeof(Toggle)}Prefab is not bound.");
                return;
            }

            _toggleInstance.value = _isOn;
            _toggleInstance.RegisterValueChangedCallback(evt =>
            {
                _isOn = evt.newValue;
            });
        }

        public void Bind(Func<bool> getter, Action<bool> setter)
        {
            _toggleInstance.SetValueWithoutNotify(getter());
            _toggleInstance.RegisterValueChangedCallback(evt =>
            {
                setter(evt.newValue);
            });
        }
    }
}
