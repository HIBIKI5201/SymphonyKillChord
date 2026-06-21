using UnityEngine;
using UnityEngine.UIElements;

namespace KillChord.Runtime.View
{
    public class SettingManager : MonoBehaviour
    {
        [SerializeField]
        private SettingBase[] _settingBases;
        [SerializeField]
        private UIDocument _uiRoot;

        private void Start()
        {
            for(int i = 0; i < _settingBases.Length ; i++)
            {
                _settingBases[i].Create(_uiRoot);
            }
        }
    }
}
