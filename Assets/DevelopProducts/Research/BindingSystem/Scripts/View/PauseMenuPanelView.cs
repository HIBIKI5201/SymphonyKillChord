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
        public CriAtom CriAtom => _criAtom;
        private void Awake()
        {
            foreach (var panel in _settingPanels)
            {
                panel.PanelView.Initialize(this);
                panel.Button.onClick.AddListener(() => SwitchPanel(panel.PanelView));
            }
            SwitchPanel(_settingPanels[0].PanelView);
        }

        /// <summary>
        ///     表示するパネルを切り替える。それまで表示していたパネルは非表示にする。
        /// </summary>
        /// <param name="panel"></param>
        private void SwitchPanel(SettingPanelView panel)
        {
            if (_currentSettingPanel != null)
            {
                _currentSettingPanel.SetPanelActive(false);
            }
            panel.SetPanelActive(true);
            _currentSettingPanel = panel;
        }
        [SerializeField] private InputActionAsset _inputActionAsset;
        [SerializeField] private CriAtom _criAtom;
        [SerializeField] private ButtonPanelSet[] _settingPanels;
        private SettingPanelView _currentSettingPanel;
    }
}
