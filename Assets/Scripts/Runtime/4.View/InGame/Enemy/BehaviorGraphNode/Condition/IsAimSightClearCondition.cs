using KillChord.Runtime.View.InGame.Enemy.AIFacade;
using System;
using Unity.Behavior;

namespace KillChord.Runtime.View.InGame.Enemy.BehaviorGraphNode.Condition
{
    /// <summary>
    ///     敵の照準の射線が通っているかを判定する Behavior Graph の条件ノード。
    /// </summary>
    [Serializable, Unity.Properties.GeneratePropertyBag]
    [Condition(name: "IsAimSightClear", story: "敵との間に遮蔽物がない [Bool] [State]", category: "Conditions", id: "fb7f79905d34791230533bf26dd2e567")]
    public partial class IsAimSightClearCondition : StateConditionBase<EnemyStateFacade>
    {
        /// <summary>
        ///     射線が通っているかを返す。
        /// </summary>
        protected override bool ReadState(EnemyStateFacade state)
        {
            return state.IsSightClearToAim;
        }
    }
}
