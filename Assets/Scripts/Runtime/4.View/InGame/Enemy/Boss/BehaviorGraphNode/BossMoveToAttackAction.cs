using KillChord.Runtime.View.InGame.Enemy.BehaviorGraphNode.Action;
using System;
using Unity.Behavior;
using Unity.Properties;

namespace KillChord.Runtime.View.InGame.Enemy
{
    /// <summary>
    ///     ボスを攻撃できる位置へ移動させる Behavior Graph のアクションノード。
    /// </summary>
    [Serializable, GeneratePropertyBag]
    [NodeDescription(name: "BossMoveToAttack", story: "攻撃可能な位置まで移動する(Boss) [Movement] [State]", category: "Action/Boss", id: "b2e1d3c4f5a6471890bc23de45fa6712")]
    public partial class BossMoveToAttackAction : MoveToAttackActionBase<BossMovementAIFacade, BossStateFacade>
    {
    }
}
