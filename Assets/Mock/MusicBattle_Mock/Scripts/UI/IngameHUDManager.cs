using UnityEngine;
using UnityEngine.UIElements;

namespace Mock.MusicBattle
{
    /// <summary>
    ///     インゲームのHUDを管理するクラス。
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public class IngameHUDManager : MonoBehaviour
    {
        private UIDocument _document;
        private VisualElement _root;

        private void Awake()
        {
            if (TryGetComponent(out _document))
            {
                _root = _document.rootVisualElement;
            }
        }

        
    }
}
