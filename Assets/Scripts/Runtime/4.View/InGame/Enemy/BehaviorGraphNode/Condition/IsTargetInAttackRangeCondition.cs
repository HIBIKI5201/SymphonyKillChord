using KillChord.Runtime.View.InGame.Enemy.AIFacade;
using System;
using Unity.Behavior;

namespace KillChord.Runtime.View.InGame.Enemy.BehaviorGraphNode.Condition
{
    /// <summary>
    ///     ターゲットが敵の攻撃射程内にいるかを判定する Behavior Graph の条件ノード。
    /// </summary>
    [Serializable, Unity.Properties.GeneratePropertyBag]
    [Condition(name: "IsTargetInAttackRange", story: "敵が攻撃範囲内にいる [Bool] [State]", category: "Conditions", id: "f089200575131990cf77ee4ef830d114")]
    public partial class IsTargetInAttackRangeCondition : StateConditionBase<EnemyStateFacade>
    {
        /// <summary>
        ///     射程内にいるかを返す。
        /// </summary>
        protected override bool ReadState(EnemyStateFacade state)
        {
            return state.IsTargetInAttackRange;
        }
    }
}
