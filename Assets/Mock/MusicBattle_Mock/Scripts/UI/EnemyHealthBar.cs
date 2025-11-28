using UnityEngine;
using UnityEngine.UIElements;

namespace Mock.MusicBattle
{
    [UxmlElement]
    public partial class EnemyHealthBar : VisualElement
    {
        public EnemyHealthBar()
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

        private const string UXML_RESOURCES_PATH = "EnemyHealthBar";
    }
}
