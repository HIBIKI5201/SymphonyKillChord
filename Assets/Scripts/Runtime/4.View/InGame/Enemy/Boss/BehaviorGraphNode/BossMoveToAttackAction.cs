using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.Enemy
{
    /// <summary>
    ///     ボスを攻撃できる位置へ移動させる Behavior Graph のアクションノード。
    /// </summary>
    [Serializable, GeneratePropertyBag]
    [NodeDescription(name: "BossMoveToAttack", story: "攻撃可能な位置まで移動する(Boss) [Movement] [State]", category: "Action/Boss", id: "b2e1d3c4f5a6471890bc23de45fa6712")]
    public partial class BossMoveToAttackAction : Unity.Behavior.Action
    {
        [SerializeReference] public BlackboardVariable<BossMovementAIFacade> Movement;
        [SerializeReference] public BlackboardVariable<BossStateFacade> State;

        /// <summary>
        ///     攻撃位置への移動を開始する。参照が無い場合は失敗を返す。
        /// </summary>
        protected override Unity.Behavior.Node.Status OnStart()
        {
            if (Movement?.Value == null || State?.Value == null) return Unity.Behavior.Node.Status.Failure;
            Movement.Value.MoveToAttack();
            return Unity.Behavior.Node.Status.Running;
        }

        /// <summary>
        ///     射程内かつ射線が通ったら成功を返し、それまでは実行中を返す。
        /// </summary>
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

        /// <summary>
        ///     ノード終了時の処理。現在は何もしない。
        /// </summary>
        protected override void OnEnd()
        {
        }
    }
}
