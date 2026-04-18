using KillChord.Runtime.View;
using System;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "IsAttacking", story: "Agent [State] Is Attacking", category: "Conditions", id: "aaddd2fea642038ded13432c6882f4bd")]
public partial class IsAttackingCondition : Condition
{
    [SerializeReference] public BlackboardVariable<EnemyStateFacade> State;

    public override bool IsTrue()
    {
        return State.Value.IsAttacking;
    }

    public override void OnStart()
    {
    }

    public override void OnEnd()
    {
    }
}
