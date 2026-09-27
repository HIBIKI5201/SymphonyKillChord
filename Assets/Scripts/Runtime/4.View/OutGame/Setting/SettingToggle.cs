using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace KillChord.Runtime.View
{
    /// <summary>
    ///     トグル形式の設定項目。
    /// </summary>
    public class SettingToggle : SettingBase
    {
        [SerializeField, Tooltip("トグルの初期値。")]
        private bool _isOn;
        private Toggle _toggleInstance;

        /// <summary>
        ///     トグルを取得して初期値を設定する。
        /// </summary>
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

        /// <summary>
        ///     設定値の取得・反映の処理とトグルを結びつける。
        /// </summary>
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
