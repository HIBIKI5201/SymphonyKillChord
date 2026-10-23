using KillChord.Runtime.View.InGame.Enemy.BehaviorGraphNode.Condition;
using System;
using Unity.Behavior;

namespace KillChord.Runtime.View.InGame.Enemy
{
    /// <summary>
    ///     ボスの照準の射線が通っているかを判定する Behavior Graph の条件ノード。
    /// </summary>
    [Serializable, Unity.Properties.GeneratePropertyBag]
    [Condition(name: "BossIsAimSightClear", story: "敵との間に遮蔽物がない(Boss) [Bool] [State]", category: "Conditions/Boss", id: "18c7d9eafbfc827456bc89dea0fb2d78")]
    public partial class BossIsAimSightClearCondition : StateConditionBase<BossStateFacade>
    {
        /// <summary>
        ///     射線が通っているかを返す。
        /// </summary>
        protected override bool ReadState(BossStateFacade state)
        {
            return state.IsSightClearToAim;
        }
    }
}
