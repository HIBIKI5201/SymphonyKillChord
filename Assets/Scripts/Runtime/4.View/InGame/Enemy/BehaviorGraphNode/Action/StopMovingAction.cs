using KillChord.Runtime.View.InGame.Enemy.AIFacade;
using System;
using Unity.Behavior;
using Unity.Properties;

namespace KillChord.Runtime.View.InGame.Enemy.BehaviorGraphNode.Action
{
    /// <summary>
    ///     敵の移動を止める Behavior Graph のアクションノード。
    /// </summary>
    [Serializable, GeneratePropertyBag]
    [NodeDescription(name: "StopMoving", story: "移動を停止する [Movement] [State]", category: "Action", id: "72e36c342c9233772b0a01e15cd5b846")]
    public partial class StopMovingAction : StopMovingActionBase<EnemyMovementAIFacade, EnemyStateFacade>
    {
    }
}
