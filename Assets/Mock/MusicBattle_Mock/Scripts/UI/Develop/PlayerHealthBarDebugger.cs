using Mock.MusicBattle.Character;
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
        private float _damage = 10;

        private IngameHUDManager _hud;
        private HealthEntity _healthEntity;
        private void Awake()
        {
            _hud = GetComponent<IngameHUDManager>();
        }

        private void Start()
        {
            _healthEntity = new HealthEntity(_maxHealth);
            _hud.InitializePlayerHealthBar(_healthEntity);
        }

        [ContextMenu(nameof(ApplyHealthValue))]
        private void ApplyHealthValue()
        {
            _healthEntity.TakeDamage(_damage);
        }
    }
}
