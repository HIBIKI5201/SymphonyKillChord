using KillChord.Runtime.View.InGame.Enemy.AIFacade;
using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.Enemy.BehaviorGraphNode.Action
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(name: "StopMoving", story: "遘ｻ蜍輔ｒ蛛懈ｭ｢縺吶ｋ [Movement] [State]", category: "Action", id: "72e36c342c9233772b0a01e15cd5b846")]
    public partial class StopMovingAction : Unity.Behavior.Action
    {
        [SerializeReference] public BlackboardVariable<EnemyMovementAIFacade> Movement;
        [SerializeReference] public BlackboardVariable<EnemyStateFacade> State;

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
