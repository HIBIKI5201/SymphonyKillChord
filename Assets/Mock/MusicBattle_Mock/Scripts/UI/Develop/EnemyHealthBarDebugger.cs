using Mock.MusicBattle.Character;
using Mock.MusicBattle.UI;
using UnityEngine;

namespace Mock.MusicBattle.Develop
{
    /// <summary>
    ///     敵のヘルスバーのデバッグ用コンポーネント。
    /// </summary>
    public class EnemyHealthBarDebugger : MonoBehaviour
    {
        [SerializeField]
        private float _maxHealth = 100f;

        [SerializeField]
        private float _damage = 10;

        [SerializeField]
        private IngameHUDManager _hud;

        private HealthEntity _healthEntity;

        private void Start()
        {
            _healthEntity = new HealthEntity(_maxHealth);
            _ = _hud.AddEnemyHealthBar(_healthEntity, transform);
        }

        [ContextMenu(nameof(ApplyHealthValue))]
        private void ApplyHealthValue()
        {
            _healthEntity.TakeDamage(_damage);
            _hud.ShowDamageText(_damage, transform.position);
        }
    }
}
