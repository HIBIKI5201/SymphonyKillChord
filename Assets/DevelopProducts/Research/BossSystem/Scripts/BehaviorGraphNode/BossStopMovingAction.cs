using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;

namespace DevelopProducts.Boss.BehaviorGraphNode
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(name: "BossStopMoving", story: "dev_移動を停止する(Boss) [Movement] [State]", category: "Action/Boss", id: "c3d2e4b5a6f7482901cd34ef56ab7823")]
    public partial class BossStopMovingAction : Unity.Behavior.Action
    {
        [SerializeReference] public BlackboardVariable<BossMovementAIFacade> Movement;
        [SerializeReference] public BlackboardVariable<BossStateFacade> State;

        protected override Unity.Behavior.Node.Status OnStart()
        {
            if (Movement?.Value == null || State?.Value == null) return Unity.Behavior.Node.Status.Failure;
            Movement.Value.StopMoving();
            return Unity.Behavior.Node.Status.Success;
        }

        protected override Unity.Behavior.Node.Status OnUpdate()
        {
            return Unity.Behavior.Node.Status.Success;
        }

        protected override void OnEnd()
        {
        }
    }
}
