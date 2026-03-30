using KillChord.Runtime.Adaptor.InGame.Battle;
using KillChord.Runtime.Domain.InGame.Character;
using LitMotion;
using LitMotion.Extensions;
using UnityEngine;

namespace KillChord.Develop
{
    public class EnemySample : MonoBehaviour, IDamageable, IViewModelDamage
    {
        [SerializeField] private Transform _view;
        public BattleController BattleController => _battleController;

        public void Init(BattleController battleController)
        {
            _battleController = battleController;
        }

        public void OnDamage(DamageDTO dto)
        {
            if (_view == null)
            {
                Debug.Log("_viewがnullなので早期returnしました。", this);
                return;
            }
            if (_currentHealth.Value > dto.CurrentHealth.Value)
            {
                _handle.TryCancel();
                _handle = LSequence.Create()
                    .Join(LMotion.Punch.Create(0f, 0.2f, 0.2f)
                        .WithFrequency(Random.Range(5, 10))
                        .BindToLocalPositionX(_view))
                    .Join(LMotion.Punch.Create(0f, 0.2f, 0.2f)
                        .WithFrequency(Random.Range(5, 10))
                        .BindToLocalPositionY(_view))
                    .Join(LMotion.Punch.Create(0f, 0.2f, 0.2f)
                        .WithFrequency(Random.Range(5, 10))
                        .BindToLocalPositionZ(_view))
                    .Run();
            }
            _currentHealth = dto.CurrentHealth;
        }
        private void Awake()
        {
            Debug.Assert(_view != null, "_view is not assigned. Please assign a view transform to enable damage feedback.", this);
        }
        private void OnDestroy()
        {
            _handle.TryCancel();
        }

        private MotionHandle _handle;
        private Health _currentHealth;
        private BattleController _battleController;
    }
}