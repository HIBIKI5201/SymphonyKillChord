namespace KillChord.Runtime.Domain.InGame.Battle
{
    /// <summary>
    ///     攻撃の基本情報を保持するクラス。
    ///     攻撃の種類や基本ダメージなど、攻撃に関する定数的な情報を管理するためのクラス。
    /// </summary>
    public class AttackDefinition
    {
        /// <summary>
        ///     コンストラクタ。
        /// </summary>
        public AttackDefinition(string attackName,
            Damage baseDamage,
            AttackParameterSet attackParameterSet,
            IAttackPipeline attackPipeline)
        {
            AttackName = attackName;
            BaseDamage = baseDamage;
            AttackParameterSet = attackParameterSet;
            AttackPipeline = attackPipeline;
        }

        public string AttackName { get; }
        public Damage BaseDamage { get; }
        public AttackParameterSet AttackParameterSet { get; }
        public IAttackPipeline AttackPipeline { get; }
    }
}
