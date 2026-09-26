using KillChord.Runtime.View.InGame.Enemy.AIFacade;
using System;
using Unity.Behavior;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.Enemy.BehaviorGraphNode.Condition
{
    /// <summary>
    ///     敵が攻撃中かを判定する Behavior Graph の条件ノード。
    /// </summary>
    [Serializable, Unity.Properties.GeneratePropertyBag]
    [Condition(name: "IsAttacking", story: "攻撃中である [Bool] [State]", category: "Conditions", id: "aaddd2fea642038ded13432c6882f4bd")]
    public partial class IsAttackingCondition : Unity.Behavior.Condition
    {
        [SerializeReference] public BlackboardVariable<EnemyStateFacade> State;
        [SerializeReference] public BlackboardVariable<bool> Bool;

        /// <summary>
        ///     攻撃中かが Bool の値と一致するかを返す。
        /// </summary>
        public override bool IsTrue()
        {
            if (State?.Value == null) return false;
            return State.Value.IsAttacking == Bool.Value;
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
