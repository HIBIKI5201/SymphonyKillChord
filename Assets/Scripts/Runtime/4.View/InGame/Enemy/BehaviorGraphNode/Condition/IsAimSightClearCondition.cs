using System;
using Unity.Behavior;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.Enemy
{
    [Serializable, Unity.Properties.GeneratePropertyBag]
    [Condition(name: "IsAimSightClear", story: "攻撃目標との間に障害物がない [Bool] [State]", category: "Conditions", id: "fb7f79905d34791230533bf26dd2e567")]
    public partial class IsAimSightClearCondition : Condition
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