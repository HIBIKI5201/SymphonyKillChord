using KillChord.Runtime.View.InGame.Enemy.AIFacade;
using System;
using Unity.Behavior;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.Enemy.BehaviorGraphNode.Condition
{
    /// <summary>
    ///     ターゲットが敵の攻撃射程内にいるかを判定する Behavior Graph の条件ノード。
    /// </summary>
    [Serializable, Unity.Properties.GeneratePropertyBag]
    [Condition(name: "IsTargetInAttackRange", story: "敵が攻撃範囲内にいる [Bool] [State]", category: "Conditions", id: "f089200575131990cf77ee4ef830d114")]
    public partial class IsTargetInAttackRangeCondition : Unity.Behavior.Condition
    {
        [SerializeReference] public BlackboardVariable<EnemyStateFacade> State;
        [SerializeReference] public BlackboardVariable<bool> Bool;

        /// <summary>
        ///     射程内にいるかが Bool の値と一致するかを返す。
        /// </summary>
        public override bool IsTrue()
        {
            if (State?.Value == null) return false;
            return State.Value.IsTargetInAttackRange == Bool.Value;
        }

        /// <summary>
        ///     条件評価の開始時の処理。現在は何もしない。
        /// </summary>
        public override void OnStart()
        {
        }

        /// <summary>
        ///     条件評価の終了時の処理。現在は何もしない。
        /// </summary>
        public override void OnEnd()
        {
        }
    }
}
