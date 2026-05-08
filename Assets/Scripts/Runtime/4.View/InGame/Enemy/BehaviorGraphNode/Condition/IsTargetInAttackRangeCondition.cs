using KillChord.Runtime.View.InGame.Enemy.AIFacade;
using System;
using Unity.Behavior;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.Enemy.BehaviorGraphNode.Condition
{
    [Serializable, Unity.Properties.GeneratePropertyBag]
    [Condition(name: "IsTargetInAttackRange", story: "謾ｻ謦・岼讓吶′謾ｻ謦・ｯ・峇蜀・[Bool] [State]", category: "Conditions", id: "f089200575131990cf77ee4ef830d114")]
    public partial class IsTargetInAttackRangeCondition : Unity.Behavior.Condition
    {
        [SerializeReference] public BlackboardVariable<EnemyStateFacade> State;
        [SerializeReference] public BlackboardVariable<bool> Bool;

        public override bool IsTrue()
        {
            if (State?.Value == null) return false;
            return State.Value.IsTargetInAttackRange == Bool.Value;
        }

        public override void OnStart()
        {
        }

        public override void OnEnd()
        {
        }
    }
}
