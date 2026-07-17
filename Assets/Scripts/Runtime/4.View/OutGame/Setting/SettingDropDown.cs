using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace KillChord.Runtime.View
{
    public class SettingDropDown : SettingBase
    {
        [SerializeField]
        private List<string> _choices = new List<string> { "Option" };
        [SerializeField]
        private int _selectedIndex;
        private DropdownField _dropDownInstance;

        protected override void OnInitialize()
        {
            _dropDownInstance = _baseInstance.Q<DropdownField>();
            if (_dropDownInstance == null)
            {
                Debug.LogError($"{typeof(DropdownField)}Prefab is not bound.");
                return;
            }

            if (_choices == null)
            {
                _choices = new List<string>();
            }

            _dropDownInstance.choices = _choices;

            if (_choices.Count <= 0)
            {
                _dropDownInstance.index = -1;
                return;
            }

            _selectedIndex = Mathf.Clamp(_selectedIndex, 0, _choices.Count - 1);
            _dropDownInstance.index = _selectedIndex;
            _dropDownInstance.RegisterValueChangedCallback(evt =>
            {
                _selectedIndex = _dropDownInstance.index;
            });
        }

        public void Bind(Func<int> getter, Action<int> setter)
        {
            if (_dropDownInstance.choices.Count <= 0)
            {
                return;
            }

            int index = Mathf.Clamp(getter(), 0, _dropDownInstance.choices.Count - 1);
            _dropDownInstance.SetValueWithoutNotify(_dropDownInstance.choices[index]);
            _dropDownInstance.RegisterValueChangedCallback(evt =>
            {
                setter(_dropDownInstance.index);
            });
        }
    }
}
