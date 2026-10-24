using KillChord.Runtime.View.InGame.Enemy.BehaviorGraphNode.Condition;
using System;
using Unity.Behavior;

namespace KillChord.Runtime.View.InGame.Enemy
{
    /// <summary>
    ///     ボスが攻撃中かを判定する Behavior Graph の条件ノード。
    /// </summary>
    [Serializable, Unity.Properties.GeneratePropertyBag]
    [Condition(name: "BossIsAttacking", story: "攻撃中である(Boss) [Bool] [State]", category: "Conditions/Boss", id: "07b6c8d9eaeb716345ab78cd9aef1c67")]
    public partial class BossIsAttackingCondition : StateConditionBase<BossStateFacade>
    {
        /// <summary>
        ///     攻撃中かを返す。
        /// </summary>
        protected override bool ReadState(BossStateFacade state)
        {
            return state.IsAttacking;
        }
    }
}
