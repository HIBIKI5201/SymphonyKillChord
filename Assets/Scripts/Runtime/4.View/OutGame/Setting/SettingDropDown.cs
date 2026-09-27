using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace KillChord.Runtime.View
{
    /// <summary>
    ///     ドロップダウン形式の設定項目。
    /// </summary>
    public class SettingDropDown : SettingBase
    {
        [SerializeField, Tooltip("ドロップダウンの選択肢。")]
        private List<string> _choices = new List<string> { "Option" };
        [SerializeField, Tooltip("選択中の選択肢のインデックス。")]
        private int _selectedIndex;
        private DropdownField _dropDownInstance;

        /// <summary>
        ///     ドロップダウンを取得し、選択肢と選択中の項目を設定する。
        /// </summary>
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

        /// <summary>
        ///     設定値の取得・反映の処理とドロップダウンを結びつける。
        /// </summary>
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
