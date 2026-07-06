using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;

namespace DevelopProducts.Boss.BehaviorGraphNode
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(name: "BossGetStunned", story: "dev_スタン状態を開始する(Boss) [State] [Battle]", category: "Action/Boss", id: "d4c3f5a6b7e8493012de45fa67bc8934")]
    public partial class BossGetStunnedAction : Unity.Behavior.Action
    {
        [SerializeReference] public BlackboardVariable<BossStateFacade> State;
        [SerializeReference] public BlackboardVariable<BossBattleAIFacade> Battle;

        protected override Unity.Behavior.Node.Status OnStart()
        {
            if (State?.Value?.gameObject == null || Battle?.Value == null)
            {
                return Unity.Behavior.Node.Status.Failure;
            }

            BossStateFacade state = State.Value;
            BossBattleAIFacade battle = Battle.Value;
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
