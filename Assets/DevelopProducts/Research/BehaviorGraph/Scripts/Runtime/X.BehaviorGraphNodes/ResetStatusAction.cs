using DevelopProducts.BehaviorGraph.Runtime.View.InGame;
using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;
namespace DevelopProducts.BehaviorGraph.Runtime.BehaviorGraphNodes
{

    [Serializable, GeneratePropertyBag]
    [NodeDescription(name: "ResetStatus", story: "Reset Status", category: "Action", id: "d4c2617c8d7c70560f8b2a773af0ad12")]
    public partial class ResetStatusAction : Action
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
            _facade.ResetStatus();
            return Status.Success;
        }

        protected override Status OnUpdate()
        {
            return Status.Success;
        }

        protected override void OnEnd()
        {
        }
    }
}