namespace KillChord.Runtime.Domain.InGame.Battle
{
    /// <summary>
    ///     攻撃段階のコンテキストを表す構造体。
    /// </summary>
    public readonly ref struct AttackStepContext
    {
        public AttackStepContext(AttackDefinition attackDefinition, IAttacker attacker, IDefender defender)
        {
            _attackDefinition = attackDefinition;
            _damage = attackDefinition.BaseDamage;
            _criticalCount = 0;
            _attacker = attacker;
            _defender = defender;
        }

        public AttackStepContext(Damage damage, int criticalCount, in AttackStepContext attackStepContext)
        {
            _attackDefinition = attackStepContext._attackDefinition;
            _damage = damage;
            _criticalCount = criticalCount;
            _attacker = attackStepContext._attacker;
            _defender = attackStepContext._defender;
        }

        public AttackDefinition AttackDefinition => _attackDefinition;
        public Damage Damage => _damage;
        public int CriticalCount => _criticalCount;
        public IAttacker Attacker => _attacker;
        public IDefender Defender => _defender;

        private readonly AttackDefinition _attackDefinition;
        private readonly Damage _damage;
        private readonly int _criticalCount;
        private readonly IAttacker _attacker;
        private readonly IDefender _defender;
    }
}
