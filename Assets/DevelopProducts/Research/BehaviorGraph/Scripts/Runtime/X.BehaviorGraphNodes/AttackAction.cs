using DevelopProducts.BehaviorGraph.Runtime.Adaptor.InGame.Enemy;
using DevelopProducts.BehaviorGraph.Runtime.View.InGame;
using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;
namespace DevelopProducts.BehaviorGraph.Runtime.BehaviorGraphNodes
{

    [Serializable, GeneratePropertyBag]
    [NodeDescription(name: "Attack", story: "Agent Attacks", category: "Action", id: "ba60a0dc807026fa595c96836fca65e6")]
    public partial class AttackAction : Action
    {
        [SerializeReference] public BlackboardVariable<EnemyAIFacade> Facade;
        private EnemyAIFacade _facade;

        protected override Status OnStart()
        {
            _facade = Facade.ObjectValue as EnemyAIFacade;
            if (_facade is null)
            {
                throw new ArgumentNullException("_facade", "[Behavior Graph]ファサードがNULLです。");
            }
            _facade.Attack();
            return Status.Running;
        }

        protected override Status OnUpdate()
        {
            if ((_facade.CurrentStatus & EnumEnemyStatus.Aiming) == EnumEnemyStatus.Aiming
                || (_facade.CurrentStatus & EnumEnemyStatus.Attacking) == EnumEnemyStatus.Attacking)
            {
                Debug.Log("[Behavior Graph] EnemyStatus : " + _facade.CurrentStatus);
                // 攻撃行動中の場合
                return Status.Running;
            }
            else
            {
                // 攻撃行動完了の場合
                return Status.Success;
            }
        }

        protected override void OnEnd()
        {
        }
    }
}