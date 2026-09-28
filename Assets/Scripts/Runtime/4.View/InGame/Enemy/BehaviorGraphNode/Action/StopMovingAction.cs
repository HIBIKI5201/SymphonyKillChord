using KillChord.Runtime.View.InGame.Enemy.AIFacade;
using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.Enemy.BehaviorGraphNode.Action
{
    /// <summary>
    ///     敵の移動を止める Behavior Graph のアクションノード。
    /// </summary>
    [Serializable, GeneratePropertyBag]
    [NodeDescription(name: "StopMoving", story: "移動を停止する [Movement] [State]", category: "Action", id: "72e36c342c9233772b0a01e15cd5b846")]
    public partial class StopMovingAction : Unity.Behavior.Action
    {
        [SerializeReference] public BlackboardVariable<EnemyMovementAIFacade> Movement;
        [SerializeReference] public BlackboardVariable<EnemyStateFacade> State;

        /// <summary>
        ///     移動を止めて成功を返す。参照が無い場合は失敗を返す。
        /// </summary>
        protected override Unity.Behavior.Node.Status OnStart()
        {
            if (Movement?.Value == null || State?.Value == null) return Unity.Behavior.Node.Status.Failure;
            Movement.Value.StopMoving();
            return Unity.Behavior.Node.Status.Success;
        }

        /// <summary>
        ///     常に成功を返す。
        /// </summary>
        protected override Unity.Behavior.Node.Status OnUpdate()
        {
            return Unity.Behavior.Node.Status.Success;
        }

        /// <summary>
        ///     ノード終了時の処理。現在は何もしない。
        /// </summary>
        protected override void OnEnd()
        {
        }
    }
}
