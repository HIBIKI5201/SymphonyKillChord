using KillChord.Runtime.Domain.InGame.Battle;
using System;

namespace KillChord.Runtime.Domain.InGame.Character
{
    /// <summary>
    ///     キャラクターの基本的な情報を保持するクラス。
    /// </summary>
    public class CharacterEntity : IAttacker, IDefender
    {
        /// <summary>
        ///     コンストラクタ。
        /// </summary>
        /// <param name="name"></param>
        /// <param name="health"></param>
        /// <param name="moveSpeed"></param>
        /// <param name="attackPower"></param>
        /// <param name="combatSpec"></param>
        public CharacterEntity(CharacterName name,
            HealthEntity health,
            CharacterCombatSpec combatSpec
            )
        {
            if (health is null)
                throw new ArgumentNullException(nameof(health));
            if (combatSpec is null)
                throw new ArgumentNullException(nameof(combatSpec));

            _name = name;
            _health = health;
            _combatSpec = combatSpec;
        }

        public event Action<CharacterEntity> OnDied;

        public CharacterName Name => _name;
        public Guid Id { get; } = Guid.NewGuid();
        public CharacterCombatSpec CombatSpec => _combatSpec;
        public Health CurrentHealth => _health.CurrentHealth;
        public Health MaxHealth => _health.MaxHealth;
        public bool IsDead => CurrentHealth.Value <= 0f;
        /// <summary>
        ///     無敵状態かどうかを示すプロパティ。回避以外にもあるかもなので、変数は無敵にした。
        /// </summary>
        public bool IsInvincible => _isInvincible;

        /// <summary>
        ///     ダメージを受ける処理。
        ///     HealthEntityのChangeHealthを呼び出す。
        /// </summary>
        /// <param name="damage"></param>
        public void TakeDamage(Damage damage)
        {
            if (IsDead)
            {
                return;
            }

            if (_isInvincible)
            {
                return;
            }

            float nextHealthValue = Math.Max(0, CurrentHealth.Value - damage.Value);
            Health nextHealth = new Health(nextHealthValue);
            _health.ChangeHealth(nextHealth);

            if (CurrentHealth.Value <= 0f && !_isDeadNotified)
            {
                _isDeadNotified = true;
                OnDied?.Invoke(this);
            }
        }

        /// <summary>
        ///     HPを回復する処理。
        /// </summary>
        /// <param name="healAmount"></param>
        public void Heal(Health healAmount)
        {
            Health nextHealth = new Health(CurrentHealth.Value + healAmount.Value);
            _health.ChangeHealth(nextHealth);
        }

        public void SetInvincible(bool isInvincible)
        {
            _isInvincible = isInvincible;
        }

        private CharacterName _name;
        private HealthEntity _health;
        private CharacterCombatSpec _combatSpec;
        private bool _isDeadNotified = false;
        private bool _isInvincible = false;
    }
}
