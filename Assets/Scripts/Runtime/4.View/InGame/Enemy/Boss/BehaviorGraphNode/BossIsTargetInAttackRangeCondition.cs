using System;
using Unity.Behavior;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.Enemy
{
    [Serializable, Unity.Properties.GeneratePropertyBag]
    [Condition(name: "BossIsTargetInAttackRange", story: "敵が攻撃範囲内にいる(Boss) [Bool] [State]", category: "Conditions/Boss", id: "e5b4a6c7d8f9504123ef56ab78cd9a45")]
    public partial class BossIsTargetInAttackRangeCondition : Unity.Behavior.Condition
    {
        [SerializeReference] public BlackboardVariable<BossStateFacade> State;
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
