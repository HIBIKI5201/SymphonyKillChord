using KillChord.Runtime.View.InGame.Enemy.AIFacade;
using System;
using Unity.Behavior;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.Enemy.BehaviorGraphNode.Condition
{
    [Serializable, Unity.Properties.GeneratePropertyBag]
    [Condition(name: "IsDiscovered", story: "プレイヤーを発見済みである [Bool] [State]", category: "Conditions", id: "b3f8a1d4e2c9407fa6b1c8d3e5f70a92")]
    public partial class IsDiscoveredCondition : Unity.Behavior.Condition
    {
        [SerializeReference] public BlackboardVariable<EnemyStateFacade> State;
        [SerializeReference] public BlackboardVariable<bool> Bool;

        public override bool IsTrue()
        {
            if (State?.Value == null) return false;
            return State.Value.IsDiscovered == Bool.Value;
        }

        public override void OnStart()
        {
        }

        public override void OnEnd()
        {
        }
    }
}
