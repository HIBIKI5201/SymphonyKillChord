using KillChord.Runtime.View.InGame.Enemy.AIFacade;
using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.Enemy.BehaviorGraphNode.Action
{
    /// <summary>
    ///     敵を攻撃できる位置へ移動させる Behavior Graph のアクションノード。
    /// </summary>
    [Serializable, GeneratePropertyBag]
    [NodeDescription(name: "MoveToAttack", story: "攻撃可能な位置まで移動する [Movement] [State]", category: "Action", id: "8b82e763f6fed498af18c3983a2c822b")]
    public partial class MoveToAttackAction : Unity.Behavior.Action
    {
        [SerializeReference] public BlackboardVariable<EnemyMovementAIFacade> Movement;
        [SerializeReference] public BlackboardVariable<EnemyStateFacade> State;

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
            EnemyStateFacade stateFacade = State.Value;
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
