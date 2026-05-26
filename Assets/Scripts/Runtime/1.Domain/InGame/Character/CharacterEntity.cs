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
        /// <param name="attackInterval"></param>
        /// <param name="health"></param>
        /// <param name="combatSpec"></param>
        public CharacterEntity(CharacterName name,
            HealthEntity health,
            CharacterCombatSpec combatSpec,
            AttackInterval attackInterval
        )
        {
            if (health is null)
                throw new ArgumentNullException(nameof(health));
            if (combatSpec is null)
                throw new ArgumentNullException(nameof(combatSpec));

            _name = name;
            _health = health;
            _combatSpec = combatSpec;
            _attackIntervalEntity = new AttackIntervalEntity(attackInterval);
        }

        /// <summary>
        ///     HPに変化があった時に発火するイベント。<br/>
        ///     引数は、現在HP、最大HP、変化量（ダメージは負、回復は正）
        /// </summary>
        public event Action<float, float, float> OnHealthChanged;

        /// <summary> キャラクター死亡時に発火するイベント。 </summary>
        public event Action<CharacterEntity> OnDied;

        /// <summary> キャラクター名を取得する。 </summary>
        public CharacterName Name => _name;

        /// <summary> キャラクター固有のIDを取得する。 </summary>
        public Guid Id { get; } = Guid.NewGuid();

        /// <summary> コンバットスペックを取得する。 </summary>
        public CharacterCombatSpec CombatSpec => _combatSpec;

        /// <summary> 現在のHPを取得する。 </summary>
        public Health CurrentHealth => _health.CurrentHealth;

        /// <summary> 最大HPを取得する。 </summary>
        public Health MaxHealth => _health.MaxHealth;

        /// <summary> 死亡しているかどうかを取得する。 </summary>
        public bool IsDead => CurrentHealth.Value <= 0f;

        /// <summary> 無敵状態かどうかを示すプロパティ。 </summary>
        public bool IsInvincible => _isInvincible;

        /// <summary> 攻撃の硬直状態を管理するエンティティを取得する。 </summary>
        public AttackIntervalEntity AttackIntervalEntity => _attackIntervalEntity;

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

            float prevHealthValue = CurrentHealth.Value;
            float nextHealthValue = Math.Max(0, CurrentHealth.Value - damage.Value);
            Health nextHealth = new Health(nextHealthValue);
            _health.ChangeHealth(nextHealth);
            float amountChanged = _health.CurrentHealth.Value - prevHealthValue;
            OnHealthChanged?.Invoke(_health.CurrentHealth.Value, _health.MaxHealth.Value, amountChanged);

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
            float prevHealthValue = CurrentHealth.Value;
            Health nextHealth = new Health(CurrentHealth.Value + healAmount.Value);
            _health.ChangeHealth(nextHealth);
            float amountChanged = _health.CurrentHealth.Value - prevHealthValue;
            OnHealthChanged?.Invoke(_health.CurrentHealth.Value, _health.MaxHealth.Value, amountChanged);
        }

        /// <summary>
        ///     無敵状態を設定する。
        /// </summary>
        /// <param name="isInvincible"></param>
        public void SetInvincible(bool isInvincible)
        {
            _isInvincible = isInvincible;
        }

        /// <summary>
        ///     再初期化処理。
        /// </summary>
        public void Reset()
        {
            _health.ChangeHealth(new Health(_health.MaxHealth.Value));
            _isDeadNotified = false;
            _isInvincible = false;
        }

        private CharacterName _name;
        private HealthEntity _health;
        private CharacterCombatSpec _combatSpec;
        private AttackIntervalEntity _attackIntervalEntity;
        private bool _isDeadNotified;
        private bool _isInvincible;
    }
}