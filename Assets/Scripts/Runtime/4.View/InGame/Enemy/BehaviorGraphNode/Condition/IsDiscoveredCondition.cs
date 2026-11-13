using KillChord.Runtime.View.InGame.Enemy.AIFacade;
using System;
using Unity.Behavior;

namespace KillChord.Runtime.View.InGame.Enemy.BehaviorGraphNode.Condition
{
    /// <summary>
    ///     敵がプレイヤーを発見済みかを判定する Behavior Graph の条件ノード。
    /// </summary>
    [Serializable, Unity.Properties.GeneratePropertyBag]
    [Condition(name: "IsDiscovered", story: "プレイヤーを発見済みである [Bool] [State]", category: "Conditions", id: "b3f8a1d4e2c9407fa6b1c8d3e5f70a92")]
    public partial class IsDiscoveredCondition : StateConditionBase<EnemyStateFacade>
    {
        /// <summary>
        ///     発見済みかを返す。
        /// </summary>
        protected override bool ReadState(EnemyStateFacade state)
        {
            return state.IsDiscovered;
        }
    }
}
