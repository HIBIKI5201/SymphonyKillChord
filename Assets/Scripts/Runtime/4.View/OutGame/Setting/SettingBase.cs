using UnityEngine;
using UnityEngine.UIElements;

namespace KillChord.Runtime.View
{
    public abstract class SettingBase : ScriptableObject
    {
        [SerializeField]
        protected VisualTreeAsset _visualPrefab;
        [SerializeField]
        protected string _titleText = "Master Volume";
        [SerializeField]
        protected string _pageName = "AudioPage";
        [SerializeField]
        protected string _categoryName = "AudioCategory";
        protected VisualElement _instance;
        protected UIDocument _uiRoot;
        public void Create(UIDocument uiDocument)
        {
            _uiRoot = uiDocument;
            var root = _uiRoot.rootVisualElement;
            var page = root.Q<VisualElement>(_pageName);
            var button = root.Q<Button>(_categoryName);
            _instance = _visualPrefab.Instantiate();
            button.text = _pageName;
            button.clicked += () => ShowPage(_pageName);

            Bind();
            page.Add(_instance);
        }

        /// <summary>
        ///     UI特有のバインド処理。
        /// </summary>
        protected abstract void Bind();
        /// <summary>
        ///     ページ切り替え。
        /// </summary>
        /// <param name="pageName"> 立ち上げるページ </param>
        protected void ShowPage(string pageName)
        {
            var root = _uiRoot.rootVisualElement;

            root.Q<VisualElement>("AudioPage").style.display = DisplayStyle.None;
            root.Q<VisualElement>("ScreenPage").style.display = DisplayStyle.None;
            root.Q<VisualElement>("KeyPage").style.display = DisplayStyle.None;

            root.Q<VisualElement>(pageName).style.display = DisplayStyle.Flex;
        }
    }
}
