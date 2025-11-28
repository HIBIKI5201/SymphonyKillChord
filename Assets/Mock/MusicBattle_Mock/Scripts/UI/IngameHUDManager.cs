using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UIElements;

namespace Mock.MusicBattle.UI
{
    /// <summary>
    ///     インゲームのHUDを管理するクラス。
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public class IngameHUDManager : MonoBehaviour
    {
        /// <summary>
        ///     ヘルスバーの割合を変更する。
        /// </summary>
        /// <param name="current"></param>
        /// <param name="max"></param>
        public void ChangeHealthBar(float current, float max) => 
            _playerHealthBar?.ChangeHealthBar(current, max, destroyCancellationToken);

        private UIDocument _document;
        private VisualElement _root;

        private PlayerHealthBar _playerHealthBar;

        private void Awake()
        {
            if (TryGetComponent(out _document))
            {
                _root = _document.rootVisualElement;
            }
        }

        private void Start()
        {
            _playerHealthBar = new PlayerHealthBar();
            _root.Add(_playerHealthBar);
        }
    }
}
