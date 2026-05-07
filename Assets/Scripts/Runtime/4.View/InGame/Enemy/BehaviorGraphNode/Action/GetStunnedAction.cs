using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

namespace KillChord.Runtime.View.InGame.Enemy
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(name: "GetStunned", story: "被弾硬直を開始する [State] [Battle]", category: "Action", id: "459e141cce9d40aaaebad1a7c2283299")]
    public partial class GetStunnedAction : Action
    {
        [SerializeReference] public BlackboardVariable<EnemyStateFacade> State;
        [SerializeReference] public BlackboardVariable<EnemyBattleAIFacade> Battle;

        protected override Status OnStart()
        {
            if (State?.Value?.gameObject == null
                || Battle?.Value == null) return Status.Failure;

            GameObject agent = State.Value.gameObject;
            EnemyStateFacade state = State.Value;
            EnemyBattleAIFacade battle = Battle.Value;

            battle.CancelAttack();
            state.Stunned();
            return Status.Running;
        }

        protected override Status OnUpdate()
        {
            if (State.Value.IsStunned) return Status.Running;
            return Status.Success;
        }

        protected override void OnEnd()
        {
        }
    }
}