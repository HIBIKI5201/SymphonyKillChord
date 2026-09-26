using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.Enemy
{
    /// <summary>
    ///     ボスの移動を止める Behavior Graph のアクションノード。
    /// </summary>
    [Serializable, GeneratePropertyBag]
    [NodeDescription(name: "BossStopMoving", story: "移動を停止する(Boss) [Movement] [State]", category: "Action/Boss", id: "c3d2e4b5a6f7482901cd34ef56ab7823")]
    public partial class BossStopMovingAction : Unity.Behavior.Action
    {
        [SerializeReference] public BlackboardVariable<BossMovementAIFacade> Movement;
        [SerializeReference] public BlackboardVariable<BossStateFacade> State;

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
