using KillChord.Runtime.View;
using System;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "IsStunned", story: "被弾硬直中 [Bool] [State]", category: "Conditions", id: "842f5b1b693cb1d6b1e5202aa4bcfccc")]
public partial class IsStunnedCondition : Condition
{
    [SerializeReference] public BlackboardVariable<EnemyStateFacade> State;
    [SerializeReference] public BlackboardVariable<bool> Bool;

    public override bool IsTrue()
    {
        return State.Value.IsStunned == Bool.Value;
    }
    public override void OnStart()
    {
    }

    public override void OnEnd()
    {
    }
}
