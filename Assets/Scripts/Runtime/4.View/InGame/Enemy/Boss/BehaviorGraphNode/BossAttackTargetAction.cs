using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.Enemy
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(name: "BossAttackTarget", story: "対象を攻撃する(Boss) [Battle] [State]", category: "Action/Boss", id: "a1f0c2d3e4b5460789ab12cd34ef5601")]
    public partial class BossAttackTargetAction : Unity.Behavior.Action
    {
        [SerializeReference] public BlackboardVariable<BossBattleAIFacade> Battle;
        [SerializeReference] public BlackboardVariable<BossStateFacade> State;

        protected override Unity.Behavior.Node.Status OnStart()
        {
            if (Battle?.Value == null || State?.Value == null) return Unity.Behavior.Node.Status.Failure;
            Battle.Value.StartAttack();
            return Unity.Behavior.Node.Status.Running;
        }

        protected override Unity.Behavior.Node.Status OnUpdate()
        {
            return State.Value.IsAttacking
                ? Unity.Behavior.Node.Status.Running
                : Unity.Behavior.Node.Status.Success;
        }

        protected override void OnEnd()
        {
        }
    }
}
