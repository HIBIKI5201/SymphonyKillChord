using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Mock.MusicBattle.UI
{
    /// <summary>
    ///     ダメージテキストの実体クラス。
    /// </summary>
    [UxmlElement]
    public partial class DamageTextEntity : VisualElement
    {
        public DamageTextEntity()
        {
            VisualTreeAsset treeAsset = Resources.Load<VisualTreeAsset>(UXML_RESOURCES_PATH);
            if (treeAsset == null)
            {
                Debug.LogError($"Failed to load UXML at path: {UXML_RESOURCES_PATH}");
                return;
            }
            TemplateContainer container = treeAsset.Instantiate();
            hierarchy.Add(container);
        }

        public void Initialize(Action action) => _onRelease = action;

        public void BindData(float damage, Vector3 position)
        {

        }

        private const string UXML_RESOURCES_PATH = "DamageTextEntity";

        private Action _onRelease;
    }
}
