using KillChord.Runtime.View.InGame.Enemy.AIFacade;
using System;
using Unity.Behavior;

namespace KillChord.Runtime.View.InGame.Enemy.BehaviorGraphNode.Condition
{
    /// <summary>
    ///     敵が攻撃中かを判定する Behavior Graph の条件ノード。
    /// </summary>
    [Serializable, Unity.Properties.GeneratePropertyBag]
    [Condition(name: "IsAttacking", story: "攻撃中である [Bool] [State]", category: "Conditions", id: "aaddd2fea642038ded13432c6882f4bd")]
    public partial class IsAttackingCondition : StateConditionBase<EnemyStateFacade>
    {
        /// <summary>
        ///     攻撃中かを返す。
        /// </summary>
        protected override bool ReadState(EnemyStateFacade state)
        {
            return state.IsAttacking;
        }
    }
}
