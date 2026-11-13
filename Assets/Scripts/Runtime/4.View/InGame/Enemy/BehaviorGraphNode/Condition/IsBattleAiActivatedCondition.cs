using KillChord.Runtime.View.InGame.Enemy.AIFacade;
using System;
using Unity.Behavior;

namespace KillChord.Runtime.View.InGame.Enemy.BehaviorGraphNode.Condition
{
    /// <summary>
    ///     敵の戦闘 AI が有効かを判定する Behavior Graph の条件ノード。
    /// </summary>
    [Serializable, Unity.Properties.GeneratePropertyBag]
    [Condition(name: "IsBattleAiActivated", story: "敵の戦闘AIが有効 [Bool] [State]", category: "Conditions", id: "e481ef1704416ac2b10bcdd0903055fe")]
    public partial class IsBattleAiActivatedCondition : StateConditionBase<EnemyStateFacade>
    {
        /// <summary>
        ///     戦闘 AI が有効かを返す。
        /// </summary>
        protected override bool ReadState(EnemyStateFacade state)
        {
            return state.IsBattleAiActivated;
        }
    }
}
