using CriWare;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DevelopProducts.BindingSystem
{
    /// <summary>
    ///      ポーズメニュー全体の管理を行うクラス
    /// </summary>
    public class PauseMenuPanelView : MonoBehaviour
    {

        private void Awake()
        {
            foreach (var panel in _settingPanels)
            {
                panel.PanelView.Initialize(this);
            }
            _currentSettingPanel = _settingPanels[0].PanelView;
        }
        [SerializeField] private InputActionAsset _inputActionAsset;
        [SerializeField] private CriAtom _criAtom;
        [SerializeField] private ButtonPanelSet[] _settingPanels;
        private SettingPanelView _currentSettingPanel;
    }
}
