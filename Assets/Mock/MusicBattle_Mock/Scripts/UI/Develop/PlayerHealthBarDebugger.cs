using Mock.MusicBattle.UI;
using UnityEngine;

namespace Mock.MusicBattle.Develop
{
    /// <summary>
    ///     ヘルスバーのデバッグ用コンポーネント。
    /// </summary>
    [RequireComponent(typeof(IngameHUDManager))]
    public class PlayerHealthBarDebugger : MonoBehaviour
    {
        [SerializeField]
        private float _maxHealth = 100f;
        [SerializeField]
        private float _currentHealth = 50f;

        private IngameHUDManager _hud;
        private void Awake()
        {
            _hud = GetComponent<IngameHUDManager>();
        }

        [ContextMenu(nameof(ApplyHealthValue))]
        private void ApplyHealthValue()
        {
            _hud?.ChangeHealthBar(_currentHealth, _maxHealth);
        }
    }
}
