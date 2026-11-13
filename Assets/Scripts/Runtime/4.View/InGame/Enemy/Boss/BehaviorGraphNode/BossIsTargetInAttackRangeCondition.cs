using KillChord.Runtime.View.InGame.Enemy.BehaviorGraphNode.Condition;
using System;
using Unity.Behavior;

namespace KillChord.Runtime.View.InGame.Enemy
{
    /// <summary>
    ///     ターゲットがボスの攻撃射程内にいるかを判定する Behavior Graph の条件ノード。
    /// </summary>
    [Serializable, Unity.Properties.GeneratePropertyBag]
    [Condition(name: "BossIsTargetInAttackRange", story: "敵が攻撃範囲内にいる(Boss) [Bool] [State]", category: "Conditions/Boss", id: "e5b4a6c7d8f9504123ef56ab78cd9a45")]
    public partial class BossIsTargetInAttackRangeCondition : StateConditionBase<BossStateFacade>
    {
        /// <summary>
        ///     射程内にいるかを返す。
        /// </summary>
        protected override bool ReadState(BossStateFacade state)
        {
            return state.IsTargetInAttackRange;
        }
    }
}
