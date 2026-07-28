using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;

namespace DevelopProducts.Boss.BehaviorGraphNode
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(name: "BossMoveToAttack", story: "dev_攻撃可能な位置まで移動する(Boss) [Movement] [State]", category: "Action/Boss", id: "b2e1d3c4f5a6471890bc23de45fa6712")]
    public partial class BossMoveToAttackAction : Unity.Behavior.Action
    {
        [SerializeReference] public BlackboardVariable<BossMovementAIFacade> Movement;
        [SerializeReference] public BlackboardVariable<BossStateFacade> State;

        protected override Unity.Behavior.Node.Status OnStart()
        {
            if (Movement?.Value == null || State?.Value == null) return Unity.Behavior.Node.Status.Failure;
            Movement.Value.MoveToAttack();
            return Unity.Behavior.Node.Status.Running;
        }

        protected override Unity.Behavior.Node.Status OnUpdate()
        {
            BossStateFacade stateFacade = State.Value;
            if (stateFacade.IsTargetInAttackRange && stateFacade.IsSightClearToAim)
            {
                return Unity.Behavior.Node.Status.Success;
            }

            Movement.Value.MoveToAttack();
            return Unity.Behavior.Node.Status.Running;
        }

        protected override void OnEnd()
        {
        }
    }
}
