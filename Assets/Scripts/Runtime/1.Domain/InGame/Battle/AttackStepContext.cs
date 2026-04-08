namespace KillChord.Runtime.Domain.InGame.Battle
{
    /// <summary>
    ///     攻撃段階のコンテキストを表す構造体。
    /// </summary>
    public readonly ref struct AttackStepContext
    {
        /// <summary>
        ///     攻撃段階のコンテキストを初期化するコンストラクタ。
        ///     値を直接指定して初期化するためのコンストラクタ。
        /// </summary>
        /// <param name="attackDefinition"></param>
        /// <param name="attacker"></param>
        /// <param name="defender"></param>
        public AttackStepContext(AttackDefinition attackDefinition, IAttacker attacker, IDefender defender)
        {
            _attackDefinition = attackDefinition;
            _damage = attackDefinition.BaseDamage;
            _criticalCount = 0;
            _attacker = attacker;
            _defender = defender;
        }

        /// <summary>
        ///     攻撃段階のコンテキストを初期化するコンストラクタ。
        ///     同じ攻撃段階の前のコンテキストから値を抽出して初期化するためのコンストラクタ。
        /// </summary>
        /// <param name="damage"></param>
        /// <param name="criticalCount"></param>
        /// <param name="attackStepContext"></param>
        public AttackStepContext(Damage damage, int criticalCount, in AttackStepContext attackStepContext)
        {
            _attackDefinition = attackStepContext._attackDefinition;
            _damage = damage;
            _criticalCount = criticalCount;
            _attacker = attackStepContext._attacker;
            _defender = attackStepContext._defender;
        }

        /// <summary> 攻撃定義を取得するプロパティ。 </summary>
        public AttackDefinition AttackDefinition => _attackDefinition;
        /// <summary> ダメージ量を取得するプロパティ。 </summary>
        public Damage Damage => _damage;
        /// <summary> クリティカルヒットの回数を取得するプロパティ。 </summary>
        public int CriticalCount => _criticalCount;
        /// <summary> 攻撃者を取得するプロパティ。 </summary>
        public IAttacker Attacker => _attacker;
        /// <summary> 防御者を取得するプロパティ。 </summary>
        public IDefender Defender => _defender;

        private readonly AttackDefinition _attackDefinition;
        private readonly Damage _damage;
        private readonly int _criticalCount;
        private readonly IAttacker _attacker;
        private readonly IDefender _defender;
    }
}
