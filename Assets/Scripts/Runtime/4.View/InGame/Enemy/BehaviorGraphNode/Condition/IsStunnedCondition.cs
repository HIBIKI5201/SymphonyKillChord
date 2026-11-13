using KillChord.Runtime.View.InGame.Enemy.AIFacade;
using System;
using Unity.Behavior;

namespace KillChord.Runtime.View.InGame.Enemy.BehaviorGraphNode.Condition
{
    /// <summary>
    ///     敵が硬直中かを判定する Behavior Graph の条件ノード。
    /// </summary>
    [Serializable, Unity.Properties.GeneratePropertyBag]
    [Condition(name: "IsStunned", story: "スタン状態である [Bool] [State]", category: "Conditions", id: "842f5b1b693cb1d6b1e5202aa4bcfccc")]
    public partial class IsStunnedCondition : StateConditionBase<EnemyStateFacade>
    {
        /// <summary>
        ///     硬直中かを返す。
        /// </summary>
        protected override bool ReadState(EnemyStateFacade state)
        {
            return state.IsStunned;
        }
    }
}
