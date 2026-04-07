using KillChord.Runtime.Domain.InGame.Character;

namespace KillChord.Runtime.Domain.InGame.Battle
{
    public readonly struct AttackParameterSet
    {
        public AttackParameterSet(
            CriticalChance criticalChance,
            CriticalDamage criticalDamage,
            Damage confirmedDamage
            )
        {
            CriticalChance = criticalChance;
            CriticalDamage = criticalDamage;
            ConfirmedDamage = confirmedDamage;
        }

        public CriticalChance CriticalChance { get; }
        public CriticalDamage CriticalDamage { get; }
        public Damage ConfirmedDamage { get; }
    }
}
