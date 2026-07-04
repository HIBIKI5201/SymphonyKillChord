using System;
using Unity.Behavior;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.Enemy
{
    [Serializable, Unity.Properties.GeneratePropertyBag]
    [Condition(name: "BossIsAimSightClear", story: "敵との間に遮蔽物がない(Boss) [Bool] [State]", category: "Conditions/Boss", id: "18c7d9eafbfc827456bc89dea0fb2d78")]
    public partial class BossIsAimSightClearCondition : Unity.Behavior.Condition
    {
        [SerializeReference] public BlackboardVariable<BossStateFacade> State;
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
