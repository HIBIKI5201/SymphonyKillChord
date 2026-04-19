using KillChord.Runtime.View;
using System;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "IsAttacking", story: "攻撃中 [Bool] [State] ", category: "Conditions", id: "aaddd2fea642038ded13432c6882f4bd")]
public partial class IsAttackingCondition : Condition
{
    [SerializeReference] public BlackboardVariable<EnemyStateFacade> State;
    [SerializeReference] public BlackboardVariable<bool> Bool;

    public override bool IsTrue()
    {
        return State.Value.IsAttacking == Bool.Value;
    }

    public override void OnStart()
    {
    }

    public override void OnEnd()
    {
    }
}
