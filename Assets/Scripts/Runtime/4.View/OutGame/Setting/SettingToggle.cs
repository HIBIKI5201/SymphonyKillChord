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
        private string _pageName = "AudioPage";
        [SerializeField]
        private string _categoryName = "AudioCategory";

        private void Start()
        {
            var root = _uiRoot.rootVisualElement;
            var page = root.Q<VisualElement>(_pageName);
            var button = root.Q<Button>(_categoryName);
            var prefab = _visualPrefab.Instantiate();
            button.text = _pageName;
            button.clicked += () => ShowPage(_pageName);
            // element.Q<Label>().text = _titleText;
            Slider slider = prefab.Q<Slider>();
            slider.value = _slideValue;
            page.Add(prefab);
        }

        private void ShowPage(string pageName)
        {
            var root = _uiRoot.rootVisualElement;

            root.Q<VisualElement>("AudioPage").style.display = DisplayStyle.None;
            root.Q<VisualElement>("ScreenPage").style.display = DisplayStyle.None;
            root.Q<VisualElement>("KeyPage").style.display = DisplayStyle.None;

            root.Q<VisualElement>(pageName).style.display = DisplayStyle.Flex;
        }
    }
}
