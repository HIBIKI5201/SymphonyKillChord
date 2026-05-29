using KillChord.Runtime.View.InGame.Enemy.AIFacade;
using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.Enemy.BehaviorGraphNode.Action
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(name: "GetStunned", story: "スタン状態を開始する [State] [Battle]", category: "Action", id: "459e141cce9d40aaaebad1a7c2283299")]
    public partial class GetStunnedAction : Unity.Behavior.Action
    {
        [SerializeReference] public BlackboardVariable<EnemyStateFacade> State;
        [SerializeReference] public BlackboardVariable<EnemyBattleAIFacade> Battle;

        protected override Unity.Behavior.Node.Status OnStart()
        {
            if (State?.Value?.gameObject == null || Battle?.Value == null)
            {
                return Unity.Behavior.Node.Status.Failure;
            }

            EnemyStateFacade state = State.Value;
            EnemyBattleAIFacade battle = Battle.Value;
            battle.CancelAttack();
            state.Stunned();
            return Unity.Behavior.Node.Status.Running;
        }

        protected override Unity.Behavior.Node.Status OnUpdate()
        {
            if (State.Value.IsStunned) return Unity.Behavior.Node.Status.Running;
            return Unity.Behavior.Node.Status.Success;
        }

        protected override void OnEnd()
        {
        }
    }
}
