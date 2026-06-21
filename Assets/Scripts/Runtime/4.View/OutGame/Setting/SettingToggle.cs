using UnityEngine;
using UnityEngine.UIElements;

namespace KillChord.Runtime.View
{
    [CreateAssetMenu(fileName = "ToggleSetting", menuName = "KillChord/Settings/Toggle")]
    public class SettingToggle : SettingBase
    {
        [SerializeField]
        private bool _isOn;

        protected override void Bind()
        {
            Toggle toggle = _instance.Q<Toggle>();
            if (toggle == null){
                Debug.LogError($"{typeof(Toggle)}Prefabをデータにバインドしてください。");
                return;
            } 
            toggle.value = _isOn;
            toggle.RegisterValueChangedCallback(evt =>
            {
                _isOn = evt.newValue;
            });
        }
    }
}
