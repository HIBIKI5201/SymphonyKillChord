using KillChord.Runtime.Adaptor.InGame.Enemy;
using KillChord.Runtime.Adaptor.InGame.UI;
using KillChord.Runtime.View.InGame.UI;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.Enemy
{
    /// <summary>
    ///     敵のHP表示関連View。
    /// </summary>
    public class EnemyHealthView : MonoBehaviour, IDamageNumber
    {
        public void Initialize(IHealthHudPresenter presenter)
        {
            _presenter = presenter;
        }

        public void Bind(IHealthHudViewModel viewModel)
        {
            if (_healthHudView == null)
            {
                Debug.LogError("[EnemyHealthView] HealthHudViewの参照がありません。", this);
                return;
            }

            _healthHudView.Bind(viewModel);
        }

        public void ShowDamage(float damage)
        {
            DamageNumberView view = Instantiate(_damageNumberPrefab, _damageNumberSpawnPoint);
            view.Play(damage);
        }

        private void OnDestroy()
        {
            _presenter?.Dispose();
        }

        [SerializeField] private HealthHudView _healthHudView;
        [SerializeField] private Transform _damageNumberSpawnPoint;
        [SerializeField] private DamageNumberView _damageNumberPrefab;
        private IHealthHudPresenter _presenter;
    }
}
