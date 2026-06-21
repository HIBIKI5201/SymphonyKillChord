using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace KillChord.Runtime.View
{
    public class SettingToggle : MonoBehaviour
    {
        [SerializeField]
        private VisualTreeAsset _visualPrefab;
        [SerializeField]
        private UIDocument _uiRoot;
        [SerializeField]
        private string _titleText = "Master Volume";
        [SerializeField]
        private float _slideValue = 0.3f;
        [SerializeField]
        private string _tabName = "Audio";
        private VisualElement _elementRoot;

        private void Start()
        {
            _elementRoot =  _uiRoot.rootVisualElement.Q<VisualElement>(_tabName);
            VisualElement element = _visualPrefab.Instantiate();
            element.Q<Label>().text = _titleText;
            Slider slider = element.Q<Slider>();
            slider.value = _slideValue;
            _elementRoot.Add(element);
        }
    }
}
