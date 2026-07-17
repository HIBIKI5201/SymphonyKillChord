using System;
using UnityEngine;
using UnityEngine.UI;

namespace DevelopProducts.BindingSystem
{
    [Serializable]
    public class ButtonPanelSet
    {
        public Button Button => _button;
        public SettingPanelView PanelView => _panelView;

        [SerializeField] private Button _button;
        [SerializeField] private SettingPanelView _panelView;
    }   
}
