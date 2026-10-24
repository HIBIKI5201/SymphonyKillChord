using KillChord.Runtime.View.InGame.Enemy.AIFacade;
using System;
using Unity.Behavior;
using Unity.Properties;

namespace KillChord.Runtime.View.InGame.Enemy.BehaviorGraphNode.Action
{
    /// <summary>
    ///     敵を攻撃できる位置へ移動させる Behavior Graph のアクションノード。
    /// </summary>
    [Serializable, GeneratePropertyBag]
    [NodeDescription(name: "MoveToAttack", story: "攻撃可能な位置まで移動する [Movement] [State]", category: "Action", id: "8b82e763f6fed498af18c3983a2c822b")]
    public partial class MoveToAttackAction : MoveToAttackActionBase<EnemyMovementAIFacade, EnemyStateFacade>
    {
    }
}
