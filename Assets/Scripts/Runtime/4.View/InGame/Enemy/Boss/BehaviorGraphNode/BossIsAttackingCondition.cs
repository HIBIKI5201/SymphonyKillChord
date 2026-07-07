using System;
using Unity.Behavior;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.Enemy
{
    [Serializable, Unity.Properties.GeneratePropertyBag]
    [Condition(name: "BossIsAttacking", story: "攻撃中である(Boss) [Bool] [State]", category: "Conditions/Boss", id: "07b6c8d9eaeb716345ab78cd9aef1c67")]
    public partial class BossIsAttackingCondition : Unity.Behavior.Condition
    {
        [SerializeReference] public BlackboardVariable<BossStateFacade> State;
        [SerializeReference] public BlackboardVariable<bool> Bool;

        public override bool IsTrue()
        {
            if (State?.Value == null) return false;
            return State.Value.IsAttacking == Bool.Value;
        }

        public override void OnStart()
        {
        }

        public override void OnEnd()
        {
        }
    }
}
