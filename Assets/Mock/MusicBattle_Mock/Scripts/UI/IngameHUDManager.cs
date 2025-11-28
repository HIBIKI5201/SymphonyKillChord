using Mock.MusicBattle.Character;
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
        public void InitializePlayerHealthBar(HealthEntity healthEntity) => 
            _playerHealthBar.Initialize(healthEntity);

        public EnemyHealthBar AddEnemyHealthBar(HealthEntity healthEntity)
        {
            EnemyHealthBar enemyHealthBar = new EnemyHealthBar();
            _root.Add(enemyHealthBar);
            enemyHealthBar.Initialize(healthEntity);

            return enemyHealthBar;
        }

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
