using DevelopProducts.BehaviorGraph.Runtime.Application.InGame.Battle;
using DevelopProducts.BehaviorGraph.Runtime.Domain.InGame.Battle;
using DevelopProducts.BehaviorGraph.Runtime.Domain.InGame.Character;
using KillChord.Runtime.InfraStructure;
namespace DevelopProducts.BehaviorGraph.Runtime.InfraStructure
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

            int? beatType = data.UseBeatType ? data.BeatType : null;

            AttackPipeline pipeline = new AttackPipeline();

            return new AttackDefinition(
                data.AttackName,
                new Damage(data.BaseDamage),
                attackParameterSet,
                pipeline,
                beatType
                );
        }
    }
}
