using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.Enemy
{
    /// <summary>
    ///     ボスにターゲットへの攻撃を行わせる Behavior Graph のアクションノード。
    /// </summary>
    [Serializable, GeneratePropertyBag]
    [NodeDescription(name: "BossAttackTarget", story: "対象を攻撃する(Boss) [Battle] [State]", category: "Action/Boss", id: "a1f0c2d3e4b5460789ab12cd34ef5601")]
    public partial class BossAttackTargetAction : Unity.Behavior.Action
    {
        [SerializeReference] public BlackboardVariable<BossBattleAIFacade> Battle;
        [SerializeReference] public BlackboardVariable<BossStateFacade> State;

        /// <summary>
        ///     攻撃を開始する。参照が無い場合は失敗を返す。
        /// </summary>
        protected override Unity.Behavior.Node.Status OnStart()
        {
            if (Battle?.Value == null || State?.Value == null) return Unity.Behavior.Node.Status.Failure;
            Battle.Value.StartAttack();
            return Unity.Behavior.Node.Status.Running;
        }

        /// <summary>
        ///     攻撃中は実行中を返し、攻撃が終わったら成功を返す。
        /// </summary>
        protected override Unity.Behavior.Node.Status OnUpdate()
        {
            return State.Value.IsAttacking
                ? Unity.Behavior.Node.Status.Running
                : Unity.Behavior.Node.Status.Success;
        }

        /// <summary>
        ///     ノード終了時の処理。現在は何もしない。
        /// </summary>
        protected override void OnEnd()
        {
        }
    }
}
