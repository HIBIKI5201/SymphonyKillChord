using KillChord.Runtime.View.InGame.Enemy.BehaviorGraphNode.Action;
using System;
using Unity.Behavior;
using Unity.Properties;

namespace KillChord.Runtime.View.InGame.Enemy
{
    /// <summary>
    ///     ボスの移動を止める Behavior Graph のアクションノード。
    /// </summary>
    [Serializable, GeneratePropertyBag]
    [NodeDescription(name: "BossStopMoving", story: "移動を停止する(Boss) [Movement] [State]", category: "Action/Boss", id: "c3d2e4b5a6f7482901cd34ef56ab7823")]
    public partial class BossStopMovingAction : StopMovingActionBase<BossMovementAIFacade, BossStateFacade>
    {
    }
}
