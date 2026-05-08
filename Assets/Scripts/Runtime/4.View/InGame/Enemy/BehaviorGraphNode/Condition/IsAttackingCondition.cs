using KillChord.Runtime.View.InGame.Enemy.AIFacade;
using System;
using Unity.Behavior;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.Enemy.BehaviorGraphNode.Condition
{
    [Serializable, Unity.Properties.GeneratePropertyBag]
    [Condition(name: "IsAttacking", story: "謾ｻ謦・ｸｭ [Bool] [State] ", category: "Conditions", id: "aaddd2fea642038ded13432c6882f4bd")]
    public partial class IsAttackingCondition : Unity.Behavior.Condition
    {
        [SerializeReference] public BlackboardVariable<EnemyStateFacade> State;
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
