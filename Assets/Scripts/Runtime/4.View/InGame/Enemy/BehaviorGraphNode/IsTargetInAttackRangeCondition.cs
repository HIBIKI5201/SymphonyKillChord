using KillChord.Runtime.View;
using System;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "IsTargetInAttackRange", story: "攻撃目標が攻撃範囲内 [Bool] [State]", category: "Conditions", id: "f089200575131990cf77ee4ef830d114")]
public partial class IsTargetInAttackRangeCondition : Condition
{
    [SerializeReference] public BlackboardVariable<EnemyStateFacade> State;
    [SerializeReference] public BlackboardVariable<bool> Bool;

    public override bool IsTrue()
    {
        return State.Value.IsTargetInAttackRange == Bool.Value;
    }

    public override void OnStart()
    {
    }

    public override void OnEnd()
    {
    }
}
