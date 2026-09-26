using KillChord.Runtime.Utility.Diagnostics;
using UnityEngine;
using UnityEngine.UIElements;

namespace KillChord.Runtime.View
{
    /// <summary>
    ///     設定画面の項目の基底クラス。
    /// </summary>
    public abstract partial class SettingBase : MonoBehaviour
    {
        [SerializeField, Tooltip("項目の見た目となる UXML。")]
        protected VisualTreeAsset _visualPrefab;
        protected string _pageName;
        protected string _categoryName;
        protected VisualElement _baseInstance;
        protected UIDocument _uiRoot;
        /// <summary>
        ///     指定カテゴリのページに項目を生成する。
        /// </summary>
        public void Create(UIDocument uiDocument, Category category, string title)
        {
            Initialize(category);
            _uiRoot = uiDocument;
            var root = _uiRoot.rootVisualElement;
            var page = root.Q<VisualElement>(_pageName);
            var button = root.Q<Button>(_categoryName);
            _baseInstance = _visualPrefab.Instantiate();


            button.text = _pageName;
            button.clicked += () => ShowPage(_pageName);

            OnInitialize();
            var label = _baseInstance.Q<Label>();
            label.text = title;
            page.Add(_baseInstance);
        }

        /// <summary>
        ///     カテゴリからカテゴリ名とページ名を決める。
        /// </summary>
        private void Initialize(Category category)
        {
            _categoryName = $"{category}Category";
            _pageName = $"{category}Page";
        }
        /// <summary>
        ///     UI特有の初期化。
        /// </summary>
        protected abstract void OnInitialize();

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

        // protected virtual void Info<T>(object message)
        // {
        //     DevLog.Log($"{typeof(T).Name}型 {message}");
        // }

        // protected virtual void Warning<T>(object message)
        // {
        //     Debug.LogWarning($"{typeof(T).Name}型 {message}");
        // }
        // protected virtual void Error<T>(object message)
        // {
        //     Debug.LogError($"{typeof(T).Name}型 {message}");
        // }
    }
}
