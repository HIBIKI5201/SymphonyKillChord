using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace KillChord.Runtime.View
{
    [CreateAssetMenu(fileName = "DropDownSetting", menuName = "KillChord/Settings/DropDown")]
    public class SettingDropDown : SettingBase
    {
        [SerializeField]
        private List<string> _choices = new List<string> { "Option" };
        [SerializeField]
        private int _selectedIndex;

        protected override void Bind()
        {
            DropdownField dropDown = _instance.Q<DropdownField>();
            if(dropDown == null) {
                Debug.LogError($"{typeof(DropdownField)}Prefabをデータにバインドしてください。");
                return;
            }

            if (_choices == null)
            {
                _choices = new List<string>();
            }

            dropDown.choices = _choices;

            if (_choices.Count <= 0)
            {
                dropDown.index = -1;
                return;
            }

            _selectedIndex = Mathf.Clamp(_selectedIndex, 0, _choices.Count - 1);
            dropDown.index = _selectedIndex;
            dropDown.RegisterValueChangedCallback(evt =>
            {
                _selectedIndex = dropDown.index;
            });
        }
    }
}
