using KillChord.Runtime.Domain.InGame.Battle;
using UnityEngine;

namespace KillChord.Runtime.Application.InGame.Battle
{
    public static class AttackCalculator
    {
        public static AttackResult Calculate(
            AttackDefinition attackDefinition,
            IAttacker attacker,
            IDefender defender)
        {
            AttackStepContext stepContext = new AttackStepContext(attackDefinition, attacker, defender);
            return attackDefinition.AttackPipeline.Execute(stepContext);
        }
    }
}
