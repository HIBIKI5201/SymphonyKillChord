using KillChord.Runtime.View.InGame.Enemy.BehaviorGraphNode.Action;
using System;
using Unity.Behavior;
using Unity.Properties;

namespace KillChord.Runtime.View.InGame.Enemy
{
    /// <summary>
    ///     ボスにターゲットへの攻撃を行わせる Behavior Graph のアクションノード。
    /// </summary>
    [Serializable, GeneratePropertyBag]
    [NodeDescription(name: "BossAttackTarget", story: "対象を攻撃する(Boss) [Battle] [State]", category: "Action/Boss", id: "a1f0c2d3e4b5460789ab12cd34ef5601")]
    public partial class BossAttackTargetAction : AttackTargetActionBase<BossBattleAIFacade, BossStateFacade>
    {
    }
}
