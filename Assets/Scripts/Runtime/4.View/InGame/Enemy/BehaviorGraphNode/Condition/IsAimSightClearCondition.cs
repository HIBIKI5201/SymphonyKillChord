using KillChord.Runtime.View.InGame.Enemy.AIFacade;
using System;
using Unity.Behavior;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.Enemy.BehaviorGraphNode.Condition
{
    [Serializable, Unity.Properties.GeneratePropertyBag]
    [Condition(name: "IsAimSightClear", story: "敵との間に遮蔽物がない [Bool] [State]", category: "Conditions", id: "fb7f79905d34791230533bf26dd2e567")]
    public partial class IsAimSightClearCondition : Unity.Behavior.Condition
    {
        [SerializeReference] public BlackboardVariable<EnemyStateFacade> State;
        [SerializeReference] public BlackboardVariable<bool> Bool;

        public override bool IsTrue()
        {
            if (State?.Value == null) return false;
            return State.Value.IsSightClearToAim == Bool.Value;
        }

        public override void OnStart()
        {
        }

        public override void OnEnd()
        {
        }
    }
}
