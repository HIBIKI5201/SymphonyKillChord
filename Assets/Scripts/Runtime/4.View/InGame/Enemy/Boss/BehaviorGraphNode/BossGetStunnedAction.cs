using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.Enemy
{
    /// <summary>
    ///     ボスの硬直が解けるまで待機させる Behavior Graph のアクションノード。
    /// </summary>
    [Serializable, GeneratePropertyBag]
    [NodeDescription(name: "BossGetStunned", story: "スタン状態を開始する(Boss) [State] [Battle]", category: "Action/Boss", id: "d4c3f5a6b7e8493012de45fa67bc8934")]
    public partial class BossGetStunnedAction : Unity.Behavior.Action
    {
        [SerializeReference] public BlackboardVariable<BossStateFacade> State;
        [SerializeReference] public BlackboardVariable<BossBattleAIFacade> Battle;

        /// <summary>
        ///     参照を確認し、硬直の処理を開始する。参照が無い場合は失敗を返す。
        /// </summary>
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

        /// <summary>
        ///     硬直中は実行中を返し、解除されたら成功を返す。
        /// </summary>
        protected override Unity.Behavior.Node.Status OnUpdate()
        {
            if (State.Value.IsStunned) return Unity.Behavior.Node.Status.Running;
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
