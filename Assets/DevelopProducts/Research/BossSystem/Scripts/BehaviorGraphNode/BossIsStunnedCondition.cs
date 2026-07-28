using System;
using Unity.Behavior;
using UnityEngine;

namespace DevelopProducts.Boss.BehaviorGraphNode
{
    [Serializable, Unity.Properties.GeneratePropertyBag]
    [Condition(name: "BossIsStunned", story: "dev_スタン状態である(Boss) [Bool] [State]", category: "Conditions/Boss", id: "f6a5b7c8e9da615234fa67bc89de0b56")]
    public partial class BossIsStunnedCondition : Unity.Behavior.Condition
    {
        [SerializeReference] public BlackboardVariable<BossStateFacade> State;
        [SerializeReference] public BlackboardVariable<bool> Bool;

        public override bool IsTrue()
        {
            if (State?.Value == null) return false;
            return State.Value.IsStunned == Bool.Value;
        }

        public override void OnStart()
        {
        }

        public override void OnEnd()
        {
        }
    }
}
