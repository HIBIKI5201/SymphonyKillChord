using Mock.MusicBattle.Character;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

namespace Mock.MusicBattle.UI
{
    /// <summary>
    ///     インゲームのHUDを管理するクラス。
    /// </summary>
    [DefaultExecutionOrder(-1000)]
    [RequireComponent(typeof(UIDocument))]
    public class IngameHUDManager : MonoBehaviour
    {
        public void InitializePlayerHealthBar(HealthEntity healthEntity) => 
            _playerHealthBar?.Initialize(healthEntity);

        public async Task<EnemyHealthBar> AddEnemyHealthBar(HealthEntity healthEntity, Transform transform)
        {
            while (_root == null) await Awaitable.NextFrameAsync();

            EnemyHealthBar enemyHealthBar = new EnemyHealthBar();
            _root.Add(enemyHealthBar);
            enemyHealthBar.Initialize(healthEntity, transform);

            return enemyHealthBar;
        }

        private UIDocument _document;
        private VisualElement _root;

        private PlayerHealthBar _playerHealthBar;
        private List<EnemyHealthBar> _enemyHealthBars = new();

        private void Start()
        {
            if (TryGetComponent(out _document))
            {
                _root = _document.rootVisualElement;
                if (_root == null)
                {
                    Debug.LogError("rootVisualElement の取得に失敗しました");
                    return;
                }

                _playerHealthBar = new PlayerHealthBar();
                _root.Add(_playerHealthBar);
            }
        }
    }
}
