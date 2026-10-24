using KillChord.Runtime.Adaptor.InGame.Enemy.EnemyAIFacadeInterface;
using KillChord.Runtime.View.InGame.Enemy.AIFacade;
using System;
using Unity.Behavior;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.Enemy.BehaviorGraphNode.Action
{
    /// <summary>
    ///     攻撃できる位置へ移動させる、敵・ボス共通のアクションノードの基底。
    /// </summary>
    /// <typeparam name="TMovement"> Blackboard に置く移動ファサードの型。 </typeparam>
    /// <typeparam name="TState"> Blackboard に置く状態ファサードの型。 </typeparam>
    [Serializable]
    public abstract class MoveToAttackActionBase<TMovement, TState> : Unity.Behavior.Action
        where TMovement : MonoBehaviour, IEnemyMovementAIFacade
        where TState : StateFacadeBase
    {
        [SerializeReference] public BlackboardVariable<TMovement> Movement;
        [SerializeReference] public BlackboardVariable<TState> State;

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
            TState stateFacade = State.Value;
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
