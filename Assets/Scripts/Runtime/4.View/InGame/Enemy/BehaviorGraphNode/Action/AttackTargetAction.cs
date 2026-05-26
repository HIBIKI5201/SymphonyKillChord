using KillChord.Runtime.View.InGame.Enemy.AIFacade;
using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.Enemy.BehaviorGraphNode.Action
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(name: "AttackTarget", story: "対象を攻撃する [Battle] [State]", category: "Action", id: "611c230a6a1f2c1d944d9d2cf1c3a297")]
    public partial class AttackTargetAction : Unity.Behavior.Action
    {
        [SerializeReference] public BlackboardVariable<EnemyBattleAIFacade> Battle;
        [SerializeReference] public BlackboardVariable<EnemyStateFacade> State;

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
