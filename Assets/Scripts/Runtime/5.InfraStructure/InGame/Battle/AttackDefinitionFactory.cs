using KillChord.Runtime.Domain.InGame.Battle;
using KillChord.Runtime.Domain.InGame.Character;

namespace KillChord.Runtime.InfraStructure
{
    public static class AttackDefinitionFactory
    {
        public static AttackDefinition Create(AttackDefinitionData data)
        {
            if (data == null)
            {
                throw new System.ArgumentNullException(nameof(data));
            }
            if (data.AttackParameterSetData == null)
            {
                throw new System.ArgumentNullException(nameof(data.AttackParameterSetData));
            }
            if (data.AttackPipelineAsset == null)
            {
                throw new System.ArgumentNullException(nameof(data.AttackPipelineAsset));
            }

            AttackParameterSet attackParameterSet = new AttackParameterSet(
                new CriticalChance(data.AttackParameterSetData.CriticalChance),
                new CriticalMultiplier(data.AttackParameterSetData.CriticalDamageMultiplier),
                new Damage(data.AttackParameterSetData.ConfirmedDamage)
                );

            return new AttackDefinition(
                data.AttackName,
                new Damage(data.BaseDamage),
                attackParameterSet,
                data.AttackPipelineAsset.Create()
                );
        }
    }
}
