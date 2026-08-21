using KillChord.Runtime.View.InGame.Enemy.AIFacade;
using System;
using Unity.Behavior;
using UnityEngine;
namespace KillChord.Runtime.View.InGame.Enemy.BehaviorGraphNode.Condition
{
    [Serializable, Unity.Properties.GeneratePropertyBag]
    [Condition(name: "IsBattleAiActivated", story: "敵の戦闘AIが有効 [Bool] [State]", category: "Conditions", id: "e481ef1704416ac2b10bcdd0903055fe")]
    public partial class IsBattleAiActivatedCondition : Unity.Behavior.Condition
    {
        [SerializeReference] public BlackboardVariable<EnemyStateFacade> State;
        [SerializeReference] public BlackboardVariable<bool> Bool;


        public override bool IsTrue()
        {
            return State.Value.IsBattleAiActivated == Bool.Value;
        }

        public override void OnStart()
        {
        }

        public override void OnEnd()
        {
        }
    }
}